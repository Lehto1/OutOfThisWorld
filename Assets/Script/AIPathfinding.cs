using System.Threading;
using UnityEngine;
using UnityEngine.AI;

//Detta kommer vara bas scriptet för all AI rörelse i spelet
//för insekter och människor

public abstract class AIPathfinding : MonoBehaviour
{
    [Header("Pathfinding")]
    //Skapar en navmesh variable.
    //kommer hantera mycket utav Ains navigation
    protected NavMeshAgent navAgent; //det inbyggda navigations systemet

    //En variabel för Ai:s rörelsehastighet
    //Bestämmer bashastigheten för ALL ai
    [SerializeField] protected float aiMovementSPeed;

    //Den maximala rörelsehastigheten 
    //Den hastighet som basAI:n kommer ha vi exemplevis jagning utav spelaren
    [SerializeField] protected float aiSprintSPeed;

    //Den hastighet som AI:n vänder sig med 
    //Byter riktning med 
    [SerializeField] protected float aiTurningSpeed;

    //Dess acceleration
    [SerializeField] protected float aiAcc; // Acceleration

    [Header("Tracking Info")]

    //Gör en referens till spelaren,
    //spelaren är AIs målposition
    //Spelarens Transform
    [SerializeField] protected Transform playerTransformTarget;

    //Variabel för Ains dekteterinsradie
    //Inom värdet utav denna radie kommer AIn kunna upptäcka spelaren 
    [SerializeField] protected float aiRadiusOfDetection = 20f;

    //Hur nära Ain måste vara spealren för att kunna göra skada på spealrens hälsa
    [SerializeField] protected float aiAttackRadius = 1f;

    //Variable för AiNS FOV, AIns synvinkel
    [SerializeField] protected float aiFOV = 115f; //alltså 115 grader synvinkel

    //Ai mosnters höravstånd, 
    //bestämmer radien som AIn kan höra ljud på
    [SerializeField] protected float aiHearingRange = 20f;

    // Tillför spelarens egna LAGER. 
    //Spelaren behöver vara i sitt egna lager så att AIN filterar korrekt
    [SerializeField] protected LayerMask playerLayer;

    //Kopplar ett hinderLager.
    //Dena används för att belusta om en AI instans "line of sight"
    [SerializeField] protected LayerMask obstacleLayer;

    [Header("AiState")]
    
    /// Ains nuvarande "state"
    //Det som AIn gör i stunden
    protected AiState currentAIState = AiState.Idle; // Ai står still och 

    //Variabel för det "state" som kom innan den nuvarande.

    /// /förredetta tillståndet
    /// 
    protected AiState previousAIState;

    //En bool flag som håller koll på om spelaren har blicigt upptäckt utav AI:N
    //flag
    protected bool detectedPlayer = false;

    //Variabel som lagrar spelarens senast kända position.
    //Denna varriablel uppdateras varje gång AI:n utav olika själ tappar bort spelaren
    protected Vector3 mostRecentPlayerPOS;

    protected float distanceToTarget;


    [Header("Patroling")]

    //Waypoints, som AIn går och rör sig mellan
    [SerializeField] protected Transform[] aiPatrolWaypoints;

    //håller reda på vilke puntk som ain försöker ta sig till
    [SerializeField] protected int aiPatrolWaypointsIndex;

    //Bestämmer hur länge en AI ska stanna vid varje koordinatpunk
    [SerializeField] protected float aiTimeAtPoint;

    // 
    [SerializeField] protected float aiPointTole;

    //eN TRACKER FÖR hur länge Ai:n har väntat vid en viss checkpoint
    [SerializeField] protected float aiPointTImer;


