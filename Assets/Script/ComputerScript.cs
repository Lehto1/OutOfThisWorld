using TMPro;
using UnityEngine;

public class ComputerScript : MonoBehaviour
{
    // Bomb relaterat variabler
    bool Planted; 
    [SerializeField] GameObject Bomb;
    [SerializeField] Vector3 PlantLocation;
    public BombProgress BombProgressScript;

    // Text variabel
    [SerializeField] TextMeshProUGUI PressEText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PressEText.enabled = false;
        Planted = false;
    }

    private void OnTriggerStay(Collider other) // Lägger till text när spelaren är nära datorn och sånt
    {
        if (Planted == false)
        {
            PressEText.enabled = true;
        }
        PlantBomb();
    }

    private void OnTriggerExit(Collider other) //Blir av med texten när spelaren går iväg från datorn
    {
        PressEText.enabled = false;
    }

    void PlantBomb() // Gör saker som att justera planted variabeln och minskar bombsleft variabeln med 1
    {
        if (Input.GetKeyDown(KeyCode.E) && Planted == false)
        {
            Planted = true;
            CreateBomb();
            BombProgressScript.BombsLeft -= 1;
            PressEText.enabled = false;
        }
    }

    void CreateBomb() //Creates and alters the size of the bomb
    {
        GameObject BombVariable = Instantiate(Bomb, PlantLocation, Quaternion.identity);
        BombVariable.transform.localScale = new Vector3(50, 50, 50);
    }
}
