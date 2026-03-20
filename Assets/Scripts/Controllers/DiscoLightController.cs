using UnityEngine;
using System.Collections;

public class DiscoLightController : MonoBehaviour
{
    public Transform target;

    [Header("Orbit Settings")]
    public float height = 6f;
    public float radius = 2.5f;
    public float orbitSpeed = 120f;

    [Header("Angle Offset")]
    public float startAngle = 0f;

    [Header("Light Settings")]
    public float colorChangeSpeed = 0.2f;

    private Light discoLight;
    private float angle;
    private Coroutine discoRoutine;

    void Awake()
    {
        discoLight = GetComponent<Light>();
        angle = startAngle;
    }

    void Update()
    {
        if (!gameObject.activeSelf || target == null) return;

        // Orbit around player
        angle += orbitSpeed * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * radius,
            height,
            Mathf.Sin(rad) * radius
        );

        transform.position = target.position + offset;

        //  Always look at player
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    public void StartDisco()
    {
        gameObject.SetActive(true);

        if (discoRoutine != null)
            StopCoroutine(discoRoutine);

        discoRoutine = StartCoroutine(ColorCycle());
    }

    public void StopDisco()
    {
        if (discoRoutine != null)
            StopCoroutine(discoRoutine);

        gameObject.SetActive(false);
    }

    IEnumerator ColorCycle()
    {
        while (true)
        {
            discoLight.color = Random.ColorHSV();
            yield return new WaitForSeconds(colorChangeSpeed);
        }
    }
}