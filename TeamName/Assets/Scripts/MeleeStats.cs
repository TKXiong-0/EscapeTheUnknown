using UnityEngine;

[CreateAssetMenu]

public class MeleeStats : ScriptableObject
{
    public GameObject meleeModel;

    [Range(1, 10)] public int meleeDamage = 3;
    [Range(1, 5)] public float meleeRange = 2f;
    [Range(0.1f, 2)] public float attackRate = 0.8f;

    public ParticleSystem hitEffect;

    public AudioClip[] swingSound;
    [Range(0, 1)] public float swingSoundVol = 1f;
}
