using UnityEngine;

public class CameraFollowHead : MonoBehaviour
{
    [SerializeField] Transform headAnchor;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] float followSpeed = 10f;

    void LateUpdate()
    {
        if (headAnchor == null) return;

        Vector3 targetPos = headAnchor.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );
    }
}