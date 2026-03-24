using UnityEngine;

[CreateAssetMenu]

public class HealStats : ScriptableObject
{
    public GameObject healModel;

    [Range(1, 20)] public int healAmount = 5;

    public AudioClip[] healSound;
    [Range(0, 1)] public float healSoundVol = 1f;
}