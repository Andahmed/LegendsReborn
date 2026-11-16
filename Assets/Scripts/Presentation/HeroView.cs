using System;
using UnityEngine;
using UnityEngine.UI;
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
        private int maxHp;
        private int currentHp;
        private Image healthFill;
        private Transform healthBarRoot;
        private Material teamMarkerMaterial;

        public int Team { get; private set; }
        public int Slot { get; private set; }

        public void Initialize(UnitSpec spec, string modelId, int team, int slot, bool monster)
        {
            Team = team;
            Slot = slot;
            maxHp = Mathf.Max(1, spec != null ? spec.Hp : 1);
            currentHp = maxHp;
            CreateHealthBar();

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
                    // Keep imported material and texture colors intact; team identity uses a ground marker.
                }

                animator = model.GetComponentInChildren<Animator>(true);
                if (animator == null) animator = model.AddComponent<Animator>();
                if (animator.avatar == null)
                {
                    Avatar[] avatars = Resources.LoadAll<Avatar>(folder + modelId);
                    if (avatars.Length > 0) animator.avatar = avatars[0];
                }
                LoadAnimationClips(modelId, monster);
                TryCreatePlayableGraph();
            }
            else
            {
                Debug.LogError("[HeroView] Missing required FBX model at Resources/" + folder + modelId +
                    ". Run Legends Reborn > Art > Install MVP Art and reimport the FBX files. No primitive character fallback is created.");
                return;
            }

            baseScale = modelRoot != null ? modelRoot.localScale : Vector3.one;
            basePosition = modelRoot != null ? modelRoot.localPosition : Vector3.zero;
            baseRotation = modelRoot != null ? modelRoot.localRotation : Quaternion.identity;
            CreateTeamMarker();
            transform.localRotation = Quaternion.Euler(0f, team == 0 ? 0f : 180f, 0f);
        }

        private void CreateHealthBar()
        {
            GameObject canvasObject = new GameObject("HP Bar", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            healthBarRoot = canvasObject.transform;
            healthBarRoot.localPosition = new Vector3(0f, 2.9f, 0f);
            healthBarRoot.localScale = Vector3.one * .01f;
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 20;
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(120f, 12f);
            GameObject backgroundObject = new GameObject("HP Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            backgroundObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, .78f);
            GameObject fillObject = new GameObject("HP Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(backgroundObject.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            healthFill = fillObject.GetComponent<Image>();
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = 0;
            healthFill.color = Team == 0 ? new Color(.18f, .82f, .62f) : new Color(.92f, .28f, .24f);
            UpdateHealthBar();
        }

        private void CreateTeamMarker()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Team Ground Marker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(0f, .025f, 0f);
            marker.transform.localScale = new Vector3(1.12f, .018f, 1.12f);
            Collider collider = marker.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = marker.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader != null)
            {
                teamMarkerMaterial = new Material(shader);
                teamMarkerMaterial.color = Team == 0 ? new Color(.08f, .76f, .72f) : new Color(.88f, .22f, .2f);
                renderer.sharedMaterial = teamMarkerMaterial;
            }
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void ApplyHealthEvent(BattleEvent battleEvent)
        {
            if (battleEvent.Type == BattleEventType.Death) currentHp = 0;
            else if (battleEvent.Type == BattleEventType.Heal)
                currentHp = Mathf.Min(maxHp, currentHp + Mathf.Max(0, battleEvent.Amount));
            else if (battleEvent.Type == BattleEventType.Attack || battleEvent.Type == BattleEventType.Skill ||
                     battleEvent.Type == BattleEventType.Ultimate)
                currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, battleEvent.Amount));
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Clamp01((float)currentHp / Mathf.Max(1, maxHp));
        }

        private void SpawnImpact(BattleEvent battleEvent)
        {
            Color color = battleEvent.Type == BattleEventType.Heal ? new Color(.25f, 1f, .58f) :
                (battleEvent.Type == BattleEventType.Ultimate ? new Color(1f, .72f, .22f) :
                (battleEvent.Crit ? new Color(1f, .34f, .16f) : new Color(.42f, .82f, 1f)));
            GameObject effect = new GameObject("Combat Impact VFX");
            effect.transform.position = transform.position + Vector3.up * 1.15f;
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.duration = .45f;
            main.loop = false;
            main.maxParticles = 24;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.22f, .48f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(.055f, .14f);
            main.startColor = new ParticleSystem.MinMaxGradient(color);
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)10, (short)16) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .16f;
            particles.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.Billboard;
            particles.Play();
            Destroy(effect, 1.1f);
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
            if (isTarget) ApplyHealthEvent(battleEvent);

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
                SpawnImpact(battleEvent);
            }
            if (isTarget && battleEvent.Type == BattleEventType.Heal) SpawnImpact(battleEvent);

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
            if (healthBarRoot != null && Camera.main != null) healthBarRoot.forward = Camera.main.transform.forward;

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
            if (teamMarkerMaterial != null) Destroy(teamMarkerMaterial);
        }
    }
}