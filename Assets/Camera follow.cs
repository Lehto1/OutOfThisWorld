using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    public Transform player;      // Spelaren som kameran följer
    public Vector3 offset;        // Avstånd bakom/ovanför spelaren
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        // Önskad position bakom spelaren
        Vector3 targetPosition = player.position + offset;

        // Mjuk följning
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
