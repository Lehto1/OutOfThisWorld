using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Movement : MonoBehaviour
{
    int Running = 1; //Variabel som ändrar på spelarens hastighet
    bool Crouching = false; //Bool som bestämmer om spelaren crouchar eller inte

    Rigidbody RB;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        IsCrouching();
        RunningOrNot();
        MovementStuff();
    }

    void MovementStuff() //Includes keybinds and code for moving the player
    {
        RB.angularVelocity = new Vector3(0, 0, 0);
        RB.linearVelocity = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W) || Crouching == false) // If sats for att go bakåt och framåt när man står up
        {
            RB.linearVelocity = Running * transform.forward;
        }
        else if (Input.GetKey(KeyCode.S) || Crouching == false)
        {
            RB.linearVelocity = Running * -transform.forward;
        }

        if (Input.GetKey(KeyCode.W) || Crouching == true) // If sats for att go bakåt och framåt när man är crouching
        {
            RB.linearVelocity = transform.forward;
        }
        else if (Input.GetKey(KeyCode.S) || Crouching == false)
        {
            RB.linearVelocity = -transform.forward;
        }


        if (Input.GetKey(KeyCode.A)) //If sats för att rotera din käraktär
        {
            RB.angularVelocity = new Vector3(0, -10, 0);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            RB.angularVelocity = new Vector3(0, 10, 0);
        }
    }

    void RunningOrNot()
    {
        if (Input.GetKey(KeyCode.LeftShift)) //Används för att ändra spelarens hastighet när de springer
        {
            Running = 4;
        }
        else
        {
            Running = 2;
        }
    }

    void IsCrouching()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Crouching == false)
        {
            Crouching = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) || Crouching == true)
        {
            Crouching = false;
        }
    }
}
