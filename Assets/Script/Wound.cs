using UnityEngine;
using UnityEngine.Video;

public class Wound : MonoBehaviour
{

    // "Sår" kalssen 
    public int id; // sårets ID nummer 
    public float damage; //såret skada
    public float woundSeverity;
    public bool isInfected; // flagg som signerar ifall skadan är infekterad eller inte
    public float infectionTime;

    // Den tid som det kommer ta för infekteras automatiskt
    float timeUntillAutoInfection;

    //Hur infekterat såret är
    public float virusLoad = 0f;

    //Den maximala graden utav infektion
    public float maxVirusLoad = 100f;

    //

    //en konstruktor för skapelsen utav skador
    public Wound(int woundID, float damageAmount)
    {
        id = woundID;
        this.damage = damageAmount;
        woundSeverity = damageAmount * 1.5f;
        isInfected = false;
        infectionTime = 0f;

        //Beräknar tiden innan auto
        timeUntillAutoInfection = woundSeverity * 3f;
    }

    //Metod som uppdaterar sårets infektionsstatus 
    public void UpdateWoundInfection(float deltaTime)
    {

        //Kontrollerar om såret inte är infekterat
        if(!isInfected)
        {

            //ÖKAR den totala tiden 
            infectionTime += deltaTime;

            //Kontrolerar om det har gått tillräckligt länge för såret att bli auto infekterat
            if(infectionTime >= timeUntillAutoInfection)
            {
                //Såret infekteras
                InfectWound();
            }
        }
        else
        {
            //hinner itne
           
        }
    }

    //Metod som manuelt/automatiskt "smittar" såret i fråga
    public void InfectWound()
    {
        // kontrolla så att såret inte redan är infekterat
        if(!isInfected)
        {
            isInfected = true;

            //sätter load
            virusLoad = 15f;
        }
    }

    //Minskar virus
    public void TreatInfection(float treatment)
    {
        //mINSKAR virusbelastningen
        virusLoad -= treatment;

        //clampar så att den inte går under 0
        virusLoad = Mathf.Clamp(virusLoad, 0f, maxVirusLoad);

        //Om belastningen understiger/når 0, botas såret
        if(virusLoad <= 0f)
        {
            isInfected = false;

        }
    }
    public override string ToString()
    {
        //Visar olika status beroende på om det är ett infekterat sår
        string woundStatus = isInfected ? $"Infected ({GetInfectionPercentage()}%" : "Healthy";
       
     
        return $"ID:{id}, woundStatus:{woundStatus}, DMG:{damage}, VirusLoad: {virusLoad}/{maxVirusLoad}";
    }
    //Reetunerar infektionstatusen
    public float GetInfectionPercentage()
    {
        return (virusLoad / maxVirusLoad) * 100f;
    }
    //fyller sedan
    //ifall vi hinner 
}
