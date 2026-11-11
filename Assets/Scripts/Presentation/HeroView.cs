using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Presentation
{
    /// <summary>
    /// 3D character presentation. Uses imported FBX clips through Playables when available,
    /// while preserving the lightweight procedural reactions as a safe fallback.
    /// </summary>
    public sealed class HeroView : MonoBehaviour
    {
        private Transform modelRoot;
        private Animator animator;
        private PlayableGraph animationGraph;
        private AnimationMixerPlayable animationMixer;
        private AnimationClipPlayable idlePlayable;
        private AnimationClipPlayable actionPlayable;
        private AnimationClip idleClip;
        private AnimationClip walkClip;
        private AnimationClip flyingClip;
        private AnimationClip attackClip;
        private AnimationClip hitClip;
        private AnimationClip deathClip;
        private bool graphReady;
        private bool actionPlaying;
        private bool actionFading;
        private bool actionLoops;
        private float actionElapsed;
        private float actionBlend;

        private Vector3 baseScale;
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private float motion;
        private float attackLunge;
        private float hitShake;
        private bool dead;

        public int Team { get; private set; }
        public int Slot { get; private set; }

        public void Initialize(UnitSpec spec, string modelId, int team, int slot, bool monster)
        {
            Team = team;
            Slot = slot;

            string folder = monster ? "BattleMonsters/" : "HomeHeroes/";
            GameObject prefab = Resources.Load<GameObject>(folder + modelId);
            if (prefab != null)
            {
                GameObject model = Instantiate(prefab, transform);
                model.name = modelId + " Model";
                modelRoot = model.transform;

                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    Bounds bounds = GetCombinedBounds(renderers);
                    float desiredHeight = monster ? 2.7f : 2.35f;
                    float scale = desiredHeight / Mathf.Max(.01f, bounds.size.y);

                    // Small deterministic variation keeps repeated archetypes from looking cloned.
                    float variation = 1f + (StableVariant(spec != null ? spec.Id : modelId, slot) - 1.5f) * .035f;
                    modelRoot.localScale = Vector3.one * scale * variation;

                    // Align the imported mesh feet with the battle slot even when its pivot is not at ground level.
                    bounds = GetCombinedBounds(renderers);
                    modelRoot.position += Vector3.up * (transform.position.y - bounds.min.y);
                    ApplyHeroTint(renderers, spec != null ? spec.Id : modelId, team, slot);
                }

                animator = model.GetComponentInChildren<Animator>(true);
                LoadAnimationClips(modelId, monster);
                TryCreatePlayableGraph();
            }
            else
            {
                // Temporary fallback remains visible until the humanoid FBX files are committed and imported.
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Model Missing - Temporary Silhouette";
                body.transform.SetParent(transform, false);
                body.transform.localScale = new Vector3(.8f, 1.05f, .55f);
                Renderer renderer = body.GetComponent<Renderer>();
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material material = new Material(shader);
                material.color = team == 0 ? new Color(.12f, .68f, .65f) : new Color(.62f, .2f, .18f);
                renderer.sharedMaterial = material;
                Destroy(body.GetComponent<Collider>());
                modelRoot = body.transform;
            }

            baseScale = modelRoot != null ? modelRoot.localScale : Vector3.one;
            basePosition = modelRoot != null ? modelRoot.localPosition : Vector3.zero;
            baseRotation = modelRoot != null ? modelRoot.localRotation : Quaternion.identity;
            transform.localRotation = Quaternion.Euler(0f, team == 0 ? 0f : 180f, 0f);
        }

        private static Bounds GetCombinedBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static float StableVariant(string id, int slot)
        {
            unchecked
            {
                int hash = 17 + slot * 31;
                if (!string.IsNullOrEmpty(id))
                    for (int i = 0; i < id.Length; i++) hash = hash * 31 + id[i];
                return ((hash & 0x7fffffff) % 4);
            }
        }

        private static void ApplyHeroTint(Renderer[] renderers, string id, int team, int slot)
        {
            float variant = StableVariant(id, slot);
            Color teamBase = team == 0 ? new Color(.72f, .9f, 1f) : new Color(1f, .72f, .68f);
            Color[] accents =
            {
                new Color(1f, 1f, 1f),
                new Color(1f, .91f, .76f),
                new Color(.78f, .9f, 1f),
                new Color(.88f, .8f, 1f)
            };
            Color tint = Color.Lerp(teamBase, accents[(int)variant], .34f);
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.GetPropertyBlock(block);
                block.SetColor("_Color", tint);
                block.SetColor("_BaseColor", tint);
                renderer.SetPropertyBlock(block);
            }
        }

        private void LoadAnimationClips(string modelId, bool monster)
        {
            // FBX clips are sub-assets of the model. Never scan the whole Resources folder:
            // doing so can select another hero's or a monster's animation.
            string folder = (monster ? "BattleMonsters/" : "HomeHeroes/") + modelId;
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(folder);

            idleClip = FindExactClip(clips, "Idle");
            walkClip = FindExactClip(clips, "Run", "Walk");
            flyingClip = FindExactClip(clips, "Bat_Flying", "Flying");
            if (idleClip == null) idleClip = flyingClip;

            // Compare complete clip leaf names, not substrings. This prevents Idle_Attacking
            // from being selected before Sword_Attack.
            attackClip = FindExactClip(clips, "Sword_Attack", "Bat_Attack", "Attack", "Skill", "Cast");
            hitClip = FindExactClip(clips, "RecieveHit", "ReceiveHit", "Bat_Hit", "Hit", "Hurt");
            deathClip = FindExactClip(clips, "Death", "Bat_Death", "Die");
        }

        private static AnimationClip FindExactClip(AnimationClip[] clips, params string[] acceptedNames)
        {
            if (clips == null || acceptedNames == null) return null;
            for (int n = 0; n < acceptedNames.Length; n++)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    AnimationClip clip = clips[i];
                    if (clip == null) continue;
                    string leaf = clip.name;
                    int separator = leaf.LastIndexOf('|');
                    if (separator >= 0 && separator < leaf.Length - 1) leaf = leaf.Substring(separator + 1);
                    separator = leaf.LastIndexOf(':');
                    if (separator >= 0 && separator < leaf.Length - 1) leaf = leaf.Substring(separator + 1);
                    if (string.Equals(leaf, acceptedNames[n], StringComparison.OrdinalIgnoreCase))
                        return clip;
                }
            }
            return null;
        }

        /// <summary>Switches between the model's looping walk/fly clip and its idle loop.</summary>
        public void SetMoving(bool moving)
        {
            if (!graphReady || dead) return;
            AnimationClip locomotion = flyingClip != null ? flyingClip : walkClip;
            if (moving && locomotion != null) PlayClip(locomotion, true);
            else if (!moving && actionPlaying && actionLoops)
            {
                actionPlaying = false;
                actionLoops = false;
                actionFading = true;
            }
        }

        private void TryCreatePlayableGraph()
        {
            if (animator == null || animator.avatar == null || idleClip == null) return;

            animationGraph = PlayableGraph.Create("HeroView_" + Team + "_" + Slot);
            animationGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            animationMixer = AnimationMixerPlayable.Create(animationGraph, 2, true);

            idlePlayable = AnimationClipPlayable.Create(animationGraph, idleClip);
            idlePlayable.SetLoopTime(true);
            animationGraph.Connect(idlePlayable, 0, animationMixer, 0);
            animationMixer.SetInputWeight(0, 1f);
            animationMixer.SetInputWeight(1, 0f);

            AnimationPlayableOutput output = AnimationPlayableOutput.Create(animationGraph, "Character Animation", animator);
            output.SetSourcePlayable(animationMixer);
            animationGraph.Play();
            graphReady = true;
        }

        private void PlayClip(AnimationClip clip, bool loop = false)
        {
            if (!graphReady || clip == null || dead) return;

            if (actionPlayable.IsValid()) animationGraph.DestroyPlayable(actionPlayable);
            actionPlayable = AnimationClipPlayable.Create(animationGraph, clip);
            actionPlayable.SetLoopTime(loop);
            actionPlayable.SetDuration(loop ? double.PositiveInfinity : clip.length);
            animationGraph.Connect(actionPlayable, 0, animationMixer, 1);
            actionPlayable.SetTime(0d);
            actionElapsed = 0f;
            actionBlend = 0f;
            actionFading = false;
            actionLoops = loop;
            actionPlaying = true;
        }

        public void PlayEvent(BattleEvent battleEvent)
        {
            bool isTarget = battleEvent.TargetTeam == Team && battleEvent.TargetSlot == Slot;
            bool isSource = battleEvent.SourceTeam == Team && battleEvent.SourceSlot == Slot;

            if (isTarget && battleEvent.Type == BattleEventType.Death)
            {
                dead = true;
                motion = .35f;
                attackLunge = 0f;
                hitShake = .1f;
                if (deathClip != null) PlayDeathClip();
                return;
            }

            if (isTarget && (battleEvent.Type == BattleEventType.Attack ||
                battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate ||
                battleEvent.Type == BattleEventType.Heal))
            {
                motion = .22f;
                hitShake = battleEvent.Type == BattleEventType.Ultimate ? .20f : .12f;
                if (hitClip != null) PlayClip(hitClip);
            }

            if (isSource && (battleEvent.Type == BattleEventType.Attack ||
                battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate))
            {
                motion = .16f;
                attackLunge = battleEvent.Type == BattleEventType.Ultimate ? .95f :
                    (battleEvent.Type == BattleEventType.Skill ? .68f : .46f);

                if (attackClip != null) PlayClip(attackClip);
                else if (animator != null && animator.runtimeAnimatorController != null)
                {
                    if (HasParameter("Attack")) animator.SetTrigger("Attack");
                    else if (HasParameter("attack")) animator.SetTrigger("attack");
                }
            }
        }

        private void PlayDeathClip()
        {
            // Death is allowed after dead=true; unlike other actions it must finish once.
            if (!graphReady || deathClip == null) return;
            if (actionPlayable.IsValid()) animationGraph.DestroyPlayable(actionPlayable);
            actionPlayable = AnimationClipPlayable.Create(animationGraph, deathClip);
            actionPlayable.SetLoopTime(false);
            actionPlayable.SetDuration(deathClip.length);
            animationGraph.Connect(actionPlayable, 0, animationMixer, 1);
            actionPlayable.SetTime(0d);
            actionElapsed = 0f;
            actionBlend = 0f;
            actionFading = false;
            actionLoops = false;
            actionPlaying = true;
        }

        private bool HasParameter(string parameterName)
        {
            if (animator == null) return false;
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].name == parameterName &&
                    parameters[i].type == AnimatorControllerParameterType.Trigger) return true;
            return false;
        }

        private void Update()
        {
            if (modelRoot == null) return;

            if (actionPlaying || actionFading)
            {
                if (actionPlaying)
                {
                    actionElapsed += Time.deltaTime;
                    actionBlend = Mathf.MoveTowards(actionBlend, 1f, Time.deltaTime * 10f);
                    if (actionPlayable.IsValid() && actionPlayable.GetAnimationClip() != null &&
                        !actionLoops && actionElapsed >= actionPlayable.GetAnimationClip().length)
                    {
                        if (dead)
                        {
                            actionPlayable.SetTime(actionPlayable.GetAnimationClip().length);
                            actionPlayable.SetSpeed(0d);
                            actionPlaying = false;
                            actionBlend = 1f;
                        }
                        else
                        {
                            actionPlaying = false;
                            actionFading = true;
                        }
                    }
                }
                else if (actionFading)
                {
                    actionBlend = Mathf.MoveTowards(actionBlend, 0f, Time.deltaTime * 8f);
                    if (actionBlend <= .001f)
                    {
                        actionBlend = 0f;
                        actionFading = false;
                    }
                }

                animationMixer.SetInputWeight(0, 1f - actionBlend);
                animationMixer.SetInputWeight(1, actionBlend);
            }

            float bob = Mathf.Sin(Time.time * 2.2f + Slot) * .035f;
            motion = Mathf.MoveTowards(motion, 0f, Time.deltaTime);
            attackLunge = Mathf.MoveTowards(attackLunge, 0f, Time.deltaTime * 3.4f);
            hitShake = Mathf.MoveTowards(hitShake, 0f, Time.deltaTime * 3.8f);

            float pulse = 1f + motion * 1.4f;
            modelRoot.localScale = baseScale * pulse;

            if (dead)
            {
                if (!graphReady || deathClip == null)
                {
                    modelRoot.localPosition = basePosition + Vector3.down * .14f;
                    modelRoot.localRotation = Quaternion.Lerp(modelRoot.localRotation,
                        baseRotation * Quaternion.Euler(0f, 0f, 72f), Time.deltaTime * 5f);
                }
                return;
            }

            float shake = Mathf.Sin(Time.time * 42f + Slot) * hitShake;
            modelRoot.localPosition = basePosition + new Vector3(shake, bob, attackLunge);
            modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, baseRotation, Time.deltaTime * 8f);
        }

        private void OnDestroy()
        {
            if (animationGraph.IsValid()) animationGraph.Destroy();
        }
    }
}