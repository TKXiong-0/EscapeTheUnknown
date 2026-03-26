using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Stats", menuName = "Inventory/Melee Stats")]
public class MeleeStats : ScriptableObject
{
    [Header("----- Melee Info -----")]
    public string meleeName;

    [Header("----- Model -----")]
    public GameObject meleeModel;

    [Header("----- Attack -----")]
    public int attackDamage = 2;
    public float attackRate = 0.5f;
    public float attackDistance = 2f;

    [Header("----- Effects -----")]
    public GameObject hitEffect;

    [Header("----- Audio -----")]
    public AudioClip[] swingSound;
    [Range(0f, 1f)] public float swingSoundVol = 0.5f;

    [Header("----- Hold Settings -----")]
    public Vector3 holdPosition = Vector3.zero;
    public Vector3 holdRotation = Vector3.zero;
    public Vector3 holdScale = Vector3.one;
}