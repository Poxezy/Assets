using UnityEngine;

[RequireComponent(typeof(Light))]
public class ItemLightPulse : MonoBehaviour
{
    [SerializeField] private float minimumIntensity = 1f;
    [SerializeField] private float maximumIntensity = 3f;
    [SerializeField] private float pulseSpeed = 2f;

    private Light itemLight;

    private void Awake()
    {
        itemLight = GetComponent<Light>();
    }

    private void Update()
    {
        float value =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        itemLight.intensity = Mathf.Lerp(
            minimumIntensity,
            maximumIntensity,
            value
        );
    }
}