using UnityEngine;

[CreateAssetMenu(fileName = "New Gun Stats", menuName = "Inventory/Gun Stats")]
public class gunStats : ScriptableObject
{
    [Header("----- Gun Info -----")]
    public string gunName;

    [Header("----- Model -----")]
    public GameObject gunModel;

    [Header("----- Shooting -----")]
    public int shootDamage = 3;
    public float shootRate = 0.8f;
    public float shootDistance = 15f;

    [Header("----- Ammo -----")]
    public int ammoMax = 12;
    public int ammoCur = 12;

    [Header("----- Effects -----")]
    public GameObject hitEffect;

    [Header("----- Audio -----")]
    public AudioClip[] shootSound;
    [Range(0f, 1f)] public float shootSoundVol = 0.5f;

    [Header("----- Hold Settings -----")]
    public Vector3 holdPosition = new Vector3(0f, 0f, 0f);
    public Vector3 holdRotation = new Vector3(4.647f, -76.149f, 0.12f);
    public Vector3 holdScale = new Vector3(1.3f, 1.3f, 1.3f);

    private void OnValidate()
    {
        holdPosition = new Vector3(0f, 0f, 0f);
        holdRotation = new Vector3(4.647f, -76.149f, 0.12f);
        holdScale = new Vector3(1.3f, 1.3f, 1.3f);
    }
}