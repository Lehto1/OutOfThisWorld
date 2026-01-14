using TMPro;
using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [SerializeField] Transform player;      // Spelaren som kameran följer
    [SerializeField] Vector3 offset;        // Avstånd bakom/ovanför spelaren
    [SerializeField] float smoothSpeed = 5f;
    Vector3 Velocity;

    void Update()
    {

        // Kameran har samma rotation som spelaren
        transform.rotation = player.rotation;

        // Mjuk följning
        // Mjuk följning
        // transform.position = Vector3.Lerp(
        // transform.position,
        // player.position - offset,
        // smoothSpeed * Time.deltaTime
        // );

        transform.position = player.position + 5 * -transform.forward;


    }
}
