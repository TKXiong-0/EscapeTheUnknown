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
    public Vector3 holdPosition = new Vector3(0.02f, -0.033f, -0.145f);
    public Vector3 holdRotation = new Vector3(22.39f, 94.425f, 6.422f);
    public Vector3 holdScale = new Vector3(1.3f, 1.3f, 1.3f);

    private void OnValidate()
    {
        holdPosition = new Vector3(0.02f, -0.033f, -0.145f);
        holdRotation = new Vector3(22.39f, 94.425f, 6.422f);
        holdScale = new Vector3(1.3f, 1.3f, 1.3f);
    }
}