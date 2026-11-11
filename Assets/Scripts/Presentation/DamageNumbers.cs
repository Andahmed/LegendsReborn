using UnityEngine;

namespace SandsOfLegends.Presentation
{
    public sealed class DamageNumbers : MonoBehaviour
    {
        public void Spawn(Vector3 position, int amount, bool heal, bool critical)
        {
            GameObject go = new GameObject("Floating Combat Number");
            go.transform.position = position + Vector3.up * 1.8f;
            TextMesh label = go.AddComponent<TextMesh>();
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch (System.ArgumentException) { }
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch (System.ArgumentException) { }
            }
            label.font = font;
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (label.font != null && meshRenderer != null) meshRenderer.sharedMaterial = label.font.material;
            label.text = (heal ? "+" : "-") + amount.ToString("N0");
            label.fontSize = critical ? 72 : 56;
            label.characterSize = .075f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = heal ? new Color(.25f, 1f, .62f) : (critical ? new Color(1f, .78f, .2f) : Color.white);
            go.AddComponent<FloatingDamageNumber>();
        }
    }

    public sealed class FloatingDamageNumber : MonoBehaviour
    {
        private float age;
        private TextMesh label;

        private void Awake() { label = GetComponent<TextMesh>(); }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime * 1.1f;
            if (Camera.main != null) transform.forward = Camera.main.transform.forward;
            if (label != null)
            {
                Color color = label.color;
                color.a = Mathf.Clamp01(1f - age / 1.05f);
                label.color = color;
            }
            if (age >= 1.05f) Destroy(gameObject);
        }
    }
}
