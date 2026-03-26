using UnityEngine;

public class UpperBodyLook : MonoBehaviour
{
    [SerializeField] Transform lookTransform;
    [SerializeField] Transform spine002;
    [SerializeField] Transform spine003;
    [SerializeField] Transform spine004;

    [SerializeField] float maxUpAngle = 25f;
    [SerializeField] float maxDownAngle = -25f;

    [SerializeField] float spine002Weight = 0.15f;
    [SerializeField] float spine003Weight = 0.25f;
    [SerializeField] float spine004Weight = 0.30f;

    Quaternion spine002StartRot;
    Quaternion spine003StartRot;
    Quaternion spine004StartRot;

    void Awake()
    {
        if (spine002 != null) spine002StartRot = spine002.localRotation;
        if (spine003 != null) spine003StartRot = spine003.localRotation;
        if (spine004 != null) spine004StartRot = spine004.localRotation;
    }

    void LateUpdate()
    {
        if (lookTransform == null || spine002 == null || spine003 == null || spine004 == null)
            return;

        float pitch = lookTransform.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;

        pitch = Mathf.Clamp(pitch, maxDownAngle, maxUpAngle);

        spine002.localRotation = spine002StartRot * Quaternion.Euler(pitch * spine002Weight, 0f, 0f);
        spine003.localRotation = spine003StartRot * Quaternion.Euler(pitch * spine003Weight, 0f, 0f);
        spine004.localRotation = spine004StartRot * Quaternion.Euler(pitch * spine004Weight, 0f, 0f);
    }
}