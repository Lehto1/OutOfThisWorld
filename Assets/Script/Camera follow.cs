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

    [Header("FOV zooming")]
    [SerializeField] bool useZoomFOV = true; //En boolean flag som indikerar när 
    [SerializeField] float regularFOV = 65f;
    [SerializeField] float sprintingFOV = 80f;
    [SerializeField] float speedFOV = 4f;

    [Header("When low stamina")]
    [SerializeField] float staminaLowThresh = 20f; // OM stamina understiger detta värde kommer spelarkameran påverkas
    [SerializeField] float cameraShakeMagnitudé = 0.2f; // den kraft som kameran kommer skaka/gunga med
    [SerializeField] float cameraShakeSpeed = 2f; //hastigheten som kameran kommer gunga med

    //reff
    private Movement playerMovement;
    private HealthScript playerHp;

    //privata FOV
    private Camera cam; //Refferat till kameran själv
    private float currentFOV;
    private float lastSpeed;

     
    private Vector3 Velocity;
    private float currentDistance; // Nuvariga avstånd
    private Vector3 lastPlayerPos; // senast käna spelarpositionen

    private void Start()
    {
        //Hämtar rörelse och hp scripten
        if(player != null)
        {
            playerHp = player.GetComponent<HealthScript>();
            playerMovement = player.GetComponent<Movement>();
        }

        //Hittar kameran
        cam = GetComponent<Camera>();
        if (cam != null)
        {

            currentFOV = regularFOV; //Sätter 
            cam.fieldOfView = currentFOV; //sätter som nuvarig
        }

        // Sätter LastPos till den nuvariga 
        lastPlayerPos = player.position;
        lastSpeed = 0f;
    }

    void Update()
    {
        //Kontrollerar så att spelaren finns
        if (player == null)
        {
            return;
        }


        //Räknar ut spelarens hastighet
        Vector3 velocityOfPlayer = (player.position - lastPlayerPos) / Time.deltaTime;
         float currentSpeed = new Vector3(velocityOfPlayer.x, 0, velocityOfPlayer.z).magnitude; // En horisontell
        lastPlayerPos = player.position;

        //beräknar zoom baserat på spelarens hastighet
        float playerSpeedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
        float preferedZoomDistance = Mathf.Lerp(minZoomDistance, maxZoomDistance, playerSpeedRatio);
        currentDistance = Mathf.Lerp(currentDistance, preferedZoomDistance, zoomSmothingSpeed * Time.deltaTime);

        //Beräknar FOV Zomming 
        if (useZoomFOV && cam != null)
        {

            float fovTarget = Mathf.Lerp(regularFOV,sprintingFOV,playerSpeedRatio);
            currentFOV = Mathf.Lerp(currentFOV, fovTarget, speedFOV * Time.deltaTime);
            cam.fieldOfView = currentFOV;
        }
        //Hämtar spelarens stamina
        float currentSTAMI = 0f;
        if (playerHp != null)
        {
            currentSTAMI = playerHp.GetCurrentStamina();

        }

        //Beräknar kameraskakningen baserat på låg stamina
        float amountOfShake = 0f;
        if (currentSTAMI < staminaLowThresh) //om värdet understiger
        {
            //Ju lägre stamina värde desto mer skakande
            float staminaRatio = currentSTAMI / staminaLowThresh;
            amountOfShake = (1f - staminaRatio) * cameraShakeMagnitudé;

        }

        //Tar baort Y rotationen från spelaren 
        Quaternion onlyYawCamera = Quaternion.Euler(0f, player.eulerAngles.y, 0f);

        //Justerar offseten beroende på på zoom-värdet
        Vector3 zoomAdjustedOffset = new Vector3(offset.x, offset.y, offset.z * (preferedZoomDistance / offset.magnitude)); 

        //Beräknar målpositionen baserat på spelarensr rotation
        //Rotterar barra runt Y led
        Vector3 targPos = player.position + onlyYawCamera * offset;

        //kontrollera kollisionen
        //flyttar närmare om något träffas
        Vector3 directionToCamera = (targPos - player.position).normalized;

        //Startar rayvcasten lite ovan spealren
        Vector3 rayCastOrginPos = player.position + Vector3.up * 1.0f;

        float wantedDistance = offset.magnitude;
        currentDistance = wantedDistance;

        //Spherecast istället, 
        if (Physics.SphereCast(rayCastOrginPos, camRadius, directionToCamera, out RaycastHit hit, wantedDistance, collisionMask))
        {
            float rayCollisionDistancem = Mathf.Max(hit.distance - minumumCollisionDistance, 0.5f);

            targPos = rayCastOrginPos + directionToCamera * rayCollisionDistancem;
        }

        //Applicera ShakeOffset
        Vector3 camShakeOffset = new Vector3(Random.Range(-amountOfShake, amountOfShake), Random.Range(-amountOfShake, amountOfShake), 0);
        targPos += camShakeOffset;

        ///////Ta 

        // Zoomövergång
        currentDistance = Mathf.Lerp(currentDistance, preferedZoomDistance, zoomSmothingSpeed * Time.deltaTime);

        //uppdatera targ pos baserat på distansen
        targPos = rayCastOrginPos + directionToCamera * currentDistance;

        //Tillämpar en skak-effect på kameran
        Vector3 shakingOffset = Random.insideUnitCircle * amountOfShake;
        targPos += shakingOffset;

        ///////
        /// 
        //mjuk kamerarörelse 
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
     

 