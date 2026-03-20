using UnityEngine;

public class DiscoLightManager : MonoBehaviour
{
    public static DiscoLightManager Instance;

    [Header("Lights")]
    public Light[] directionalLights;
    public Light[] discoLights;

    [Header("Ambient")]
    public Color normalAmbient = Color.white;
    public Color discoAmbient = Color.black;

    public float normalIntensity = 1f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        // reset every time scene starts
        SetDiscoMode(false);
    }

    public void SetDiscoMode(bool enable)
    {
        //  Ambient change
        RenderSettings.ambientLight = enable ? discoAmbient : normalAmbient;

        // Directional lights
        foreach (var light in directionalLights)
        {
            if (light)
                light.intensity = enable ? 0f : normalIntensity;
        }

        //  Disco lights control
        foreach (var light in discoLights)
        {
            if (light)
            {
                var controller = light.GetComponent<DiscoLightController>();

                if (enable)
                {
                    light.gameObject.SetActive(true);

                    if (controller != null)
                        controller.StartDisco(); //  start movement  color
                }
                else
                {
                    if (controller != null)
                        controller.StopDisco();

                    light.gameObject.SetActive(false);
                }
            }
        }
    }
}