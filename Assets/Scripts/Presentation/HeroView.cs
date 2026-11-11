using UnityEngine;
using SandsOfLegends.Combat;

namespace SandsOfLegends.Presentation
{
    public sealed class HeroView : MonoBehaviour
    {
        private Transform modelRoot;
        private Animator animator;
        private Vector3 baseScale;
        private float motion;
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
                // Temporary low-poly fallback only. Install the source FBX models to replace it.
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
            transform.localRotation = Quaternion.Euler(0f, team == 0 ? 0f : 180f, 0f);
        }

        public void PlayEvent(BattleEvent battleEvent)
        {
            if (battleEvent.TargetTeam == Team && battleEvent.TargetSlot == Slot && battleEvent.Type == BattleEventType.Death)
            {
                dead = true;
                motion = .35f;
                return;
            }
            if (battleEvent.TargetTeam == Team && battleEvent.TargetSlot == Slot &&
                (battleEvent.Type == BattleEventType.Attack || battleEvent.Type == BattleEventType.Skill ||
                 battleEvent.Type == BattleEventType.Ultimate || battleEvent.Type == BattleEventType.Heal))
            {
                motion = .22f;
                if (battleEvent.Type == BattleEventType.Death) dead = true;
            }
            if (battleEvent.SourceTeam == Team && battleEvent.SourceSlot == Slot &&
                (battleEvent.Type == BattleEventType.Attack || battleEvent.Type == BattleEventType.Skill ||
                 battleEvent.Type == BattleEventType.Ultimate))
            {
                motion = .16f;
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
                if (parameters[i].name == parameterName && parameters[i].type == AnimatorControllerParameterType.Trigger) return true;
            return false;
        }

        private void Update()
        {
            if (modelRoot == null) return;
            float bob = Mathf.Sin(Time.time * 2.2f + Slot) * .035f;
            if (motion > 0f) motion = Mathf.MoveTowards(motion, 0f, Time.deltaTime);
            float pulse = 1f + motion * 1.4f;
            modelRoot.localScale = baseScale * pulse;
            if (!dead) modelRoot.localPosition = new Vector3(0f, bob, 0f);
            else modelRoot.localRotation = Quaternion.Euler(0f, 0f, 72f);
        }
    }
}
