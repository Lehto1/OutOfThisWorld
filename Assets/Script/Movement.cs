using JetBrains.Annotations;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;

public class Movement : MonoBehaviour
{
    [SerializeField] int Running = 2; //Variabel som ändrar på spelarens hastighet
    [SerializeField] bool Crouching = false; //Bool som bestämmer om spelaren crouchar eller inte

    [Header("Stamina and Health")]
    [SerializeField] private HealthScript playerHpScript; //referens till spelarens hälsa
    [SerializeField] private float staminaDrainPS = 18f; //Hur mycket stamina som förbrukas per sekund vid springande
    [SerializeField] private float minStaminaRequiredToSprint = 6f; //mINSTA STAMMINA som krävs för att springa

    [Header("SpiderInteract")]
    // den nuvariga multiplikationsfaktorn för 
    [SerializeField] private float currentSpeedMult = 1f;

    [Header("Acceleration")]
    [SerializeField] private float accTime = 0.4f; //Hr snabbt spelaren kan nå full hastighet
    [SerializeField] private float deAccTime = 0.2f;  //Hur snabbt spelaren stannar upp

    [Header("Momentum")]
    [SerializeField] private float retentionOfMomentum = 0.4f; //Den andel fart som behålls vid sväng

    [Header("Extra Sprinting")]
    [SerializeField] private float burstingSprintMultiplier = 1.3f; //Hur ycket snabbare spelaren accelerrar under "Burst" tiden
    [SerializeField] private float durationOfBurts = 0.3f; //Hur länge demma "burst varar

    [Header("Crouch slide")]
    [SerializeField] private float durationOfCrouchSlide = 0.7f; //Hur länge spelarens glidande varar
    [SerializeField] private float speedOfCrouchSlide = 11f; // Dess hastighet

    [Header("Strafe Movement")]
    [SerializeField] private float strafingSpeed = 1f; // Sidrörelse

    

    //
    [SerializeField] private float slownessTimer = 0f;
    Rigidbody RB;

