using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Gun")]
public class gunStats : ScriptableObject
{
    [Header("----- Gun Info -----")]
    public string gunName;

    [Header("----- Model -----")]
    public GameObject gunModel;

    [Header("----- Shooting -----")]
    public int shootDamage = 10;
    public float shootRate = 0.2f;
    public float shootDistance = 100f;

    [Header("----- Ammo -----")]
    public int ammoMax = 30;
    public int ammoCur = 30;

    [Header("----- Effects -----")]
    public GameObject hitEffect;

    [Header("----- Audio -----")]
    public AudioClip[] shootSound;
    [Range(0f, 1f)] public float shootSoundVol = 1f;

    [Header("----- Hold Settings -----")]
    public Vector3 holdPosition;
    public Vector3 holdRotation;
    public Vector3 holdScale = Vector3.one;
}