using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

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

    [SerializeField] private HealthState state = HealthState.Alive; ///fixxar vid senare tillf'lle



    [Header("Wound list + extra")]
    //alla sår "wounds" representerar skador på spelaren kropp,
    //dessa skaddar kan natuligvis "infekteras" utav viruset
    //Skappar darför en lista på alla spelarens sår
    private List<Wound> wounds = new List<Wound>(); //Wound finns inte ännu, Kommer fixa vid seanre tillfälle

    //wound räknare, Varibeln som senare kommer hållareda på 
    //cara skada kommer att få unik id

    private int woundID = 0;

    //Event som kommer att triggras vid stör
    //När ett nytt sår "Wound" skapas 
    public event Action<Wound> OnAdditionalWound; // kommer funka senare när jag skapar wound

    //triggras när splearen övergår mellan olika "Healthstates" exempoelvis vid död
    public event Action<HealthState> OnStateChange;

    //triggras när spelaren tar skada
    public event Action<float> OnDamageTaken;

    public event Action<float> OnStaminaChnge;

    //Skappar getters
  //för det nuvarande hälsotillståndet
  public HealthState State => state;

    //getter för currentHhealth
    public float CurrentHP => currentHealth;

    //getter för MaximalHp
    public float MaxHP => maxHealth;

    //getter för de nuvrande energinivåerna
    public float CurrentStamina => currentStamina;

    //Getter för spealren maximala stamina
    public float MaxStamina => maxinumStamina;

    //Ui
    //retunerar HP i procent
    public float HPPercent => currentHealth / maxHealth;

    //retunerar Stamina procent
    public float StaminaPercent => currentStamina / maxinumStamina;
        
    void Start()
    {
        //sätter energi och häls värdena till maximalt vid kodens start
        currentHealth = maxHealth; //sätts till max
        currentStamina = maxinumStamina; //sätt till max

        Debug.Log($"Player init, Health level:{currentHealth/maxHealth}, Stamina:{currentStamina/maxinumStamina}");//säkerhet
    }
  

    // Update is called once per frame
    void Update()
    {
        //uppdatera det återhämtande stamminavärde en varje frame
        RegenStamina();
    }

    //En metod som applicerar skada på spelarens hälsa, 
    //om skadan överstiger tresholden så skapad det sår.
    public void ApplyDMG(float dMG)
    {
        //validerar, 
        //spelaren kommer inte konna ta skada efta att spealern redan dött
        if (state == HealthState.Death)
        {
            //Informerar konsollen
            Debug.Log("Player has already died, can not take DMG");
            return;

        }
         //minskar spelarhälsan
         currentHealth -= dMG;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 

        //Triggrar DMG eventet
        OnDamageTaken?.Invoke(dMG);

        Debug.Log($"Player took {dMG} damage, Which reduced player HP to {currentHealth / maxHealth} ");

        //ifall dmg värdet som spelaren tog 
        //är så pass tor så att den överstiger trhesholden 
        //skappas ett sår
        if(dMG > woundThresh)
        {
            //kallar metod som skapar skador  utifrån DMG värdet
            CreateWound(dMG);
        }

        //Uppdateara hälsotillståndet
        //då spelaren nyligen tog skada 
        UpdatePlayerHealthState();
    }

    //spelaren återfår hälsa,
    //Denna metod healar en viss mängd hälsa.
    public void HealPlayer(float healingAMount)
    {
        //valliderar 
        if( healingAMount <= 0)
        {
            Debug.LogWarning("HealingAmount has to be above 0");
            return;
        }
        //ökar spelarens hälsa 
        currentHealth += healingAMount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player healed {healingAMount} HP , Current health : {currentHealth / maxHealth}");

               //Uppdaterar här spelarens hälso tillstånd,
    UpdateHealthState();

    }

    //En metod som uppdatera spelaren hälsotillstånds baserat på
    //den nuvarande hälsan
    private void UpdateHealthState()
    {
        HealthState newState;

        //bestämmer det nya tillstondet baserat på 
        //Mängden HP
        if (currentHealth <= 0)
        {
            newState = HealthState.Death; // om Hp är lika med eller är mindre än 0, så är spelaren död
        }
        else if (currentHealth < maxHealth * 0.33f)
        {
            newState = HealthState.Ailing; // om Hp är under 33 procent

        }
        else
        {
            newState = HealthState.Alive;
        }

        //Om tillståndet förrändras, triggras detta evvent
        //för då har det skett en förändring
        if (newState != state)
        {
            state = newState;
            //ivokar 
            OnStateChange?.Invoke(state); //

            Debug.Log($"Healthstate cahnged to {state}");

        }
    }

    //Metod för att använda stamina, vilket minskar spelaren ´s enegri
    //Denna mettod kallas när spealren gör något som förbrukar energi 
    public void UseStamina(float stmAmount)
    {
        //Det går ite för denna metod att använda 0 stamina,
        //dätmed so kommer metod returna ifall stmAmount är 0

        if (stmAmount <= 0)
        {
            Debug.LogWarning("Stamina requirement not ");
            return;

        }

        //minskar stamina
        currentStamina -= stmAmount;

        //jag säkkerställer h'r att stamina värdet aldrig går utanför rimliga värde. 
        //går inte över max, går inte till minus
        currentStamina = Mathf.Clamp(currentStamina,0,maxinumStamina);

        // triggrar event onStaminachanged
        OnStaminaChnge?.Invoke(currentStamina);

        Debug.Log($"Stamina used : {stmAmount}, New stamina amout : {currentStamina}/{maxinumStamina}");

    }

    //Denna metod kallas från update varje frame.
    //Tillåter en ölångsam återhämtning av stamina
    //StaminaRegen styr hastighetn
  private void RegenStamina()
            {
        ////Om stamina inte är full, så återhämtas den
        if(currentStamina < maxinumStamina)
        {
            // ökar stmaina paserat på deltatime
            currentStamina += staminaRegen * Time.deltaTime;

            // säkkerställer att värdet allrig överstiger max
            currentStamina = Mathf.Clamp(currentStamina,0,maxinumStamina);

            //On
            OnStaminaChnge.Invoke(currentStamina);
        }

            }
   
    //Kontrollerar om spelaren har tillräckligt med stamina 
    //Rettunerar om stamina överstiger kravvärdet

    public bool HasSufficentStamina(float requiredAmount)
    {
        return currentStamina >= requiredAmount;
    }

   public void  UpdatePlayerHealthState()
    {

    }


    //Kallas när en attack gör mer skada änn tresh
    public void CreateWound(float damage)
    {
        //skappaet ett nytt object av sorten wound
        Wound newWound = new Wound
        {
            //ger skadan ett unikt IFD
            id = woundID++, //öknar ID räknaren
            //blir inte  infekteran ännu
            isInfected = false,
        };

        //LÄGGER TILL TILL LISTAN
        wounds.Add(newWound);

        //triggrar event

    }
    //Getter kod och Gettermetoder


    public List<Wound> GetWounds()
    {
        return wounds; // rettunerar listan 
    }
}

//Healthstate enum 
//alla de tre olika tillstånden
public enum HealthState
{
    Alive,
    Ailing,
    Death
}

