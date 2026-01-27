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
        topDownCamera.enabled = true;
    }

    public void EnableMainCamera()
    {
        Debug.Log("Main Camera Enable");
        mainCamera.enabled = true;
        topDownCamera.enabled = false;
    }
}
