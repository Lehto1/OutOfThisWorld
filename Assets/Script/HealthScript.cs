using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;

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
    [Header("Ui komponenter")]
    //Ui komponenter 
    [SerializeField] TextMeshPro healthText;
    [SerializeField] TextMeshPro StaminaText;
    [SerializeField] Image healthBar;
    [SerializeField] Image staminaBar;
    [SerializeField] float uiLerpingSpeed;




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
    [Header("HealthState and Effects")]
    //spelaren nuvariaga hälsotillstånd
    //vid liv, lidande eller död
    //Alive, Ailing, Dead

    //En hastighetsmodifierar baserat på spelarens hälsotillstånd 
    //kommer bla multiplicera sig med spelarens stamina och rörelse hastighet

    //Multiplikatiorn vid Healthy är 1, Påverkad därmed inte spelaren negativt
    //detta är standarden
    [SerializeField] private float speedMultiplierWhileHealthy = 1f;

    [SerializeField] private float speedMultiplierWhileInjured = 0.86f;

    [SerializeField] private float speedMultiplierWhileCritical = 0.65f;

    [SerializeField] private float speedMultiplierWhileDying = 0.35f;

    //Variablerna nedan är staminamodifierare som dem me är baserade på hälsotillståndsenumen
    //dess påverkar hur snabbt som stammina återhiämtar sig'
    //Vid normal
    [SerializeField] private float staminaMultiplierWhileHealthy = 1f;

    [SerializeField] private float staminaMultiplierWhileInjured= 0.85f;

    [SerializeField] private float staminaMultiplierWhileCritical = 0.65f;

    [SerializeField] private float staminaMultiplierWhileDying = 0.33f;

    // Varibler för DMG multiplikatorn, Denna multiplikator multipliceras med fiende skadan 
    //Destå svårare hälsotillsånd spelaren är i desto mer skada kommer spelaren att ta
    [SerializeField] private float dMGMultiplierInjured = 1.1f;

    [SerializeField] private float dMGMultiplierCritical = 1.30f;

    [SerializeField] private float dMGMultiplierDying = 1.4f;


    [SerializeField] private HealthState state = HealthState.Healthy; 

    ///fixxar vid senare tillf'lle



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
     

        //lenar Ui:N 
        uiLerpingSpeed = 3f * Time.deltaTime;
        //uppdatera det återhämtande stamminavärde en varje frame
        RegenStamina();

        UpdateUI(); //uPPDATERAR klasens kopplade UI komponenter

         
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

    //En metod som ppdaterar spelarens hälsotillstånd baserat på det nuvarnade
    // Metoden använder sig utav procentebestämma n angivna i enumen för att ävgöra vilket skick som spelaren
    // //befinner sig i
   public void  UpdatePlayerHealthState()
    {
        // Skapar en ny Healthstate variabel för det nya tillståndet
        HealthState newHeathState;

        //Beräknar spelarenns nuvariga Hp i procent 
        float hpPercentage = currentHealth / maxHealth;

        //jämför procenten med intervallen för de alla olika tillstånden
        //och utser state baserat på det
        if (currentHealth <= 0)
        {
            //spelaren har 0 maximalt, Är alltså död
            newHeathState = HealthState.Death;
        }
        else if (hpPercentage <= 0.14f)
        {
            // 0 - 14 hp
            //Ifall spelaren beffiner sig inom detta intervall så betyder det att spelaren är döende
            //Spelaren rörsig väligt sakta vid detta tillstånd
            newHeathState = HealthState.Dying;


        }
        else if (hpPercentage <= 0.40f) // 15- 40% HP
        {
            //Detta är det kritiska intervallet
            //Spelaren tar mycket mer skada 
            newHeathState = HealthState.Critical;

        }
        else if (hpPercentage <= 0.70f)
        {
            //Spelaren ligger någonstans mellan 41 till 70% resterande hp
            //Detta resluterar i att spelraten tar lite mer skada än normalt och att 
            //det exempelvis 
            newHeathState = HealthState.Injured;
        }
        else // 70 hp uppåt
        {
            // Detta är det "fRISKA" intevallet 
            //
            newHeathState = HealthState.Healthy;

        }

        //Kollar om tillståndet är det samma som förr eller om spelaren har byt
        //tillstånd
        if(newHeathState != state)
        {
            //lagrar koden den gamla staten i en egen variabel
            HealthState previousState = state;

            //uppdaterar state till det nya
            state = newHeathState;

            //Triggrar event ifall jag senare lägger till AI, ljud eller andra beroednde klasser
            OnStateChange?.Invoke(state);

            UnityEngine.Debug.Log($" The Healthstate has been changed, {previousState} --> {newHeathState}");

            //Kallar metod som applicerar tillståndsspecifissaeffekterna 
            ApplyHealthStateEffect(newHeathState);
        }

    }

    // Denna metod applicerar en lång rad olika effekter på spealren. När Hälsotillstånden ändras 
    //så kallas denna metod,(När en förändring i tillstånd upptäcks).
    //
    private void ApplyHealthStateEffect (HealthState newState)
    {
        //Använder switch-statemetn 
        switch (newState) {

    }


    //Kallas när en attack gör mer skada änn tresh
    public void CreateWound(float damage)
    {

        //skappaet ett nytt object av sorten wound
        Wound newWound = new Wound(woundID++,damage);


        //LÄGGER TILL TILL LISTAN
        wounds.Add(newWound);
        //triggrar evvent
        OnAdditionalWound?.Invoke(newWound);
     

        //logg
        Debug.Log($"New wound created, ID : {newWound.id}. DMG : {damage}, Tot wounds {wounds.Count}");
    }

        //retunerar lista med wound "sår" object 
        public List<Wound> GetWounds()
    {
        return wounds;
    }

    //Hämtar antallet sår
    //retunerar hur många sår spelaren har
    public int GetCountOfWounds()
    {
        return wounds.Count;
    }

    //en metod för att även hämta alla infekterade sår, räknar egenom och 
    private int GetCountInfectedWounds()
    {
        int count = 0;
        //looper egenom hela listan
        foreach (Wound wound in wounds)
        {
            if (wound.isInfected)
            {
                count++;
            }


        }
        return count;
    }

    //lääker specifika sår 
    //Tar bort ett sår från liostan

    public void HealAWound(int idWound)
    {
        //söker upp såred med samma ID
        Wound healThisWound = wounds.Find(w => w.id == idWound); ; //sö

        if(healThisWound != null )
        {
            //HITTAT SÅR
            //TAr bort det funna såret
            wounds.Remove(healThisWound);

            Debug.Log($"The wound {woundID} has healed and been removed");

        } else
        {
            //hittades inte
            Debug.LogWarning($"Could not match any woud to ID : {woundID}");
        }
    }

    //Ui FIller metoder nedan
    void UpdateUI()
    {
        //lämnar texterna tomma för tillfället 
        HealthbarFiller();
        StaminaBarFiller();
         //ändra färg på healthbar
         ColorChanger();
    }
     void HealthbarFiller()
    {
        //Fyller Ui 
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealth / maxHealth, uiLerpingSpeed); //Lerpar
    }
    void StaminaBarFiller()
    {
        staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, currentStamina / maxinumStamina, uiLerpingSpeed);
    }
    void ColorChanger()
    {
        Color healthColour = Color.Lerp(Color.red, Color.green, (currentHealth / maxHealth)); //kommer bilda ett

        healthBar.color = healthColour;
    }
    //Getter kod och Gettermetoder


   // public List<Wound> GetWounds()
   // {
    //    return wounds; // rettunerar listan 
  //  }
}

//Healthstate enum 
//alla de fyra olika tillstånden kommer att påverka spelaren på olika sätt
public enum HealthState
{
    Healthy, //omkring 100 - 70% Hp, Spelaren är vid detta stadie i en väldigt god from
    Injured, //Omkring 70 - 40 HP spelaren är skadad men är ändå i relativt gott tilstånd
    Critical, // 40 - 16 hp, 
    Dying, // 14 -  1 hp
    Death // 0% hp, Spelaren har dött
}

