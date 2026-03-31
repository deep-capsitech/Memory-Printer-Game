using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    public static bool IsFirebaseReady { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
        {
            var status = task.Result;

            if (status == DependencyStatus.Available)
            {
                IsFirebaseReady = true;
                Debug.Log("Firebase Ready");
            }
            else
            {
                Debug.LogError("Firebase failed: " + status);
                IsFirebaseReady = false;
            }
        });
    }
}