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
    public Vector3 holdPosition = Vector3.zero;
    public Vector3 holdRotation = Vector3.zero;
    public Vector3 holdScale = Vector3.one;
}