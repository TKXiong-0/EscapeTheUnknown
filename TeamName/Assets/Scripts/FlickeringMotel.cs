using UnityEngine;
using TMPro;
using System.Collections;

public class FlickerTrigger : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro text;
    public TextMeshPro text2;

    [Header("Trigger Settings")]
    public string playerTag = "Player";
    public int maxTriggerFlickers = 3;

    [Header("Wait Before Flicker")]
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    [Header("Burst Settings")]
    public int minBurstFlashes = 2;
    public int maxBurstFlashes = 5;
    public float flashOffTime = 0.05f;
    public float flashOnTime = 0.08f;

    [Header("Light Look")]
    public Color onColor = Color.red;
    public float minOnIntensity = 5f;
    public float maxOnIntensity = 12f;

    private Material mat1;
    private Material mat2;
    private int flickerCount = 0;
    private bool playerInside = false;
    private bool isRunning = false;

    void Start()
    {
        if (text == null)
        {
            Debug.LogError("FlickerTrigger: No TextMeshPro assigned.");
            return;
        }

        mat1 = text.fontMaterial;
        mat2 = text2.fontMaterial;
        SetOn();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (flickerCount >= maxTriggerFlickers) return;

        playerInside = true;

        if (!isRunning)
            StartCoroutine(FlickerRoutine());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
    }

    IEnumerator FlickerRoutine()
    {
        isRunning = true;

        while (playerInside && flickerCount < maxTriggerFlickers)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            if (!playerInside) break;

            int burstCount = Random.Range(minBurstFlashes, maxBurstFlashes + 1);

            for (int i = 0; i < burstCount; i++)
            {
                SetOff();
                yield return new WaitForSeconds(flashOffTime);

                SetOn();
                yield return new WaitForSeconds(flashOnTime);
            }

            flickerCount++;
        }

        SetOn();
        isRunning = false;
    }

    void SetOff()
    {
        if (mat1 != null) mat1.SetColor("_FaceColor", Color.black);
        if (mat2 != null) mat2.SetColor("_FaceColor", Color.black);
    }

    void SetOn()
    {
        float intensity = Random.Range(minOnIntensity, maxOnIntensity);
        Color finalColor = onColor * intensity;

        if (mat1 != null) mat1.SetColor("_FaceColor", finalColor);
        if (mat2 != null) mat2.SetColor("_FaceColor", finalColor);
    }
}