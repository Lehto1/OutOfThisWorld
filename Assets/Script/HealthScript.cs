using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //dESSA VÄRDEN KOMMER KONTROLERA SPELARENS hälsa
    [Header("Hälsainställnngar")]
    //varibel som laggrar Spelarens maximala hälsa 
    //för tilfället är denna 100 hp
    [SerializeField] private float maxHealth = 100f;

    //varibel för spelarens nuvarnade hälsa 
    [SerializeField] private float currentHealth;



    //staminan är spelarens energi

    //denna ennergi kommer senare påverkas andändas up av rörelse och 
    [Header("StaminaSettings")]

    //variablen för spelaren maximala energinivå
   [SerializeField] private float maxinumStamina = 100f;

    //Nuvarande stamina, spelarens energinivå i stunden
    [SerializeField] private float currentStamina;

    //En varibel för hur snabbt staminanivåerna återhämtar sig själva
    [SerializeField] private float staminaRegen = 4f;

    [Header("Wound Creation")]
    //En variabel med det minsta värde som spelaren behöver ta i skada för att ett "sår" ska skapas
    //om mindre än detta värde tas, kommer inget wound skapas
    [SerializeField] private float woundThresh = 5f;

    //detta kontrolerar hur mycket skada som beror på alvarighetsgraden
    [SerializeField] private float multiplierDamageTWound = 1f;
//
    [Header("HealthState")]
    //spelaren nuvariaga hälsotillstånd
    //vid liv, lidande eller död
    //Alive, Ailing, Dead

    [SerializeField] private HealthState state = HeathState.Alive;

    [Header("Wound list + extra")]
    //alla sår "wounds" representerar skador på spelaren kropp,
    //dessa skaddar kan natuligvis "infekteras" utav viruset
    //Skappar darför en lista på alla spelarens sår
    private List<Wound> wounds = new List<Wound>();

    private 

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
