using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Align ambient + key/fill lights per scene for stable, comfortable look.
/// Book pickup lights (short range) left alone.
/// </summary>
public class RoomLightingBootstrap : MonoBehaviour
{
    /// <summary>Call from GameplaySceneSetup — no RuntimeInitialize auto-run.</summary>
    public static void ApplyForActiveScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "Leaderboard")
            return;

        if (scene == "classroom")
            ApplyClassroom();
        else if (scene == "campusyard" || scene == "MainScene")
            ApplyOutdoor(scene == "campusyard");
    }

    void Start()
    {
        ApplyForActiveScene();
    }

    static void ApplyClassroom()
    {
        // Soft indoor trilight — no skybox wash
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.74f, 0.76f, 0.80f);
        RenderSettings.ambientEquatorColor = new Color(0.50f, 0.48f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.24f, 0.22f, 0.20f);
        RenderSettings.ambientIntensity = 1.1f;
        RenderSettings.fog = false;
        RenderSettings.reflectionIntensity = 0.6f;

        Light key = null;

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null) continue;
            string n = light.gameObject.name;

            // Local book FX — keep short warm pulse
            if (n.IndexOf("Book", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                light.intensity = Mathf.Min(light.intensity, 1.4f);
                light.range = Mathf.Min(light.range, 3.5f);
                light.shadows = LightShadows.None;
                continue;
            }

            // Door beacon — leave modest
            if (n.IndexOf("Door", System.StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("Beacon", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                light.intensity = Mathf.Min(light.intensity, 1.4f);
                light.range = Mathf.Min(light.range, 6f);
                light.shadows = LightShadows.None;
                continue;
            }

            if (light.type == LightType.Directional)
            {
                key = light;
                light.intensity = 1.3f;
                light.color = new Color(1f, 0.96f, 0.90f);
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.75f;
            }
            else if (light.type == LightType.Point)
            {
                // Room fill only — no flood
                if (n.IndexOf("Center", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    light.intensity = 0.95f;
                    light.range = 14f;
                }
                else
                {
                    light.intensity = 0.7f;
                    light.range = 12f;
                }
                light.color = new Color(1f, 0.96f, 0.90f);
                light.shadows = LightShadows.None;
                light.bounceIntensity = 0.5f;
            }
        }

        if (key != null)
            RenderSettings.sun = key;
    }

    static void ApplyOutdoor(bool brightCampus)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = brightCampus
            ? new Color(0.78f, 0.84f, 0.92f)
            : new Color(0.82f, 0.86f, 0.92f);
        RenderSettings.ambientEquatorColor = new Color(0.55f, 0.54f, 0.52f);
        RenderSettings.ambientGroundColor = new Color(0.28f, 0.30f, 0.28f);
        RenderSettings.ambientIntensity = brightCampus ? 1.15f : 1.1f;
        RenderSettings.fog = false;
        RenderSettings.reflectionIntensity = 0.75f;

        Light key = null;
        Light fill = null;

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null) continue;
            string n = light.gameObject.name;

            // Book / door beacon local FX
            if (light.type == LightType.Point)
            {
                if (n.IndexOf("Book", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    light.intensity = Mathf.Min(light.intensity, 1.4f);
                    light.range = Mathf.Min(light.range, 3.5f);
                    light.shadows = LightShadows.None;
                }
                else if (n.IndexOf("Door", System.StringComparison.OrdinalIgnoreCase) >= 0
                         || n.IndexOf("Beacon", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    light.intensity = Mathf.Min(light.intensity, 1.6f);
                    light.range = Mathf.Min(light.range, 8f);
                    light.shadows = LightShadows.None;
                }
                continue;
            }

            if (light.type != LightType.Directional) continue;

            if (n.IndexOf("Fill", System.StringComparison.OrdinalIgnoreCase) >= 0)
                fill = light;
            else if (key == null)
                key = light;
        }

        if (key != null)
        {
            key.intensity = brightCampus ? 1.85f : 1.55f;
            key.color = new Color(1f, 0.96f, 0.88f);
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.8f;
            RenderSettings.sun = key;
        }

        if (fill != null)
        {
            fill.intensity = brightCampus ? 0.4f : 0.35f;
            fill.color = new Color(0.72f, 0.80f, 0.95f);
            fill.shadows = LightShadows.None;
        }
    }
}