    //Privata variabler
    private float currentSpeed = 0f; //Nuvarande hastighet
    private float velocitySpeed = 0f; // Smo0thdamping
    private bool wasRunning = false; //Booleanflag, sprang spelaren?
    private bool isCrouchSliding = false; // 
    private Vector3 directionOfSlide; //den riktning som spelaren glider åt
    private float sprintSpeedBust = 0f;
    private Vector3 previousVelocity; //Senaste 


    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        //Försöker hitta healthScript genom att seka egenom detta object
        if (playerHpScript == null)
        {
            playerHpScript = GetComponent<HealthScript>();
            if (playerHpScript == null) //om den fortfarande är null
            {
                Debug.LogWarning("The movementscript is missing a healthscript reference on the same GameObject");

            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //Uppdaterar alla timers
        //Uppdaterar sprint burst timer 
        if (sprintSpeedBust > 0f)
        {
            sprintSpeedBust -= speedOfCrouchSlide; //Minskar timern men varje frame
        }

        //uppdaterar crouch-slide timern
        if (isCrouchSliding && durationOfCrouchSlide > 0f)
        {
            //Minskar
            durationOfCrouchSlide -= Time.deltaTime;

            //Om slide:en är avlutad, Avlutas slide:en
            if (durationOfCrouchSlide <= 0f)
            {
                isCrouchSliding = false; // slutar glida
            }
        }

        //Uppdaterar slowness effect
        if (slownessTimer > 0f)
        {
            slownessTimer -= Time.deltaTime;

            //återställer när timern är slut
            if (slownessTimer <= 0f)
            {
                currentSpeedMult = 1f;
                Debug.Log("Slowness deactivated");
            }
        }
        //Anropar funktioner
        IsCrouching();
        RunningOrNot();
        MovementStuff();


        void MovementStuff() //Includes keybinds and code for moving the player
        {
            //Nollställer rotation och förhindrar att spelaren flippar
            RB.angularVelocity = new Vector3(0, 0, 0);
            RB.linearVelocity = new Vector3(0, RB.linearVelocity.y, 0);

            //Horizintal och vetical
            float vertical = 0f;  //Står för W/S Fram och bakåt
            float horizontal = 0f; //A/D stårför väster höger

            if (Input.GetKey(KeyCode.W) && Crouching == false) // If sats for att go bakåt och framåt när man står up
            {
        vertical = 1f;
            }
            else if (Input.GetKey(KeyCode.S) && Crouching == false)
            {
              vertical = -1f;
            }

            if (Input.GetKey(KeyCode.W) && Crouching == true) // If sats for att go bakåt och framåt när man är crouching
            {
                horizontal = 1f;
            }
            else if (Input.GetKey(KeyCode.S) && Crouching == false)
            {
                horizontal = -1f;
            }


            if (Input.GetKey(KeyCode.A)) //If sats för att rotera din käraktär
            {
                RB.angularVelocity = new Vector3(0, -4, 0);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                RB.angularVelocity = new Vector3(0, 4, 0);
            }
        }

        //Hantering utav "Crouch slide"

        if (isCrouchSliding)
        {
            //beräknar vart i slide:en som spelaren befinner sig
            float playerSlideProgress = 1f - (durationOfCrouchSlide / 0.7f); //0.7 är max

            //Minskar glidhastigheten över tid----- från speedOfCrouchslide till 0 
            float currentSlidingSpeed = Mathf.Lerp(speedOfCrouchSlide, 0, playerSlideProgress);

            //Skapar en glid-rörelse i den riktning som spelaren färdas i 
            Vector3 velocityOfCrouchSlide = directionOfSlide * currentSlidingSpeed;

            //Applicerar glid-rörelsen på spelaren
            RB.linearVelocity = new Vector3(velocityOfCrouchSlide.x, RB.linearVelocity.y, velocityOfCrouchSlide.z);

            return; //stoppar

        }

        //Beräknar rörelseriktningen + strafe
        Vector3 playerMoveDirection = Vector3.zero; //börjar från noll

        //OM spelaren inte crouchar loopas denna
        if (!Crouching)
        {
            //Framåt och bakåt
            playerMoveDirection += transform.forward * vertical;

            //lägger till en sidåtrörelse(  en strafe)
            if (vertical != 0f)
            {
                playerMoveDirection += transform.right * horizont * strafingSpeed;
            }

            //Normalisierar spelarriktningen så att rörelsen inte blir snabbare
            playerMoveDirection = playerMoveDirection.normalized;
        }
        else
        {

            //OM  spelaren crouchar 
            //Får spealren en långsammare rörelse 
            playerMoveDirection = transform.forward * vertical * 0.5f;
        }

        //Beräknar målhastigheten
        float playerTargetSpeed = 0f; //börjar med 0 hastighet

        if (playerMoveDirection.magnitude > 0.1f)
        {
            //Sätter målhastigheten till Running-värdet
            playerTargetSpeed = Running;

            //Applicerar en sprint burst om burst är aktivt och spelaren springer
            if (sprintSpeedBust > 0f && Running > 6)
            {
                playerTargetSpeed = Running * burstingSprintMultiplier; // 1,3x

            }

            //Applicerar slowness (Finns inte ännu)
            playerTargetSpeed *= currentSpeedMult;
        }
        else
        {
            //Spelaren hastighet är noll 
            //spelaren står still
            playerTargetSpeed = 0f;
        }

        //Acceleration och decelerationhantering
        float momveSmoothTime;

        if (playerTargetSpeed > currentSpeed)
        {
            //Spelaren accelerarar
            momveSmoothTime = accTime; // 0.x;
        }
        else
        {
            //  Spelaren Decelerar och stannar 
            momveSmoothTime = deAccTime;
        }

        //Skapar en mjuk övergång mellan och till målhastighetn
        currentSpeed = Mathf.SmoothDamp(currentSpeed, playerTargetSpeed, ref velocitySpeed, momveSmoothTime);

        //Beräknar "Velocity" från spelarens riktning och hastighet
        Vector3 newMoveVelocity = playerMoveDirection * currentSpeed;

        //Momentum hantering och applicering
        //Gör så att spelaren behlller lite av hastigheten vid sväng
        Vector3 mixedVelocity = Vector3.Lerp(newMoveVelocity, previousVelocity, retentionOfMomentum);

        //Sparar *previousvelocity* för nästa 
        previousVelocity = mixedVelocity;

        //Applicerar en slutgilting velocity
        //Sättter spelarrörelsen 
        RB.linearVelocity = new Vector3(mixedVelocity.x, RB.linearVelocity.y, mixedVelocity.z);

    }
            void RunningOrNot()
            {
                //
                //Kollar först om healhyscript finns
                if (playerHpScript == null)
                {
                    if (Input.GetKey(KeyCode.LeftShift)) //Används för att ändra spelarens hastighet när de springer
                    {
                        Running = 12;
                    }
                    else
                    {
                        Running = 6;
                    }

                    return; //avlustar om det inte finns stamina

                }


                //Kollar om spelaren försöker spinga 
                //Omr spelaren håller ned LeftShift
                bool wantsToRun = Input.GetKey(KeyCode.LeftShift);

                //boolean flag 
                //har spelaren tillräckligt med stamina för att få spring a eller inte 
                bool hasStaminaLeft = playerHpScript.HasSufficentStamina(minStaminaRequiredToSprint);

                if (wantsToRun && hasStaminaLeft && Crouching == false)
                {
            //Om spelaren inte sprang för a framen
            if(!wasRunning)
            {
                ////Startar burst-timern 
                //Vilket gör spelaren 1.3x snabbare
                sprintSpeedBust = durationOfBurts;
            }

            //Markerar att spealren faktiskt springer
            wasRunning = true;
                    //spelaren srpinger här

                    //öker dess hastighet
                    Running = 12;

            //dRAR STAMINA
                    //räknaer sedan ut hur mycket stamina som bör dras från spelaren
                    float staminaCostCurrentFrame = staminaDrainPS * Time.deltaTime;

                    //använd stamina genom playerHealth
                    playerHpScript.UseStamina(staminaCostCurrentFrame);

                }
                else
                {
            //Spelaren går
            //MARKERAR ATT spelaren inte springer
            wasRunning = false;

            //sätter 
                    //spelaren går , 
                    Running = 6;
                }


            }

    //Hanterar crouCH OCH crouch-slide
        void IsCrouching()
        {
        //Crouchslide, Om spelaren trycker 'ctrl', springer och inte redan crouchar
        if(Input.GetKey(KeyCode.LeftControl) && wasRunning && !Crouching)
        {
            //påbörjar crouch slide
            BeginCrouchSlide();
            return;
        }

        //Om spelaren Trycker på ctrl och inte rean crouchar
            if (Input.GetKeyDown(KeyCode.LeftControl) && Crouching == false) //Gör så att man kan stänga av och sätta på crouching
            {
                Crouching = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl) && Crouching == true)
            {
                Crouching = false;
            }
        }

    //Denna funktion anropas när spelaren springer och trycjer Ctrl
    void BeginCrouchSlide()
    {
        //Boolean markerar att speLAREN GLIDER
        Crouching = true;
        isCrouchSliding = true; 
        directionOfSlide = transform.forward; //Gilder i spelarens frammåt riktning
        durationOfCrouchSlide = 0.7f; //återswtäller
        Running = (int)speedOfCrouchSlide; // hastighet till den bestämda 

        Debug.Log("The Player's crouch-slide has begun");
    }

    public void ApplySlowness(float multi, float duration)
    {
        currentSpeedMult = multi;

        slownessTimer = duration;

    }
}
    }


