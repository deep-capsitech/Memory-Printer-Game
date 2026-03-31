#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MetaAnalyticsAutoAttach
{
    private const string GameObjectName = "MetaAnalytics";

    static MetaAnalyticsAutoAttach()
    {
        EditorApplication.delayCall += Attach;
    }

    private static void Attach()
    {
        // Find or create GameObject
        GameObject go = GameObject.Find(GameObjectName);
        if (go == null)
        {
            go = new GameObject(GameObjectName);
            Debug.Log("[Editor] Created GameObject: " + GameObjectName);
        }

        // Attach MetaAnalytics script if missing
        if (go.GetComponent<MetaAnalytics>() == null)
        {
            go.AddComponent<MetaAnalytics>();
            Debug.Log("[Editor] MetaAnalytics script attached");
            EditorUtility.SetDirty(go);
        }
    }
}
#endif
