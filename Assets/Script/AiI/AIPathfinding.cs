using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.XR;

//Detta kommer vara bas scriptet för all AI rörelse i spelet
//för insekter och människor

public abstract class AIPathfinding : MonoBehaviour
{
    [Header("Extra")]
    [SerializeField] protected float aiTurnSpeedAtObstruction = 5f;
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

    protected float stateTimer;


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

    //En flagga som barnklassen sätter till true under hopp och landing
    //När denna Boolean sätt till true kommer denna kod (fad) att pausa all motverkande logik
    protected bool isChildPerformingLeap = false;


    //Lämmanr tomt för nu, Fyller i senare
    public virtual void Awake()
    {
        //Hämtar navmesh-agenten
        NavMeshInit();

        //Skriver till konsollen
      //  Debug.Log("lll");
        //Eventuell animation
        //Eventuellt ljud
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //initialiserar alla värden och letar/finner vart spelaren är

 protected virtual void Start()
    {
        //Konfugurerar AI:ns "pathfinding" 
        NavMeshConfig();
        //Söker upp och finner spelarens position 
        Findplayer();

        //Initialiserar Ai:s skick/tillsåtmd
        //Initialiserar spelarens start tillstånd
        AIstateInit();

        UnityEngine.Debug.Log($"AI Init, State : {currentAIState} Speed : {aiMovementSPeed} ");


    }
    //Initialiserar Navmesh Agenten
    protected virtual void NavMeshInit()
    {
        //Hämtar agenten
        navAgent = GetComponent<NavMeshAgent>();

        // Ifall navAgenten finns kommer koden nedan inte att köras
        if (navAgent == null)
        {
            UnityEngine.Debug.LogError("The game does not have a NavMesh co");
            enabled = false;
            return;

        }
        UnityEngine.Debug.Log("AIpathf required Navmesh ");
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

        UnityEngine.Debug.Log($"navAget has been Configurated {gameObject.name} sPEED {navAgent.speed} TURNSPEED {navAgent.angularSpeed}");


    }
    //Denna metod är till för att hitta spelaren 
    public virtual void Findplayer()
    {
        //kolla om spelaren redan är markerad och funnen
        //ifall den är det, Returernar koden
        if(playerTransformTarget != null)
        {
            UnityEngine.Debug.Log("Player already found, returning....");
            return;
        }

        //söker efters spelar taggen
        GameObject playerOBJ = GameObject.FindGameObjectWithTag("Player");
        if (playerOBJ != null)
        {
            //sätter ai:n target position till spelar objektets transform
            playerTransformTarget = playerOBJ.transform;

            UnityEngine.Debug.Log($"Player was found by tag{playerOBJ.name}");
        }
        else
        {
            //Debuggr
            UnityEngine.Debug.LogWarning($"Player could no be found");
          
        }

        }

    //En emtod som initialiserar AI:ns start tillstånd
    public virtual void AIstateInit()
    {
        //Kollar först ifall det finns waypoints
        if (aiPatrolWaypoints != null && aiPatrolWaypoints.Length > 0)
        {
            //Efter som koden har några waypoints utsatta, så kan Ai:n nu börja patrullera
            ChangeState(AiState.Patrol);  //byter AI tillstånd
            DecideNextWaypoint();
            UnityEngine.Debug.Log($"Ai kommer starta I sitt patrullerings tillstånd");

        }
        else
        {
            //AI:n kommer förbli Idle 
            //Eftersom det inte finns några punkter för AI:n att gå efter
            ChangeState(AiState.Idle);
            UnityEngine.Debug.Log("AI will start in IDLE");

        }

    
    }


    // Update is called once per frame
  public virtual void Update()
    {
        //Hoppar över update om AI:n är död
        if (currentAIState == AiState.Dead) return;

        //Uppdaterar först Ains timer
        //alla timers
        UpdateTimer();

        BaseTrackPlayerPosition(); //färsk pos

        //Beräknar avstånd mellan Ai och spelare
        CheckDistToPlayer();

        //Kollar om det går att hitta spelaren 
    //  Ai:n kollar om det går att se splelaren
        LookForPlayer();

        //updaterar och kollar väntetiden vid punkten
        UpdatePointWaitTimer();

        //Uppdaterar AttackCooldown timern
        //Minskar timern mot 0 vilket först då tillåter nästa attack

        //reducerar timer
        
        //roterar mot den egna färdriktningen 
        RotateTowardsMovementDir();

        //Hantera hinder och väggkollisioner
        HandleWallObstuction();

        //A:n Beslutar 
        //statelogic;
        Execute();

        //Barnklassernas olika egenskaper
        UniqueBehavior();


    }
    //Uppdaterar först Ains timer
    //TImern öker så länge ai:n beffineer sig i ett vist tillstånd.
    protected virtual void UpdateTimer()
    {
      stateTimer += Time.deltaTime;

    }

    
    //uPPDATERAR OCKSÅ VÄNTETIMERN VID VARJE POINT  
protected virtual void UpdatePointWaitTimer()
    {
        if(currentAIState == AiState.Patrol)
        {
            //ifall Ai:n patrulerar ökas timern på
            aiPointTImer += Time.deltaTime;
        }

    }

    //En metod där AI:n kommer kolla om det går att se / upptäcka spelaren
    //Kollar om det går att hitta spelaren 
    protected virtual void LookForPlayer()
    {
        //Under hoppet ska detektionslogiken även den frysas
        //Koden vet redan vart spelaren är någonstans
        if (isChildPerformingLeap) return;

        //Kommer retunera ifall spelaren inte är korrekt kopplad till koden
        if (playerTransformTarget == null) return;

        //skapar 3 st boolean flags för att bättre kunna kontrolera alla vilkor
        bool inAIFOV = IsInFOV(); //sYNFÄLT
        bool inAiRange = IsINDetectionRadius(); //
        bool hasSight = InLineOfSight();

        //KOMBINER ALLA TRE TILL EN ENDA BOOL FLAG
        //En flag för när spelaren är upptäckbar
        bool AiCanSeePlayer = inAiRange && inAIFOV && hasSight;

        //detektion 
        if (AiCanSeePlayer && !detectedPlayer) //om ai:n ser spelarn och so
        {
            //Ai uppyäcker spelarn
            detectedPlayer = true;
            mostRecentPlayerPOS = playerTransformTarget.position;

            UnityEngine.Debug.Log($"Player detected");

            //byter Ai:s tillstpnd
            //bör nu börja jaga spelaren
            ChangeState(AiState.Chase);

        }
        else if (!AiCanSeePlayer && detectedPlayer)
        {
            //Ai har inte lägre spelaren i sikte
            detectedPlayer = false;

            UnityEngine.Debug.Log($"AI {gameObject.name} lost sight of the player");

            //ifall Ai:n inte längre ser spelaren, ska AI: gå till spelarens senast kända 
            //position
            ChangeState(AiState.Chase);

        }

        //Updapdate senast kända Pos
        if (detectedPlayer && playerTransformTarget != null)
        {
            //updaterar poitionen till splearpositionen
            mostRecentPlayerPOS = playerTransformTarget.position;

        }
    }

    //Metod som ansvarar för att hålla 'mostrecentPlayerPOS' updaterad
    protected virtual void BaseTrackPlayerPosition()
    {
        //Uppdaterar positionen vid könde spelare
        //NavAgent aktiveras efter hopp
        if(playerTransformTarget != null && detectedPlayer)
        {
            mostRecentPlayerPOS = playerTransformTarget.position;
        }
    }
    //beräknar avståndet mellan spelare och och AI:n

    //Tar reda på avstånde mellan Ai och spelare
    // Ai:n kommer 
    protected virtual void CheckDistToPlayer()
    {
        if (playerTransformTarget == null) // När det inte finns någon position , ingen spelare
        {
            distanceToTarget = Mathf.Infinity;
            return; 
        }
        //sätter distance to target till ett kalkulerat värde Vector3 distance mellan positionerna
        distanceToTarget = Vector3.Distance(transform.position, playerTransformTarget.position);
        

    }

    //Beslutar
   

    //bools
    //En bool för om spelaren är inom AI:ns dektektionsradie

    protected virtual bool IsINDetectionRadius()
    {
        //om spelaren inte finns, befinner den sig automatiskt utanför radien
        if (playerTransformTarget == null) return false;

        //flag för om spelaren är nära nog
        bool inRange = distanceToTarget <= aiRadiusOfDetection; //ny 

        return inRange;
        //
        
    }

    //En bool för om spelaren är inOM AI:s synfält 
    
    protected virtual bool IsInFOV()
    {

       if(playerTransformTarget == null) return false; // Fings inngne

        //beräknar riktning utfrpn hur spelaren position förhåller sig till  AI:n
        Vector3 dirrToTarget = (playerTransformTarget.position - transform.position).normalized;

        //beräknar en vinkel utifrån ovanståande
        float aiAngleToTarge = Vector3.Angle(transform.forward, dirrToTarget);

        // en flag bool för att kolla om AI:n kan see spelaren in dens FOV 
        bool inFov = aiAngleToTarge <= aiFOV / 2f; //utifrån ovanstående

        return inFov;
    }

    //Kan AI se spelaren, //kan ai 
    //håller koll så att det int finns object i vägen, för då syns inte spelaren
    //använder raycast strålar för att kolla om det stämmer eller ej
    //
    protected virtual bool InLineOfSight()
    {
        //valliderar, kontrolerar att spelaren finns
        //om spelaren inte finns retunar koden false
        //kan då iint egöra raycast
        if(playerTransformTarget == null) return false;

        //raycastar från mitten av AI modelen. 
        //inte vid basen
        //flyttar up positonen med en vector3 för att inte hamna för nära marken
        Vector3 aiSightPos = transform.position + Vector3.up * 1.5f;

        //skickar raycast strålar mot spelarens POS
        Vector3 playerTargetPOS = playerTransformTarget.position + Vector3.up * 1f;

        //beräknar riktningen och avståndet för raycast
        //villen riktning 
        //hur långt strålen ska färdas

        //beräknar vektor mella AI och spelare
        Vector3 aiDirectionToPlayer = (playerTargetPOS - aiSightPos).normalized;
        //får bara riktningen eftersom jag använde normalized

        //här beräknar jag det faktiska avstånde melllan taycastens start ochs lutpunkter
        float distToCheckk = Vector3.Distance(aiSightPos,playerTargetPOS);

        //Skapar raycast 
        RaycastHit raycastHit;
        bool somethinfHasBeenHitByRC = Physics.Raycast(aiSightPos, aiDirectionToPlayer, out raycastHit, distToCheckk, obstacleLayer); //Dess start position,riktning,resultat, hur långt att färdas, lager att

        //om raycasten träffade något
        //kollar Ai:n vad det är som raycasten träffade
        //Om Ai:n ser spelaren, retuneras True, vilket betyder att det går att se spelaren
        //oM Ai:n inte ser. eller kollar in i en vägg så kommer koden att retunera false. vilket betyder att spelrn inte syns
        if (somethinfHasBeenHitByRC)
        {
            //kollar vad det var som träffades av raycast 

            //jämför det träffade objectets transform med splerans för att kolla om strålen landade rätt
            if (raycastHit.transform == playerTransformTarget)
            {
                //STRÅLEN TRÄFFADE Spelaren
                //Ai:n kan 
                //se spelaren
                return true;
            }
            else
            {
                //strålen träffade ngot objekt, 
                //traffade vägg eller hinder
                return false;
            }

        }
        else
        {
            //När rayvast inte träffat något alls
            //fri sikt
            return true;
        }
    }

    ///_----------------------------*//// Kommer senare lägga till en state logic här 
    ///place holder för state logic .
    ///
    //Navigering

    //NAVIGERING
    protected virtual void DecideNextWaypoint()
    {

        if (aiPatrolWaypoints == null || aiPatrolWaypoints.Length == 0)
        {
            return; // de finns inga
           
        }

        if (!navAgent || !navAgent.enabled || !navAgent.isOnNavMesh) { return; }

        navAgent.SetDestination(aiPatrolWaypoints[aiPatrolWaypointsIndex].position); // Nav 
    }

    //navAgent börjar röra sig mor spelaren 
    protected virtual void MoveTowardsPlayer()
    {

        if (playerTransformTarget == null)
        {
            return;
        }

        if (!navAgent || !navAgent.enabled || !navAgent.isOnNavMesh) { return; }
        navAgent.SetDestination(playerTransformTarget.position); // sätter 

    }
    /// <summary>
    /// 


    //sTATECHANGER, //Byter Ai:s tillstånd 
    protected virtual void ChangeState(AiState newState)
    {
        //gör ingeting om det nuvarnade tillstååndet är det samma som det nya
        if(currentAIState == newState)
        {
            return;
        }

        //det gamla, förredetta tillståndet uppdateras
        previousAIState = currentAIState;

        //uppdatera  det nuvarande till det nya tillståndet
        currentAIState = newState;

        stateTimer = 0f; // eftersom koden har bytt 

        //nollställer även waypoint timern
        aiPointTImer = 0f;

        UnityEngine.Debug.Log($"AI {gameObject.name} has changed state from {previousAIState} to {currentAIState} ");

    }

    //huvud metod 
    //Kör  logic baserat på tillstånd

    
    protected virtual void Execute()
    {
        //switchar baserat på nuvarande tillstånd
        switch(currentAIState) 
        {
            case AiState.Idle: //Vid Idle
                ExecuteIdle();
                break;
            case AiState.Patrol: //vid patrullering
                ExecutePatrol();
                break;
            case AiState.Chase: // Vid jakt
                ExecuteChase();
                break;
            case AiState.Attack: // Vid attack
                ExecuteAttack();
                break;
            case AiState.Dead: // Vid död
                ExecuteDeath();
                break;

            default: break; //inget
        }
    }
    
    //helper metoder som ska implementeras och fyllas utav barnklasserna
  protected virtual void  ExecuteIdle()
    {

    }
    protected virtual void ExecutePatrol()
    {

    }
    protected virtual void ExecuteChase()
    {

    }

    protected virtual void ExecuteAttack()
    {

    }
    protected virtual void ExecuteDeath()
    {

    }
    //Barnklassernas olika egenskaper
    protected virtual void UniqueBehavior()
    {

    }

    protected virtual void RotateTowardsMovementDir()
    {
        //Kollar först om navAgent finns
        //Ifall den ite gör det så ska koden retunera
        if(navAgent == null)
        {
            return;
        } 

        //KSäkerställer att AI:n har en aget väg att fälja
        if(navAgent.path == null)
        {
            return;
        }

        //hämmtar dess rörelseriktning
        Vector3 aiMovementDirection = navAgent.desiredVelocity;

        aiMovementDirection = aiMovementDirection.normalized;

        //ifall Ai:n rör sig
        if(aiMovementDirection.sqrMagnitude < 0.01f)
        {
            return; //annars
        }

        //skapar en måltoration att rotera till
        //utav rörelseriktningen

        Quaternion targAITravelRotation = Quaternion.LookRotation(aiMovementDirection, Vector3.up); // mot rörelse rikting

        //gör så att rotering blir jäm och len 
        transform.rotation = Quaternion.Lerp(transform.rotation, targAITravelRotation, aiTurningSpeed * Time.deltaTime);

        //Dbugdgubdgubdubg
        UnityEngine.Debug.DrawLine(transform.position,transform.position + aiMovementDirection * 2f, Color.red, 0.2f);

    }
  
    protected virtual void HandleWallObstuction()
    {

        //Om ett barn alrig kör vägglogiken
        if (isChildPerformingLeap) return; 

        //kollar om en Agent finns
        if(navAgent == null)
        {
            return;
        }

        if (!navAgent.enabled)
        {
            return;


        }
        if (!navAgent.isOnNavMesh)
        {
            return;
        }

        //kontrollerar om vägen har beräknats utav navAgent AI:N
        if(navAgent.pathPending)
        {
            return;
        }

        //mätter sedan agentens hastighet vid tillfället
        //dess nuvariga hastighet
        float currentNavAiSpeed = navAgent.velocity.magnitude;

        //cHECKA om AI: har fastnat mot en vägg eller liknande
        if(currentNavAiSpeed < 0.1f && navAgent.remainingDistance > 0.5f)
        {
            UnityEngine.Debug.Log($"The {gameObject.name} AI is stuck");

            //taycast framåt mot det som hindrar AI:n
            RaycastHit wallObsHit;
            Vector3 raycastDir = transform.forward; //castar rakt fram
            float rayCastDistance = 2f;
            Vector3 rayCASTStartingPos = transform.position + Vector3.up * 0.5f; //utser en star pos

            bool hitObsorWall = Physics.Raycast(rayCASTStartingPos, raycastDir, out wallObsHit, rayCastDistance, obstacleLayer); //raycastar alltså mot ostavle lagret liksom klassens sikt

            //vid träff av vägg eller annat
            if (hitObsorWall)
            {
                UnityEngine.Debug.Log($"Wall or obstructing object in´front of {gameObject.name}");

                Vector3 normalWall = wallObsHit.normal;

                //nya riktningen
                Vector3 newDirection = Vector3.Reflect(transform.forward, normalWall); //kommer "studsa" av wäggen

                //Nya slutmålet
                Vector3 newDestination = transform.position + newDirection * 5f;

                //nav
                navAgent.SetDestination(newDestination);



            } else // OM INGEN 
            {
                UnityEngine.Debug.Log($"{gameObject.name} random U-turn");

                Vector3 rndDirection = Random.insideUnitSphere; //genererar en slumpad riktning
                rndDirection.y = 0; //säter y till 0 
                rndDirection = rndDirection.normalized;

                Vector3 rndDestination = transform.position + rndDirection * 5f; //beräknar den slumpade

                navAgent.SetDestination(rndDestination);


            }




            }
        }

    }




//Börjat med Enum
public enum AiState
{
    Idle, 
    Patrol,
    Chase,
    Attack,
    Dead,
     //Lägger kanske till tar kanske bort
}

//Barnklassernas olika egenskaper
//  UniqueBehavior

