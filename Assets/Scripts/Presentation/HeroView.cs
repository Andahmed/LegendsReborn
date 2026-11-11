using UnityEngine;
using UnityEngine.UI;
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
        private int maxHp;
        private int currentHp;
        private Image healthFill;
        private Transform healthBarRoot;

        public int Team { get; private set; }
        public int Slot { get; private set; }

        public void Initialize(UnitSpec spec, string modelId, int team, int slot, bool monster)
        {
            Team = team;
            Slot = slot;
            maxHp = Mathf.Max(1, spec.Hp);
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

            if (isTarget) ApplyHealthEvent(battleEvent);

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
            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, .78f);

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

        private void ApplyHealthEvent(BattleEvent battleEvent)
        {
            if (battleEvent.Type == BattleEventType.Death)
                currentHp = 0;
            else if (battleEvent.Type == BattleEventType.Heal)
                currentHp = Mathf.Min(maxHp, currentHp + Mathf.Max(0, battleEvent.Amount));
            else if (battleEvent.Type == BattleEventType.Attack ||
                battleEvent.Type == BattleEventType.Skill ||
                battleEvent.Type == BattleEventType.Ultimate)
                currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, battleEvent.Amount));
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Clamp01((float)currentHp / Mathf.Max(1, maxHp));
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
            if (healthBarRoot != null && Camera.main != null)
                healthBarRoot.forward = Camera.main.transform.forward;
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
