using UnityEngine;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Presentation
{
    /// <summary>3D character presentation with forward attack lunges, hit reactions and defeat fall.</summary>
    public sealed class HeroView : MonoBehaviour
    {
        private Transform modelRoot;
        private Animator animator;
        private Vector3 baseScale;
        private Vector3 basePosition;
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
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                    float desiredHeight = monster ? 2.7f : 2.35f;
                    float scale = desiredHeight / Mathf.Max(.01f, bounds.size.y);
                    modelRoot.localScale = Vector3.one * scale;
                    modelRoot.localPosition = Vector3.zero;
                }
                animator = model.GetComponentInChildren<Animator>();
            }
            else
            {
                // Temporary fallback is retained for local tests until the humanoid FBX importer runs.
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
            transform.localRotation = Quaternion.Euler(0f, team == 0 ? 0f : 180f, 0f);
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
                return;
            }

            if (isTarget && (battleEvent.Type == BattleEventType.Attack ||
                battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate ||
                battleEvent.Type == BattleEventType.Heal))
            {
                motion = .22f;
                hitShake = battleEvent.Type == BattleEventType.Ultimate ? .20f : .12f;
            }

            if (isSource && (battleEvent.Type == BattleEventType.Attack ||
                battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate))
            {
                motion = .16f;
                attackLunge = battleEvent.Type == BattleEventType.Ultimate ? .95f :
                    (battleEvent.Type == BattleEventType.Skill ? .68f : .46f);

                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    if (HasParameter("Attack")) animator.SetTrigger("Attack");
                    else if (HasParameter("attack")) animator.SetTrigger("attack");
                }
            }
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
            float bob = Mathf.Sin(Time.time * 2.2f + Slot) * .035f;
            motion = Mathf.MoveTowards(motion, 0f, Time.deltaTime);
            attackLunge = Mathf.MoveTowards(attackLunge, 0f, Time.deltaTime * 3.4f);
            hitShake = Mathf.MoveTowards(hitShake, 0f, Time.deltaTime * 3.8f);

            float pulse = 1f + motion * 1.4f;
            modelRoot.localScale = baseScale * pulse;

            if (dead)
            {
                modelRoot.localPosition = basePosition + Vector3.down * .14f;
                modelRoot.localRotation = Quaternion.Lerp(modelRoot.localRotation,
                    Quaternion.Euler(0f, 0f, 72f), Time.deltaTime * 5f);
                return;
            }

            float shake = Mathf.Sin(Time.time * 42f + Slot) * hitShake;
            modelRoot.localPosition = basePosition + new Vector3(shake, bob, attackLunge);
            modelRoot.localRotation = Quaternion.Lerp(modelRoot.localRotation,
                Quaternion.identity, Time.deltaTime * 8f);
        }
    }
}
