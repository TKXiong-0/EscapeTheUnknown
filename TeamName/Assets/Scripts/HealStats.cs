using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Stats", menuName = "Inventory/Heal Stats")]
public class HealStats : ScriptableObject
{
    [Header("----- Heal Info -----")]
    public string healName;

    [Header("----- Model -----")]
    public GameObject healModel;

    [Header("----- Healing -----")]
    public int healAmount = 25;
    public float useRate = 0.5f;

    [Header("----- Audio -----")]
    public AudioClip[] healSound;
    [Range(0f, 1f)] public float healSoundVol = 0.5f;

    [Header("----- Hold Settings -----")]
    public Vector3 holdPosition = new Vector3(0.067f, -0.02f, -0.174f);
    public Vector3 holdRotation = new Vector3(4.647f, -76.149f, 0.12f);
    public Vector3 holdScale = new Vector3(1.3f, 1.3f, 1.3f);

    private void OnValidate()
    {
        holdPosition = new Vector3(0.067f, -0.02f, -0.174f);
        holdRotation = new Vector3(4.647f, -76.149f, 0.12f);
        holdScale = new Vector3(1.3f, 1.3f, 1.3f);
    }
}