using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    internal class SpiderAI : AIPathfinding
    {
        [Header("Mutant Health")]
        [SerializeField] private float maxAIInsectHP = 50f; // Max hp för AI insekten

        private float currentInsectHP;

        [Header("SpiderSettings Attack")]
        //dess definiera hur kraftig fienden attack kommer vara
        [SerializeField] private float aiDMG = 15f; //dess bass skada
        [SerializeField] private float aiDMGVariation = 6; // Alltså +- 6 skada
        [SerializeField] private float aiAttackCoolDown = 1.5f; // co0ldown

        private float aiAttackTimer = 0f; //tidtagning
                                          //Referens till Hälskoden
                                          //Ai:n måste få tillgång till koden så att det går att attackera spelare
                                          //
        [SerializeField] private HealthScript playerHealth;

        [Header("Spidersattacks")]
        //spider bitande skada
        [SerializeField] private float spiderBiteDMG = 15F;

        //Spindel gifrskadan, sprutar gift mot spelaren
        [SerializeField] private float spiderVenomDMG = 10f;

        //Chans att psindeln spruta gift vid attack läge
        [SerializeField] private float venomChance = 0.3f;

        //drain prer sec
        [SerializeField] private float venomStaimnaDrainPS = 2f;

        [Header("Spider goopballattack")]
        //själva bollen
        [SerializeField] private GameObject goopBallPrefab;
        [SerializeField] private  Transform TransformGoopCreationPoint;

        //Attackegeskper hos slämbollen
        //styrkan som bollen avfyras med 
        [SerializeField] private float goopShotStrengh;

         //En varibel för tiden om behöver ha passerat §
         [SerializeField] private float goopShotCoOLDown;

        //Attacken´s räckhåll
        [SerializeField] private float goopShotRange;

        //System för avfyrning av 'goop'
        [Header("Spider TUrret system")]
        [SerializeField] private bool isCurrentlyInTurretMode = false; //Flag, Är spindeln i turret-läget?
        //Tid spindel får på sig att sikta på spealren 
        [SerializeField] private float spiderTurretLockT = 0.8f; ////
        //Goop laddning/återladdningstid'
        [SerializeField] private float spiderGoopChargingTime = 0.7f;
        //EN varuabler för hur snabbt spiden roterar för att sikta
        [SerializeField] private float  spiderTurretRotationSpeed = 5f;

        private float spiderTurretLockTimer = 0f; //En räknare för sikt-tiden
        private float spiderGoopChargeTimR = 0.6f; // // tÄKNARE FÖR LADDNING 
        private bool isGoopCharging = false; //Flag, laddar spinedeln 'goop' just nu?

        [Header("Advanced Goop settings")] //
        [SerializeField] private float goopProjectileSpeeed = 16f; //Bollens hastighet
        [SerializeField] private float aiTrajectoryPrediction = 0.5f; //Hur långt fram som vi räknar spelarens position
        [SerializeField] private float goopSpiderEnergyCost = 25f; // dEN Eneergi som spindel förbrukar per avfyrning
        [SerializeField] private float goopSpiderMaxEnergy = 100f; //Max energi för spindelns goop system
        [SerializeField] private float goopSpiderRegenPSec = 5f; // Regenerering per sekund
        [SerializeField] private int goopBurstCount = 3; // Mä gden bollar som avfyrars per burst
        [SerializeField] private float goopBurstAiDelay = 0.3f; // Tid mellan skotten/bollarna  i burst:en

        private float spiderCurrentGoopEnergy = 100f; //nuvarande eneregi
        private int goopBurstCounter = 0; //rän
        private float spiderGoopBurstTimr = 0f; //

        [Header("Goop Aiming")] // Siktesystem 
        [SerializeField] private Transform spiderMuzzlePoint; //Den pos där goop-bollen spawnar från //Spindels eldpunkt
        [SerializeField] private float spiderAimingAccuracy = 0.83f; // 83
        private Vector3 aiPredictedPlayerPos = Vector3.zero; // Foörutsagt position




        //Timern
        private float goopTimer = 0f;




        [Header("SpiderAI intelli")]

        //Spindeln blir riktigt sur när spelare nära
        [SerializeField] private float spiderAggroDistance = 15f;
         //spindel kan gömma sig vänta på spelaren.
        [SerializeField] private bool spiderCanHide = true;

        [SerializeField] private float spiderHideTime = 80f;

        [Header("Spider-spec Settings")]

        //Rörele och rörelse typer 
        //Hur snabbt spiden rör sig vid normala tillstånd
        [SerializeField] private float spiderNormalSpeed = 5f;
         //hur snabbt spidenl springer vid jakt
        [SerializeField] private float spiderSprintingSpeed = 8f;

        //hur snabbts spideln kan kyrpa upp för väggar/ hinder
        [SerializeField] private float spiderClimbingSpeeed = 5f;

        //"chargeup" tid innan varje hopp
        [SerializeField] private float SspiderLeapChargeTime = 0.5f;

        [Header("Spiderweb mechanics")]

        //Spindeln placerar ut ett nät för att fånga aoch sakta ned spelaren, Chans per sekund
        [SerializeField] private float webDeployChance = 0.25f;

        // hur länge spidlens nät varar på  spelaren 
        [SerializeField] private float webEffectDuration = 2.5f;

        //Hur mycket spelarne shastighet minskas med 
        [SerializeField] private float webMultiplier = 0.3f;

       
        [Header("Wound and VIrus")]

        //integration utav virus 
        [SerializeField] private float infectionChance = 0.6f; // Chans att infektera en skada
        [SerializeField] private VirusHandlingScript playerVirusHandling; //REFERENS TILL VIRUSET

        //Minnimiskadan
        //Hindrar negativ skada
        [SerializeField] private float minimumDMGperAttack = 1f;

        //Runtime variabler
        private bool isLeapingChargingUp = false;
        private float spiderLeapTimer = 0f;
        private float spiderLearnChargeTimer = 0f;

        //Timer för nät
        private float webTimer = 0f;

        //Håller spelaren fasthållen i nätet 
        private bool playerStuckInwEBn = false;
        private float duratiionOfWeb = 0f;

        //håller 



        //Initierar AI:s attacksystem
        //Anropas från start och hittar spelarens Health och Virus koder 
        protected virtual void InitializeAttack()
        {
            //Kontrollerar att spelaren är funnen
            //sätter target transform 
            Findplayer();

            //om null, hittades inte spelaren
            if (playerTransformTarget == null)
            {
                Debug.LogWarning($"{gameObject.name} HAS NOT FOUND A PLAYER, CAN THEREFORE NOT SET THE ATTACK-SYSTEM UP");
                return; //avbryter
            }

            //Letar efter Healthscript på samma GameObject som spelaren 
            //använder Getcompnent för att hitta playerhealth
            playerHealth = playerTransformTarget.GetComponent<HealthScript>();

            //gör en säkerhetskontroll 
            if (playerHealth == null)
            {
                Debug.LogError($" Critical failure:  {playerTransformTarget} lack a HealthScript");
                return; //
            }

            Debug.Log($"{gameObject.name} Healthscript found");

            //Viruskoden sitter på samma object som hälsan ovan
            //Om viruset inte finns så kan ai:n inte infektera
            playerVirusHandling = playerTransformTarget.GetComponent<VirusHandlingScript>();

            if (playerVirusHandling == null)
            {
                Debug.LogWarning($"{gameObject.name} Player lacks Virushandler");
                //Retunerar inte här då AI:n forfarande kan attakera även om den saknas
            }
            else
            {

                Debug.Log($"{gameObject.name} VirusHandler found, AI can infect wounds");

            }

            //sätter timern till ett initial värde
            //börjar med cooldown måste 

            aiAttackTimer = aiAttackCoolDown;
            Debug.Log($"{gameObject.name} attack-system fully set up , DMG : {aiDMG} cOOLDOW : {aiAttackCoolDown}");


        }

        //Skapar en startmeto för att anropa metoden ovan i 
        protected override void Start()
        {
            base.Start(); // Annropar förälderns start först

            //initialiserar Hp
            currentInsectHP = maxAIInsectHP;
            //sätter till max

            //Initialiserar attacksystemet
            InitializeAttack();

            Debug.Log($"Current {gameObject.name} HP : {currentInsectHP}/{maxAIInsectHP}");
        }

        //En public metod så att spelarn kan skada insekten
        public void TakeDamage(float damage)
        {

            currentInsectHP -= damage; // skader dess hälsa
            currentInsectHP = Mathf.Max(0, currentInsectHP); // Värdet kan aldrig gå under noll

            //ifall Insekten har leka med eller mindre än 0 Hp , så dör den
            if (currentInsectHP <= 0)
            {
                Dead(); //dödar inskekten
            }
        }
        //updaterar attackerings timern
        protected override void UpdateTimer()
        {
            base.UpdateTimer();

            //ifall Ai timern  är större än 0 
            //räknar koden ner till noll
            if (aiAttackTimer > 0f)
            {
                aiAttackTimer -= Time.deltaTime; //nedräkning
            }

            //uppdaterar timern för nedsliming
            if(goopTimer > 0f)
            {
                goopTimer -= Time.deltaTime;
            }
        }

        protected override void ExecuteIdle()
        {
            //insekten står stilla, gör inget 
            //väntar på händelser
            navAgent.velocity = Vector3.zero;

        }
        protected override void ExecutePatrol()
        {
            //sätter NavAgentens destination 
            navAgent.SetDestination(aiPatrolWaypoints[aiPatrolWaypointsIndex].position);
            //Undersöker om insekten har nått "waypointen" 
            if (navAgent.remainingDistance < aiPointTole)
            {
                //insekten har nu kommit tillräkligt nära waypointen för att det 
                //det ska räknas att AI; har varit där 
                if (aiPointTImer >= aiTimeAtPoint)
                {
                    //Börjar färd mot nästa waypoint
                    aiPatrolWaypointsIndex++;

                    //Om Ai:n har passerat alla waypoints , ska det gå tilbaka till den första startpunkten
                    if (aiPatrolWaypointsIndex >= aiPatrolWaypoints.Length)
                    {
                        aiPatrolWaypointsIndex = 0;

                    }
                    //ställer om timern
                    aiPointTImer = 0f;

                    //bestäm nästa waypoint
                    DecideNextWaypoint();

                    Debug.Log($"THe insect {gameObject.name} moving to its next waypoint : {aiPatrolWaypointsIndex}");


                }
            }
        }

        protected override void ExecuteChase()
        {
            //Turret 
            //Om AI:n redan är i turret läga, ska dne forsätt amed turret logik
            if(isCurrentlyInTurretMode)
            {
                UppdateSpiderTurretLock();
                UpdateGoopCharge();
                UpdateAISpiderBurstDelay();

                //Spindeln röre sig inte i turret läge 
                navAgent.velocity = Vector3.zero;
                return; //avlutar 
            }

            // Om AI:n inte var i turretläge 
            //Kollar koden om spelarenbefinnersig inom räckvidd
            if (detectedPlayer && playerTransformTarget != null)
            {
                //Kontrollerar om spealren är inom reäckvidd
                if (distanceToTarget <= goopShotRange && distanceToTarget > aiAttackRadius)
                {
                    //K ontrollerar om AI:n har tillräckligt med Energi
                    if (spiderCurrentGoopEnergy >= goopSpiderEnergyCost)
                    {
                        //kontrolerar om goop-cooldownen är klar eller ej 
                        if (goopTimer <= 0f)
                        {
                            //Påbörjar turretläget
                            EnterTurretMode();
                            return;
                        }
                    }
                }


                // börjar röra sig mot spelarens senas kända pos
                navAgent.SetDestination(playerTransformTarget.position);

            } else
            {    // börjar röra sig mot spelarens senas kända pos
                navAgent.SetDestination(mostRecentPlayerPOS);
            }
            
            //kollar om insekten äär nära nog spelaren för att mangla 
            if (distanceToTarget <= aiAttackRadius)
            {
                //Om spelaren är vldigt nära, attackera direkt
                ChangeState(AiState.Attack);
            }

        }



        protected override void ExecuteAttack()
        {
            //Gör en säkerhetkontroll 
            //kollar om det finns ett hälsosystem
            if (playerHealth == null)
            {
                Debug.LogError($" {gameObject.name} Playerhealth is null,");
                return; // abryter då hälskoden inte finns
            }

            //En till kontroll , kOllar ifall attack cooldownen är färdig
            if (aiAttackTimer > 0f)
            {
                return; // cooldownen pågår forfarande, väntar ....
            }

            //Tredje kontroll, Kollar om spelaren är inom attackeringsradien
            if (distanceToTarget > aiAttackRadius)
            {
                //Kommer återgå till jakt tillsåtnden om AI:n är längre bort
                return;
            }

            //Beräknar skadan med den applicerade variationen
            float rndVariation = UnityEngine.Random.Range(-aiDMGVariation, aiDMGVariation);
            float finalDMG = aiDMG + rndVariation; // den totala DMG

            //Minimi 1 skada
            finalDMG = Mathf.Max(finalDMG, minimumDMGperAttack);

            //Applicerar skadan på spelaren 
            playerHealth.ApplyDMG(finalDMG);

            Debug.Log($" {gameObject.name} Attacking with {finalDMG} DMG, Player HP {playerHealth.CurrentHP}/{playerHealth.MaxHP}");

            //Här kommer AI:n försöka inficera sår givet att attacken var stark nog 
            if (finalDMG >= 5f && UnityEngine.Random.value < infectionChance)
            {
                //
                if (playerVirusHandling != null)
                {
                    //Hämtar listan övar alla sår
                    List<Wound> allWounds = playerHealth.GetWounds();

                    if (allWounds != null && allWounds.Count > 0)
                    {
                        //Infektera det senaste såret 
                        Wound mostRecentWound = allWounds[allWounds.Count - 1];
                        mostRecentWound.isInfected = true;

                        Debug.Log($"Virus infection started {gameObject.name} infected ID {mostRecentWound.id}");
                    }

                    //

                }

            }

            //Nollsätller cooldown för nästa Attack
            aiAttackTimer = aiAttackCoolDown;

            Debug.Log($"Next attack in {aiAttackCoolDown}seconds");

        }
        protected override void ExecuteDeath()
        {
            // stoppar all rörelse 
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.velocity = Vector3.zero; // stoppar all Agent hastighet
                navAgent.enabled = false; //stängar av agenten
            }

            //Stägner Även av all AI logic 
            this.enabled = false;  // stänger av alltihopa

            // 
            Debug.Log($"{gameObject.name} Is now Dead");


        }

        //Public metod för att växla AI state till det döda
        public void Dead()
        {
            ChangeState(AiState.Dead);
        }

        //Metod som förbereder och avfyrar den slimeiga bollen
        private void ShootGoopBall()
        {
            //Säkerhetskontroller
            if(goopBallPrefab == null || spiderMuzzlePoint == null || playerTransformTarget == null)
            {
                Debug.LogWarning($"{gameObject.name} can not fire the goop, missing setup");
                return;
            }

            //Kollar spindels energinivå
            if(spiderCurrentGoopEnergy < goopSpiderEnergyCost)
            {
                Debug.Log($"{gameObject.name} does not have enough energy required to fire goop");
                return;
            }

            //Förutsäger splearens postiion
            AiCalculatedTargetPOSPrediction();

            //Beräknar riktingen mot den förutsagda positionen
            Vector3 aiDirectionToPredictedTarget = (aiPredictedPlayerPos - spiderMuzzlePoint.position).normalized;

            //lägger till ett slumpat fel i ai:ns gissing
            float hitChance = UnityEngine.Random.value;
            if(hitChance > spiderAimingAccuracy)
            {
                //Speindeln missade 
                //Den nya positionen ska avika lite från den ursprungliga målpositionen
                Vector3 rndDeviation = UnityEngine.Random.insideUnitSphere * 0.3f;
                aiDirectionToPredictedTarget = (aiDirectionToPredictedTarget + rndDeviation).normalized;
                Debug.Log($"{gameObject.name} missed it's shot");

            }

            //Kollar först om cooldownen är klar

            //Skapar bollen
            //spawnar denna boll
            //instantiatar en boll
            GameObject goopBall = Instantiate(goopBallPrefab, TransformGoopCreationPoint.position, Quaternion.identity);

            //Applicerar kraft och hastighet på bollen 
            Rigidbody goopRigidBody = goopBall.GetComponent<Rigidbody>();
            //applicerar en kraftt på denna boll
            if (goopRigidBody!= null)
            {
                goopRigidBody.linearVelocity = aiDirectionToPredictedTarget * goopProjectileSpeeed; // avstånd gpnger stryka

            }

            //Förvrukar spindelns energi
            spiderCurrentGoopEnergy -= goopSpiderEnergyCost;
           
          

            Debug.Log($"{gameObject.name} FIRED A GOOPBALL AT THE PLAYER  {spiderCurrentGoopEnergy}/{goopSpiderMaxEnergy}");

        }

        //Denna metod  roterar spideln så att den siktar på spelaren
        private void AimAtPlayerPos()
        {
            //En kontroll, FInns spelaren verkilgen?
            if (playerTransformTarget == null) return;

            //Beräknar riktning från spindel till spelaren
            Vector3 directionSpiderToPlayer = (playerTransformTarget.position - transform.position).normalized;

            //Skapar en roatiaon baserat på riktningen
            Quaternion targetRot = Quaternion.LookRotation(directionSpiderToPlayer);

            //Ler´par rotationen 
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * spiderTurretRotationSpeed);

            Debug.Log($"{gameObject.name} Aiming at player ");

        }

        //Uppdaterar turretlåsnings timern
        private void UppdateSpiderTurretLock()
        {
            if (!isCurrentlyInTurretMode) {
                return;
            }

            //Om spindeln inte redan låser, bör den börja göra det
            if(spiderTurretLockTimer< spiderTurretLockT)
            {
                spiderTurretLockTimer += Time.deltaTime;
                AimAtPlayerPos(); //Siktar samtidigt

            } else
            {
                //Börjar med att ladda 
                if(!isGoopCharging)
                {
                    isGoopCharging = true; ;

                    spiderGoopChargeTimR = 0f;

                    Debug.Log($"{gameObject.name} is now charging its goop");
                }
            }
        }

        //En metod som ger en visusel feedback Exempelvis vid laddning av goop
        private void AnimateSpiderGoopCharge ()
        {
            //lagarar 
            float chargingProcess = Mathf.Clamp01(spiderGoopChargeTimR / spiderGoopChargingTime); 

            //Eldpunkten växer i storlek under laddning
            if(spiderMuzzlePoint != null)
            {
                float pulsationScale = 1f + (chargingProcess * 0.6f); //växer

                spiderMuzzlePoint.localScale = Vector3.one * pulsationScale; 
            }
            Debug.Log($"Goop charigng {(chargingProcess * 100)}% ");
        }

        private void UpdateGoopCharge()
        {
            if (!isGoopCharging)
            {
                return;
            }

            //Öka laddnings timern 
            spiderGoopChargeTimR += Time.deltaTime;

            //Animerar under laddningen
            AnimateSpiderGoopCharge();

            //Spinder skjuter när laddningen är klar
            if (spiderGoopChargeTimR >= spiderGoopChargingTime)
            {
                //Skutwr goop
                FireGoopBall();

                //Nollställer laddningen
                isGoopCharging = false;
                spiderGoopChargeTimR = 0f;

                //Ökar burst-räknaren
                goopBurstCount++; //Ökar  täknaren

                //Om spindeln har avfyrat tillräckligt många skott, skall den omedelbart sluta
                if (goopBurstCount >= goopBurstCount)
                {
                    //avlutar "Turret"läget , återgår till jakt 
                    ExitTurretMode();
                } else
                {
                    //Väntar lite innan nästa skjuting i bursten
                    //DelY
                    spiderGoopBurstTimr = goopBurstAiDelay;
                }

            }

        }

        private void UpdateAISpiderBurstDelay()
        {
            //Väntar mellan bollarnas avfyrning i burst
            if (spiderGoopBurstTimr > 0f)
            {

                spiderGoopBurstTimr -= Time.deltaTime; // mINSKAR timern

                //När spindelns väntetid är över laddar den nästa boll 
                if (spiderGoopBurstTimr <= 0f && goopBurstCounter < goopBurstCount)
                {
                    isGoopCharging = true;
                    spiderGoopChargeTimR = 0f;
                }


            }

        }
        //Enerysystem
        private void UpdatSpiderAIeGoopEnergy()
        {
            //oM SPINDELS ai LIGGER UDNER MAX, regenererar den 
            if(spiderCurrentGoopEnergy < goopSpiderMaxEnergy)
            {
                spiderCurrentGoopEnergy += goopSpiderRegenPSec * Time.deltaTime;
                spiderCurrentGoopEnergy = Mathf.Min(spiderCurrentGoopEnergy, maxAIInsectHP); //cAPPAR TILL MAX
            }

            float aiEnergyPercent = (spiderCurrentGoopEnergy / goopSpiderMaxEnergy) * 100;
            if(aiEnergyPercent < 30f)
            {
                Debug.Log($"Low energy :{gameObject.name} Goop energy : {aiEnergyPercent}");
            }
        }


        //AI:n beräknar spelarens förmodade postioin när goop träffar 
        private void AiCalculatedTargetPOSPrediction()
        {

            //Säkkerhetskontroll
            if(playerTransformTarget == null)
            {
                aiPredictedPlayerPos = playerTransformTarget.position;
                return;
            }

            //Hä,mtar spelarens hastighet
            Movement playerMovement = playerTransformTarget.GetComponent<Movement>();
            Vector3 playerVel = Vector3.zero;

            if(playerMovement != null)
            {
                playerVel = playerMovement.GetCurrentVelocity(); 
            }

            //Beräknar tiden som det tar för goopbollen att ny spelaren
            float spiderDistanceToPLayer = distanceToTarget;
            float timeToTatget = spiderDistanceToPLayer / goopProjectileSpeeed;

            //AI:n förutsäger spelarens position
            aiPredictedPlayerPos = playerTransformTarget.position + (playerVel * timeToTatget * aiTrajectoryPrediction);

            Debug.Log($"Predited Pos:{aiPredictedPlayerPos}");

        }

        //Barnklassernas olika egenskaper
        protected override void UniqueBehavior()
        {

        }

    }
}
