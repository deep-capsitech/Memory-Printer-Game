using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Camera mainCamera;
    public Camera topDownCamera;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        EnableMainCamera();
    }

    public void EnableTopCamera()
    {
        Debug.Log("Top Camera Enable");
        mainCamera.enabled = false;
        mainCamera.tag = "Untagged";

        topDownCamera.enabled = true;
        topDownCamera.tag = "MainCamera";
    }

    public void EnableMainCamera()
    {
        Debug.Log("Main Camera Enable");
        topDownCamera.enabled = false;
        topDownCamera.tag = "Untagged";

        mainCamera.enabled = true;
        mainCamera.tag = "MainCamera";
    }
}
