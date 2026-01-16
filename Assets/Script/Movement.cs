using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Movement : MonoBehaviour
{
    [SerializeField] int Running = 2; //Variabel som ändrar på spelarens hastighet
    [SerializeField] bool Crouching = false; //Bool som bestämmer om spelaren crouchar eller inte

    [Header("Stamina and Health")]
    [SerializeField] private HealthScript playerHpScript; //referens till spelarens hälsa
    [SerializeField] private float staminaDrainPS = 18f; //Hur mycket stamina som förbrukas per sekund vid springande
    [SerializeField] private float minStaminaRequiredToSprint = 6f; //mINSTA STAMMINA som krävs för att springa

    Rigidbody RB;
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
        IsCrouching();
        RunningOrNot();
        MovementStuff();
    }

    void MovementStuff() //Includes keybinds and code for moving the player
    {
        RB.angularVelocity = new Vector3(0, 0, 0);
        RB.linearVelocity = new Vector3(0, RB.linearVelocity.y, 0);

        if (Input.GetKey(KeyCode.W) && Crouching == false) // If sats for att go bakåt och framåt när man står up
        {
            RB.linearVelocity = Running * transform.forward;
        }
        else if (Input.GetKey(KeyCode.S) && Crouching == false)
        {
            RB.linearVelocity = Running * -transform.forward;
        }

        if (Input.GetKey(KeyCode.W) && Crouching == true) // If sats for att go bakåt och framåt när man är crouching
        {
            RB.linearVelocity = transform.forward;
        }
        else if (Input.GetKey(KeyCode.S) && Crouching == false)
        {
            RB.linearVelocity = -transform.forward;
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

    void RunningOrNot()
    {
        //

        if (playerHpScript == null)
        {
            if (Input.GetKey(KeyCode.LeftShift)) //Används för att ändra spelarens hastighet när de springer
            {
                Running = 4;
            }
            else
            {
                Running = 2;
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
            //spelaren srpinger här

            //öker dess hastighet
            Running = 4;

            //räknaer sedan ut hur mycket stamina som bör dras från spelaren
            float staminaCostCurrentFrame = staminaDrainPS * Time.deltaTime;

            //använd stamina genom playerHealth
            playerHpScript.UseStamina(staminaCostCurrentFrame);

        }
        else
        {
            //spelaren går , 
            Running = 2;
        }


    }

    void IsCrouching()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Crouching == false) //Gör så att man kan stänga av och sätta på crouching
        {
            Crouching = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Crouching == true)
        {
            Crouching = false;
        }
    }
}
