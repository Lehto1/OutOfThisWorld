using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    internal class MutatedHuamn : AIPathfinding
    {
        [Header("Mutant Health")]
        [SerializeField] private float maxAIInsectHP = 50f; // Max hp för AI insekten

        private float currentInsectHP;

        [Header("InsectSettings Attack")]
        //dess definiera hur kraftig fienden attack kommer vara
        [SerializeField] private float aiDMG = 15f; //dess bass skada
        [SerializeField] private float aiDMGVariation = 6; // Alltså +- 6 skada
        [SerializeField] private float aiAttackCoolDown = 1.5f; // co0ldown

        private float aiAttackTimer = 0f; //tidtagning
                                          //Referens till Hälskoden
                                          //Ai:n måste få tillgång till koden så att det går att attackera spelare
                                          //
        [SerializeField] private HealthScript playerHealth;

        [Header("Wound and VIrus")]

        //integration utav virus 
        [SerializeField] private float infectionChance = 0.6f; // Chans att infektera en skada
        [SerializeField] private VirusHandlingScript playerVirusHandling; //REFERENS TILL VIRUSET

        //Minnimiskadan
        //Hindrar negativ skada
        [SerializeField] private float minimumDMGperAttack = 1f;


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
            // börjar röra sig mot spelarens senas kända pos
            navAgent.SetDestination(mostRecentPlayerPOS);

            //ifall insekten får syn på spelaren så..
            if (detectedPlayer && playerTransformTarget != null)
            {
                //uppdateras destinationen till den aktuella spelarpositionen
                navAgent.SetDestination(playerTransformTarget.position);
            }


            //kollar om insekten äär nära nog spelaren för att mangla 
            if (distanceToTarget <= aiAttackRadius)
            {
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
        //Barnklassernas olika egenskaper
        protected override void UniqueBehavior()
        {

        }

    }
}
