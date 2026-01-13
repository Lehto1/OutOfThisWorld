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
    public int ee = 1;
    //Lägger till states senare;
    //-------------------///

    [Header("Patroling")]

    //Waypoints, som AIn går och rör sig mellan
    [SerializeField] protected Transform[] aiPatrolWaypoints;

    //håller reda på vilke puntk som ain försöker ta sig till
    [SerializeField] protected int aiPatrolWaypointsIndex;

    //Bestämmer hur länge en AI ska stanna vid varje koordinatpunk
    [SerializeField] protected float aiTimeAtPoint;

    // 
    [SerializeField] protected float aiPointTole;


    //Lämmanr tomt för nu, Fyller i senare
    public void Awake()
    {
        NavMeshInit();
        GetHealthCode(); //Hämtar 
        //Eventuell animation
        //Eventuellt ljud


    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NavMeshConfig();
        Findplayer();
        AIstateInit();

    }

    protected virtual void NavMeshInit()
    {

    }
    public virtual void GetHealthCode()
    {

    }
    public virtual void NavMeshConfig()
    {

    }
    public virtual void Findplayer()
    {

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

