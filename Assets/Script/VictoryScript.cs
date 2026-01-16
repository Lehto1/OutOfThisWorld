using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScript : MonoBehaviour
{
    [SerializeField] BombProgress BombProgressScript;

    private void OnTriggerStay(Collider other)
    {
        if (BombProgressScript.BombsLeft == 0)
        {
            Application.Quit();
        }
    }
}
