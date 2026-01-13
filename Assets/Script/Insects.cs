using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    internal class Insects : EnemyPathMovement
    {
        [Header("Insect Settings")]
        public float health = 50f;

        void Start()
        {
            speed = 4.5f; // Insekter är snabbare
        }

        protected override void OnPathComplete()
        {
            // Insekter dör när de når slutet
            currentWaypointIndex = 0;

        }

        public void TakeDamage(float damage)
        {
            health -= damage;

            if (health <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            currentWaypointIndex = 0;

        }
    }

}
