using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    // Start is called once bef
    //
    //REFERENSER TILL Animator och ridigbody;
    private Rigidbody rb; 
    private Animator animator;

    //Animations PARAMETRAR
    private int isWalkingHash;
    private int isiSRunningHash;

    //Treshhold för movement
    [SerializeField] private float walkThresh = 0.5f;
    [SerializeField] private float runThresh = 3.5f;

    // ore the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hämtar 
        //Hämtar komponent rederenserna
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        //konverterar  PARAMETER     namn till hash
        isWalkingHash = Animator.StringToHash("isWalking");
        isiSRunningHash = Animator.StringToHash("isRunning");

        //error kontroll 
        if(animator == null )
        {
            Debug.LogError("Animationhandler missing Animator cpomonent on " + gameObject.name);
        }
        if(rb == null )
        {
            Debug.LogError("Animationhandler missing Rigidbody cpomonent on " + gameObject.name);

        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimationState();

    }
     void UpdateAnimationState()
    {
        if (animator == null || rb == null) return;

        //calculera vågrätt rörelsehastighet.
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        //bestäm om animationens tillstånd baserat på spelarens hastighet
        bool isWalking = horizontalSpeed > walkThresh;
        bool isRunning = horizontalSpeed > runThresh;

        //updaterar parametrarna
        animator.SetBool(isWalkingHash, isWalking);
        animator.SetBool(isiSRunningHash, isRunning); 
    }

}
