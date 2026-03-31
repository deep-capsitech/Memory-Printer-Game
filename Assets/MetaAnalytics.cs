using Facebook.Unity;
using UnityEngine;

public class MetaAnalytics : MonoBehaviour
{
    void Start()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                FB.ActivateApp();
                Debug.Log("Meta Analytics Started");
            });
        }
        else
        {
            FB.ActivateApp();
        }
    }
}
