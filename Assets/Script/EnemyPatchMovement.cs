using UnityEngine;

public class EnemyPathMovement : MonoBehaviour
{
    public Transform[] waypoints;   // Punkter på kartan
    public float speed = 3f;
    public float rotationSpeed = 5f;

    public int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Transform target = waypoints[currentWaypointIndex];

        // Rörelse
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotation (så fienden tittar dit den går)
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );

        // När waypoint nås
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.2f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                // Exempel: förstör fienden när rutten är klar
                currentWaypointIndex = 0;

            }
        }
    }
}

