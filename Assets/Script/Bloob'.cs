using UnityEngine;

public class GoopBall: MonoBehaviour
{
    [Header("Effect")]
    //Hur länge bollens effect va´rar
    [SerializeField] private float goopDuration = 4f; //hur länge effecten varar

    [SerializeField] private float slownessMultiplier = 0.5f;  // hur mycket långsamare spelaren blir 

    [SerializeField] private float goopDMG = 7f; //sakdan som inträffar vid boll träff

        [Header("Settings")]
    [SerializeField] private float allowedTime = 10f; //Hur länge bollen finnds innan den förstörs

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //förstör bollen efter den bestämda tiden om den inte träffar något
        Destroy(gameObject,allowedTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
     // Kollar om bolle träffade spelaren
     if(collision.gameObject.CompareTag("Player"))
        {
            //applicerar effecten
            //refererar till rörelse koden
            Movement playerMovement = collision.gameObject.GetComponent<Movement>();
            if(playerMovement != null)
            {
                //applicerar
                playerMovement.ApplySlowness(slownessMultiplier, goopDuration);
            }

            //applicerar skada
            HealthScript playerHealth = collision.gameObject.GetComponent<HealthScript>();
            if (playerHealth != null)
            {
                //applicerar
                playerHealth.ApplyDMG(goopDMG);

            }
            Debug.Log("Goop ball hit player");

        }
     Destroy(gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
,