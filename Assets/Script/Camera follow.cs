using TMPro;
using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [SerializeField] Transform player;      // Spelaren som kameran följer
    [SerializeField] Vector3 offset = new Vector3(0, 1.5f, -4f);        // Avstånd bakom/ovanför spelaren
    [SerializeField] float positionSmoothSpeed = 5f;
    [SerializeField] float rotationSmoothSpeed = 10f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float minumumCollisionDistance = 0.8f;

    private Vector3 Velocity;
    private float currentDistance; // Nuvariga avstånd

    void Update()
    {
        //Kontrollerar så att spelaren finns
        if (player == null)
        {
            return;
        }

        //Beräknar målpositionen
        Vector3 targPos = player.position + player.TransformDirection(offset);

        //kontrollera kollisionen
        //flyttar närmare om något träffas
        Vector3 directionToCamera = (targPos - player.position).normalized;

        float wantedDistance = offset.magnitude;
        currentDistance = wantedDistance;

        //Raycast
        if (Physics.Raycast(player.position, directionToCamera, out RaycastHit hit, wantedDistance, collisionMask))
        {
            currentDistance = Mathf.Max(hit.distance - minumumCollisionDistance, 0.3f);
        }

        //uppdatera targ pos baserat på distansen
        targPos = player.position + directionToCamera * currentDistance;

        //mjuk
        transform.position = Vector3.SmoothDamp(transform.position, targPos, ref Velocity, 1f / positionSmoothSpeed);

        //Kammeran tittar på spelaren
        Vector3 lookatPlayer = player.position + Vector3.up * 0.6f;
        //över huvudet 
        Vector3 dirrectionToLook = (lookatPlayer - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirrectionToLook);

        // Kameran har samma rotation som spelaren
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
     

 