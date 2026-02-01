using UnityEngine;
using UnityEngine.Rendering;

public class GoopBall: MonoBehaviour
{
    [Header("Effect")]
    //Hur länge bollens effect va´rar
    [SerializeField] private float goopDuration = 4f; //hur länge effecten varar

    [SerializeField] private float slownessMultiplier = 0.5f;  // hur mycket långsamare spelaren blir 

    [SerializeField] private float goopDMG = 7f; //sakdan som inträffar vid boll träff

    [Header("Spread Goop")] //SPridning

    [SerializeField] private float goopSpreadRadius = 4f; // Radie för splash effekter 

    [SerializeField] private AnimationCurve spreadingFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0); //Dess falloff

    [Header("Settings")]

    [SerializeField] private float allowedTime = 10f; //Hur länge bollen finnds innan den förstörs

    [SerializeField] private ParticleSystem goopParticles; //Partiklar vid träff

    private Rigidbody rb;
    private bool hasHitPTarget = false; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hämtar rigidbody
        rb = GetComponent<Rigidbody>();

        //förstör bollen efter den bestämda tiden.
        Destroy(gameObject,allowedTime);
    }

    private void OnCollisionEnter(Collision collision)
    {

        // Kollar om bolle träffade spelaren
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerGoopHit(collision.gameObject);
            return;
        }
        //Om bollen träffade något annat skall den studsa 
        if (rb != null)
        {
            rb.linearVelocity *= goopBounciness; //mminskar hastigheten vid studds

        }
    }
   private void HandlePlayerGoopHit(GameObject player)
    {
        //Träffhantering vid spelar-träff

        //säkerhetkontroll 1
        if (player == null) return;

        //Applicerar långsamhet på spelaren
        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement != null)
        {
            playerMovement.ApplySlowness(slownessMultiplier, goopDuration);
            Debug.Log("Player slowness applied");
        }
        
            // Applicera skada på spelarenm
            HealthScript playerhealth = player.GetComponent<HealthScript>();
       if(playerhealth != null)
        {
            playerhealth.ApplyDMG(goopDMG);
            Debug.Log($"Spider Goop hit the player");
        }

        //Splash Effekt omkirng spelaren
        ApplySplashEffect(player);

        //Partikel effekt 
        if(goopParticles != null)
        {
            //Placerar partikelobjectet
            Instantiate(goopParticles, transform.position,Quaternion.identity);
        }

        //Förstör sedan bollen 
        hasHitPTarget = true;
        Destroy(gameObject);
    }

    private void ApplySplashEffect(GameObject hitTarget)
    {
        //Sprider och träffar ärligande ovject kring kollisions punkten

        Collider[] allNearbyColliders = Physics.OverlapSphere(transform.position, goopSpreadRadius);

        //Loopar egenom varje närliggande Collider
        foreach (Collider collider in allNearbyColliders)
        {
            //Hoppa över colliders som redan har blivigt träffade
            if(collider.gameObject == hitTarget) continue;

            //Hoppar över Spindelns egna collider
            if (collider.CompareTag("Spider")) ;

            //Beräknar en falloff baserat på avstånd
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            float falloff = spreadingFalloff.Evaluate(Mathf.Clamp01(distance / goopSpreadRadius));

            //Applicerar en redducerad skada på de föremål sim träffas av skvätten

            HealthScript splashedHealth = collider.GetComponent<HealthScript>(); ;
            if (splashedHealth != null)
            {
                splashedHealth.ApplyDMG(goopDMG * falloff * 0.5f); //50%


            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
