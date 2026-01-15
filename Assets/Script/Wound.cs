using UnityEngine;

public class Wound : MonoBehaviour
{

    // "Sår" kalssen 
    public int id; // sårets ID nummer 
    public float damage; //såret skada
    public float woundSeverity;
    public bool isInfected; // flagg som signerar ifall skadan är infekterad eller inte
    public float infectionTime;

    //en konstruktor för skapelsen utav skador
    public Wound(int woundID, float damageAmount)
    {
        id = woundID;
        this.damage = damageAmount;
        woundSeverity = damageAmount * 1.5f;
        isInfected = false;
        infectionTime = 0f;
    }

    public override string ToString()
    {
        string woundStatus = isInfected ? "Infected" : "Healthy";

        return $"ID:{id}, woundStatus:{woundStatus}, DMG:{damage}";
    }
    //fyller sedan
    //ifall vi hinner 
}
