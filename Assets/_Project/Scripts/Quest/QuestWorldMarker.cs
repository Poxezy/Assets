using UnityEngine;

namespace MetaEdu.Quest
{
    /// <summary>Simple gold beacon over quest target. Runtime-built primitives.</summary>
    public class QuestWorldMarker : MonoBehaviour
    {
        Transform beam;
        Transform orb;
        Light pointLight;
        bool visible;
        float pulse;

        void Awake()
        {
            Build();
            Hide();
        }

        void Build()
        {
            // Pole
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "MarkerPole";
            pole.transform.SetParent(transform, false);
            pole.transform.localScale = new Vector3(0.08f, 1.1f, 0.08f);
            pole.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            Object.Destroy(pole.GetComponent<Collider>());
            ApplyGoldMat(pole.GetComponent<Renderer>(), 0.55f);
            beam = pole.transform;

            // Orb
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "MarkerOrb";
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * 0.35f;
            sphere.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            Object.Destroy(sphere.GetComponent<Collider>());
            ApplyGoldMat(sphere.GetComponent<Renderer>(), 1f);
            orb = sphere.transform;

            // Light
            var lightGo = new GameObject("MarkerLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            pointLight = lightGo.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = UITheme.Gold;
            pointLight.intensity = 1.4f;
            pointLight.range = 6f;
        }

        static void ApplyGoldMat(Renderer r, float intensity)
        {
            if (r == null) return;
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            if (sh == null) return;
            var mat = new Material(sh);
            Color c = UITheme.Gold * intensity;
            c.a = 1f;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.color = c;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", UITheme.Gold * 0.8f);
            }
            r.material = mat;
        }

        public void SetTarget(Vector3 worldPos)
        {
            transform.position = worldPos;
            if (!visible)
            {
                visible = true;
                SetActiveVisuals(true);
            }
        }

        public void Hide()
        {
            if (!visible && gameObject.activeSelf)
            {
                // keep inactive
            }
            visible = false;
            SetActiveVisuals(false);
        }

        void SetActiveVisuals(bool on)
        {
            if (beam != null) beam.gameObject.SetActive(on);
            if (orb != null) orb.gameObject.SetActive(on);
            if (pointLight != null) pointLight.enabled = on;
        }

        void Update()
        {
            if (!visible || orb == null) return;
            pulse += Time.unscaledDeltaTime * 2.5f;
            float s = 0.32f + Mathf.Sin(pulse) * 0.05f;
            orb.localScale = Vector3.one * s;
            if (pointLight != null)
                pointLight.intensity = 1.2f + Mathf.Sin(pulse) * 0.35f;
        }
    }
}
