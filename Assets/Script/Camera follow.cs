using TMPro;
using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [Header("Base variables")]
    [SerializeField] Transform player;      // Spelaren som kameran följer
    [SerializeField] Vector3 offset = new Vector3(0, 1.5f, -4f);        // Avstånd bakom/ovanför spelaren
    [SerializeField] float positionSmoothSpeed = 5f;
    [SerializeField] float rotationSmoothSpeed = 10f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float minumumCollisionDistance = 0.8f;
    [SerializeField] float camRadius = 0.3f; // Till för spherecast inte raycast

    [Header("Speed zoom")]
    [SerializeField]  float minZoomDistance = 2.0f; // Kammeran kommer åtminnstånde zooma till detta
    [SerializeField] float maxZoomDistance = 6f; //Det maximala som kammeran kommer kunna zooma
    [SerializeField] float zoomSmothingSpeed = 2f;
    [SerializeField] float maxSpeed = 14f;

    [Header("When low stamina")]
    [SerializeField] float staminaLowThresh = 20f; // OM stamina understiger detta värde kommer spelarkameran påverkas
    [SerializeField] float cameraShakeMagnitudé = 0.2f; // den kraft som kameran kommer skaka/gunga med
    [SerializeField] float cameraShakeSpeed = 2f; //hastigheten som kameran kommer gunga med


    private Vector3 Velocity;
    private float currentDistance; // Nuvariga avstånd


    void Update()
    {
        //Kontrollerar så att spelaren finns
        if (player == null)
        {
            return;
        }
        //Tar baort Y rotationen från spelaren 
        Quaternion onlyYawCamera = Quaternion.Euler(0f, player.eulerAngles.y, 0f);

        //Beräknar målpositionen baserat på spelarensr rotation
        //Rotterar barra runt Y led

        Vector3 targPos = player.position + onlyYawCamera * offset;

        //kontrollera kollisionen
        //flyttar närmare om något träffas
        Vector3 directionToCamera = (targPos - player.position).normalized;

        float wantedDistance = offset.magnitude;
        currentDistance = wantedDistance;

        //Startar rayvcasten lite ovan spealren
        Vector3 rayCastOrginPos = player.position + Vector3.up * 1.0f;

        //Spherecast istället, 
        if (Physics.SphereCast(rayCastOrginPos, camRadius, directionToCamera, out RaycastHit hit, wantedDistance, collisionMask))
        {
            currentDistance = Mathf.Max(hit.distance - minumumCollisionDistance, 0.3f);
        }

        //uppdatera targ pos baserat på distansen
        targPos = rayCastOrginPos + directionToCamera * currentDistance;

        //mjuk
        transform.position = Vector3.SmoothDamp(transform.position, targPos, ref Velocity, 1f / positionSmoothSpeed);

        //Kammeran tittar på spelaren
        //Kammeran tittar i spelaren riktning
        Vector3 playerLookDirection = player.forward; //Tittar i samma riktning som spealren

        //Skapar en  roataion baseat på spelarens blickriktning
        Quaternion targetRotation = Quaternion.LookRotation(playerLookDirection, Vector3.up);
        // Kameran har samma rotation som spelaren
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
     

 