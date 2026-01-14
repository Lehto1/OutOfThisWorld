using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    internal abstract class Insects : AIPathfinding
    {
        [Header("InsectSettings")]
        [SerializeField] private int insectDMG = 5; //dess skada
        [SerializeField] private float insectAttackCoolDown = 1.5f; // colldown
        private float insectAttackTimer = 0f; //tidtagning

        protected override void ExecuteIdle()
        {
            //insekten står stilla, gör inget 
            //väntar på händelser
            navAgent.velocity = Vector3.zero;
            
        }
        protected override void ExecutePatrol()
        {
            //Undersöker om insekten har nått "waypointen" 
            if(navAgent.remainingDistance < aiPointTole)
            {
                //insekten har nu kommit tillräkligt nära waypointen för att det 
                //det ska räknas att AI; har varit där 
                if(aiPointTImer >= aiTimeAtPoint)
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
            if(distanceToTarget <= aiAttackRadius)
            {
                ChangeState(AiState.Attack);
            }

            }
        }

    }