    //Lämmanr tomt för nu, Fyller i senare
    public void Awake()
    {
        //Hämtar navmesh-agenten
        NavMeshInit();

        //Skriver till konsollen
        Debug.Log("lll");
        //Eventuell animation
        //Eventuellt ljud
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //initialiserar alla värden och letar/finner vart spelaren är

 void Start()
    {
        //Konfugurerar AI:ns "pathfinding" 
        NavMeshConfig();
        //Söker upp och finner spelarens position 
        Findplayer();
        //Initialiserar Ai:s skick/tillsåtmd
        //Initialiserar spelarens start tillstånd
        AIstateInit();

        Debug.Log($"AI Init, State : {currentAIState} Speed : {aiMovementSPeed} ");


    }
    //Initialiserar Navmesh Agenten
    protected virtual void NavMeshInit()
    {
        //Hämtar agenten
        navAgent = GetComponent<NavMeshAgent>();

        // Ifall navAgenten finns kommer koden nedan inte att köras
        if (navAgent == null)
        {
            Debug.LogError("The game does not have a NavMesh co");
            enabled = false;
            return;

        }
        Debug.Log("AIpathf required Navmesh ");
    }

  //Konfigurerar NavMesh inställningar, sätter dessa till värdena utav klasses egna variabler nedan
  //
    public virtual void NavMeshConfig()
    {
        if (navAgent == null) return;

        //Sätter Agentens hastighet till klassens vaiabels värde
        navAgent.speed = aiMovementSPeed;

        // gör det samma med rotationshastigheten
        navAgent.angularSpeed = aiTurningSpeed;

        //sätter dess acceleratio till samma värde som klassens variabel
        navAgent.acceleration = aiAcc;

        //sätt på agenten
        navAgent.enabled = true;

        Debug.Log($"navAget has been Configurated {gameObject.name} sPEED {navAgent.speed} TURNSPEED {navAgent.angularSpeed}");


    }
    //Denna metod är till för att hitta spelaren 
    public virtual void Findplayer()
    {
        //kolla om spelaren redan är markerad och funnen
        //ifall den är det, Returernar koden
        if(playerTransformTarget != null)
        {
            Debug.Log("Player already found, returning....");
            return;
        }


    }
    public virtual void AIstateInit()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Uppdaterar först Ains timer
        UpdateTimer();

        //Kollar om det går att hitta spelaren 
        LookForPlayer();

        //Tar reda på avstånde mellan Ai och spelare
        CheckDistToPlayer();

        //Beslutar
        Execute();

        //Barnklassernas olika egenskaper
        UniqueBehavior();


    }
    //Uppdaterar först Ains timer
    protected virtual void UpdateTimer()
    {

    }

    //Kollar om det går att hitta spelaren 
    protected virtual void LookForPlayer()
    {

    }

    //Tar reda på avstånde mellan Ai och spelare
    protected virtual void CheckDistToPlayer()
    {

    }

    //Beslutar
    protected virtual void Execute()
    {

    }

    //bools
    protected virtual bool IsINDetectionRadius()
    {
        return false;
    }
    protected virtual bool IsInFOV()
    {
        return false;
    }
    protected virtual bool InLineOfSight()
    {
        return false;
    }
    ///_----------------------------*//// Kommer senare lägga till en state logic här 
    ///place holder för state logic .
    ///
    //Navigering
    protected virtual void DecideNextWaypoint()
    {
        if (aiPatrolWaypoints == null || aiPatrolWaypoints.Length == 0)
        {
            return; // de finns inga
            navAgent.SetDestination(aiPatrolWaypoints[aiPatrolWaypointsIndex].position); // Nav 
        }
    }

    //navAgent börjar röra sig mor spelaren 
    protected virtual void MoveTowardsPlayer()
    {
        if (playerTransformTarget != null)
        {
            navAgent.SetDestination(playerTransformTarget.position); // sätter 

        }

    }


    //Barnklassernas olika egenskaper
    protected abstract void UniqueBehavior();
 


}

//Börjat med Enum
public enum AiState
{
    Idle, 
    Patrol,
    Chase,
    Attack,
     //Lägger kanske till tar kanske bort
}

//Barnklassernas olika egenskaper
//  UniqueBehavior

