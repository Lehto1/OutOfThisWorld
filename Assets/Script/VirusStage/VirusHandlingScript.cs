using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

using System.Collections.Generic;



//Detta är virusmekanikens supperklass
//Alla stadier utav viruset är barn av denna kod
//Viruset kommer innefatta 4 stadier som progressivt ökar skadan och itensiteten utav de effekter som spelaren påverkas av
//De femta och sista stadiet kommer leda till ens död 

//Spelaren bygger dock resistans mot viruset, att injsukna blir mindre alvarlig vid den senare delen utav spelets gång
//smittngen uppstår från en extern trigger som  jag ännu inte gjort.
//Plannerar att man smittas när man blir attackerad 

public abstract class VirusHandlingScript : MonoBehaviour
{
    [Header("Virus Grundinställningar")]

    [SerializeField] protected string nameOfVirus = "Unknown"; //

    [Header("Fas/Stadie och Tidskontroll")]

    //Sätter värdet på det totala tiden sedan infrektion, 
    //Noll stäls om spelaren exempelvis dör av viruset
    [SerializeField] protected float totInfectionTime = 120f; //OBS kommer justeras senare. Lättare att testa med kortare väntetid

    //Tiden det tar fö´r viruset att gå från stadie 0/ett till stadie 1/ två Från Dormant till active
    [SerializeField] protected float dormantTime = 20; //OBS

    //Tiden det tar  för viruset att gå från Actvt till Critical
    [SerializeField] protected float activeTime = 50;

    //Critical --> terminal
    [SerializeField] protected float criticalTime = 80;

    [Header("sTAMINADRAIN")]
    // DRAIN VID aaktivt virus
    [SerializeField] private float drainOfStaminaActive = 10;

    [SerializeField] private float drainOfStaminaCritical= 10;



    [Header("Skada och ")]
    //basskadan per sekund under den aktiva fasen,
    //skaddan kommer upptrappas senare 
    [SerializeField] protected float baseActiveDPS = 0.003f;

    //En multiplicator under the kritiska fasen
    [SerializeField] protected float criticalFactor = 2.2f;

    //Enn multiplicator för den dödliga fasen
    [SerializeField] protected float terminalFactor = 3.5f;

    //en stamina drain per sekund under den aktiva fasen
    [SerializeField] protected float baseActiveStaminaReductionPS;

    //under den dödliga fasen
    [SerializeField] protected float terminalStaminaReduction;

    // Hur mycket viruset ökar i styrka per minut
    //mutation
    [SerializeField] protected float mutationsPM = 0.20f;

    // Hur snabbts som spelaren bygger ett immunförsvar mot viruset
    [SerializeField] protected float immunmAcumalationPM = 0.08f;

    //Den maximala virus ressistansen , [Range(0f, 1f)] protected float maxinumVirusResistans = 0.6f;en skala mellan 0 ck 1
    [SerializeField][UnityEngine.Range(0f, 1f)] protected float maxinumVirusResistans = 0.6f;

    [Header("Sår och sårsmittnin")]

    //varje sekund så finns det n tjans tt viruset smittar befintliga skador.
    //Att ha mpnga infekterade sår gör så at man tar extra skada
    [SerializeField] protected float woundInfectionChance = 0.03f;

    //Man kommer skadas extta fråme infekterade skador  p2weer sekun
    [SerializeField] protected float extraDPSPerWound = 0.4f;

    //Nu kommer alla runtime variablernna

    // En variabel för hur länge viruset  har funnits i spelaren.
    //kommer börja vid 0 och öka varj  frame
    protected float infectionTime = 0f;

    //Håller reda på virusets  nuvariga fas BASERAT PÅ INFEKTIONSTIDEN OVAN
    protected VirusStages curretStage = VirusStages.Dormant; // Börjar som lattent

    //Variable för immunförsvarets nuvarande nivå
    protected Immunity immunityLevel = Immunity.No; //börjar spelet med

    // selarens ressistans
    protected float currentResistance = 0f;

    //Variabel för virusetts ''mutationsnivå''
    //hur myckt starrkare än start virust har blivigt
    protected float mutationLevel = 1f;

    //lista på spelarens infekterade skador och sår
    protected List<Wound> infectedWounds = new List<Wound>();

    //PROTECTED Health
    //referens till spelarens hälsa
    protected HealthScript health;

    //Framtida refferens till kontroller //
    //-------------------------------&//
    //------------------------/////-

    //muttationstimer som uppdateras varje sekund
    private float mutationTimer = 0f;

    //Timerr för imunn
    private float immuneTimer = 0f;

    //Timer för wound infektion
    private float woundTimer = 0f;


    //Har events som triggras när virus byter mellan faserna
    public event Action<VirusStages> OnStageChange;

    //Denna acction triggras när immunförsvaret bygger upp
    //sänder ny paramete
    public event Action<Immunity> OnImmunityChange;

    //tRIGGRAS när virus muteras
    //
    public event Action<float> OnMutation;

    //Event som triggras när viruset botas 
    public event Action OnCured;

    //Säkerhetsgejor
    protected virtual void Awake()
    {
        //hittar Healthkoden, då den måste finnas
        //koden fungerar inte uta den
        health = GetComponent<HealthScript>();
        if (health == null)
        {
            Debug.LogError($"The [{nameOfVirus}] virus needs the heathscript to be on the same gameobject as itself");
            enabled = false;
            return;
        }

        //här kommer koden för at hitta spelar kontrollerna vara
        //---------------------------------//

        //Hälsa events
        //Fixar senare när jag skaar hälsa scriptet
        //----------------------------------//

        // health.OnWoundAdded += ControllWounInfection;
        //  health.OnSate.Changed += ControllHealthState;

    }

    protected virtual void OnEnable()
    {
        //Virusett kommer nu att aktiveras
        //
        infectionTime = 0f; //Tiden sätts till 0
        mutationLevel = 1f;
        currentResistance = 0f;//nollställer 
        infectedWounds.Clear();
        curretStage = VirusStages.Dormant; //nollställer
        immunityLevel = Immunity.No; //nollställer

        Debug.Log($"{nameOfVirus} Infection started");

    }












    //Skapar ett enum för virusets olika faser
    //Fasen kommer avgöra hur spelaren påverkas
    public enum VirusStages
    {
        Dormant = 0, // Det första stadiet, viruset påverkar knappt spelaren
        Active = 1, //Det andra stadiet, Spelaren börjar få små biverkningar, Ingen tick skada ännu 
        Critical = 2, // Det tredje stadiet, Spelaren har vid det här laget börjat ta tick DMG. Ännu svårare biverkningar
        Terminal = 3, // Detta stadie har möjlighet att ta livet av spelaren. Extrema biverkingar och DMG
    }

    //Skapper även Immun enums 
    public enum Immunity
    {
        No = 0, // Spelaren har ännu inte smittats och har därför inte något försvar mot viruset
        Little = 1, // spelaren har bekämpat virusett förr och har därmed ett försvar mot det
        Some = 2, // Ett godtyckligt försvar
        Moderate = 3, // Ett relativt starrkt försvar
        Strong = 4, // Ett mycket start försvar. 
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    protected virtual void Update()
    {

        // kontrollerar ifall spelaren är vod liv
        //slutar viruset köra
        if (health == null || health.State == HealthState.Death)
        {
            enabled = false;
            return;
        }

        //uppdaterar tidtagningen
        infectionTime += Time.deltaTime;

        //Uppdaterar virufasen baserat på tiden som har passerat
        UppdateStageOfVirus();

        //Applicerar deb grundläggliga symtomen här
        ApplyBaseSymt();

        //Bygger immunförsvar mot virrusett
        //  UppdateraImmunsystemet
        UpdateImmuneSystemm();

        UpdateVirusMutation();

        //inificerar befintliga skador och sår
        InfectWounds();

        //Alla barnklasser kommer här kunna skicka in sina överiga effekter på spelaren
        IndividualPlayerEffect();

        //kontrollerar om spelarens tid är uppe
        if (infectionTime >= totInfectionTime)
        {
            CureVirus();
        }
    }

    protected virtual void OnDisable()
    {
        //rensar alla eventdw nöär viruset tas bort från spelaren
        if (health != null)
        {
            health.OnAdditionalWound -= ControllWoundInfection;
            health.OnStateChange -= ControllHealthState;

        }
    }

    //En metod som väljer fas baserat på den tid som gått
    private void UppdateStageOfVirus()
    {
        //tidbaserat
        VirusStages newStage;
        //ifall infektiontiden visars sig vara under latenttid, behålls viruset latent
        if (infectionTime < dormantTime)
            newStage = VirusStages.Dormant;
        else if (infectionTime > activeTime)
            newStage = VirusStages.Active; //om 
        else if (infectionTime < criticalTime)
            newStage = VirusStages.Critical;
        else
            newStage = VirusStages.Terminal;

        //vid ändring av fas så triggras eventet och debugg
        if(newStage != curretStage)
        {
            curretStage = newStage;
            OnStageChange?.Invoke(curretStage);

            Debug.Log($" The {nameOfVirus} virus stage changed to {curretStage}, It's mutation level is at {mutationLevel}, Player resistane at {currentResistance}.");

                
        }

    }
    //
    //
    private void ApplyBaseSymt()
    {
        //Kalkulerar den aktuella skada på spelaren

        //Den grunläggliga skadan multiplicerar med fas-faktorn
        float stageMulti = GetDamageMultiplierForStage(curretStage);

        //multiplicerar mutation med skadan
        float mutatedDPS = baseActiveDPS * stageMulti * mutationLevel;

        //Extra skada för mängcen beffintliga skador och sår
        float woundInfectDPS = infectedWounds.Count * extraDPSPerWound;

        //Total på spelare skada,mpre resistanse
        float totalDPS = mutatedDPS + woundInfectDPS; //kombinerar alla skador

        //Applicerar spelarens resistans till skadan
        float endDPS = totalDPS * (1f - currentResistance);

        //Applicerar skadan på spelaren
        //OBBS OBBS FUNKAR INTE ÄNNU DÅ JAG GINTE HAR SKRIVIT HÄLSKODEN 
        //------------------------------//
        if (endDPS > 0f)
        {
            health.ApplyDMG(endDPS * Time.deltaTime);
        }
        //StaminaDrain rader nedan
        float drainOfStamina = (curretStage == VirusStages.Critical || curretStage == VirusStages.Terminal) ? drainOfStaminaCritical : drainOfStaminaActive;

        health.UseStamina(drainOfStamina * Time.deltaTime);
    }

    private float GetDamageMultiplierForStage (VirusStages stage)
    {
        //retturnerar en skademultpicator
        //fas-beroende 
        return stage switch // switch över de olika
        {
            VirusStages.Dormant => 0.2f, //dolmad
            VirusStages.Active => 1f,// 100 procent max styrka
            VirusStages.Critical => criticalFactor,
            VirusStages.Terminal => terminalFactor, _
            => 1f
        };
    }




    //En motod som kontrlorerar och bygger upp Immun variablen. 
    //skyddar spelen i längden
 private void UpdateImmuneSystemm()
    {
        //Immunsystmet kommer bygga upp en resitans mot viruset över tid

        immuneTimer += Time.deltaTime;

        //öker spelarens resistnas varje skund OBS KOMMER ÄNDRA SENARE
        //äNDRAR SENARE -----------/////////-----------/////////
        if (immuneTimer >= 1f)
        {
            float increasedResistance = immunmAcumalationPM / 60f; //varje frame
            currentResistance = Mathf.Clamp01(currentResistance + increasedResistance);

            //kontroll , kollar om resitansen har nått en ny nivå
            Immunity newImmunityLevel = ChooseVirusImmy(currentResistance);

            ////om den nya nivån inte är like med den befintliga så..
            if (newImmunityLevel != immunityLevel)
            {
                immunityLevel = newImmunityLevel;
                OnImmunityChange?.Invoke(immunityLevel); //håller koll påm förändrin gar
                Debug.Log($"The {nameOfVirus} virus imms has been increased to {immunityLevel}");
            }

            //nollstller även tidtagningen
            immuneTimer = 0f;

        }
    }
    //bestämmer immNivå basserat på resistansvärdet
    private Immunity ChooseVirusImmy(float resistance)
    {
        if (resistance < 0.2f) return Immunity.No;
        if (resistance < 0.4f) return Immunity.Little;
        if (resistance < 0.6f) return Immunity.Some;
        if (resistance < 0.8f) return Immunity.Moderate;
        return Immunity.Strong; // 

    }

    //kontineurlig Mutations mekanik 
    //Viruset kommmer med denna metod kunna förstärka sig själv 

    private void UpdateVirusMutation()
    {
        //   uppdaterar timern 
        mutationTimer += Time.deltaTime;

        //viruset kommer att mutera vadera minut
        if (mutationTimer >= 62f)
        {
            float mutationIncrease = mutationsPM; // ökningen blir den samma som den redan existerande variabeln
            mutationLevel += mutationIncrease;

            //event ......
            OnMutation?.Invoke(mutationLevel);

            //dubbugar
            Debug.Log($"The [{nameOfVirus}] virus mutation level:{mutationLevel}");

            //nollställer timern 
            mutationTimer = 0f;

        }
    }

    // VBirusetkoden försöker inficera skadorna 
    //om viruset lyckas så kommer tick skadan öka, spelaren skulle då ta mer skada per sekund
    private void InfectWounds()
    {
        //
        woundTimer += Time.deltaTime;

        //kollar för infektion i skadorna
        if(woundTimer >= 1f)
        {
            //hÄMTAR ALLA SKADOR SOM SPELAREN HAR SAMLAT PÅ SIG
            //SKAPAR EN VAR FÖR DETTA
            var playersWounds = health.GetWounds();

            //loopar egenom alla skador i listan
            foreach( var wound in playersWounds )
            {
                //kollar om skadan i fråga redan är smittat 
                //ifall den är det, hoppar jag över denna wound 
                if(!wound.isInfected)
                {
                    //slumpar ett värde /chans 
                    //om det slumpade värdet är mindre än chansvärdet så smittas skadan
                    if (UnityEngine.Random.value < woundInfectionChance)
                    {
                        //smittat
                        wound.isInfected = true;

                        //lägger till i listan av smittade skador
                        if (!infectedWounds.Contains(wound))
                        {
                            infectedWounds.Add(wound);

                            //
                            Debug.Log($"[{nameOfVirus}] Infected wound ; {wound.id}!");

                        }
                    }
                }
            }
            //
           woundTimer = 0f;
        }

    }
    
    //kallas när spelaren får en ny skada//OBS// skade system kommer sen, har inte hunnit med ännu
    //
    private void ControllWoundInfection(Wound wound)
    {
        // 
        if(UnityEngine.Random.value < woundInfectionChance * 2f)
        {
            //markerar som smittad
            wound.isInfected = true;

            //lägger till i listan
            infectedWounds.Add( wound);

            //
            Debug.Log($"{nameOfVirus} infected wound {wound.id}");
        }

    }

    //Detta håller koll på spelarens hälsa, vid ändringar 
    //OM SPELAREN INT ELÄNGRE ÄR VID LIV SÅ KOMMER koden at stänga
    private void ControllHealthState(HealthState newHealthState)
    {
        //koollar ifall spealren forfarande lever
        if(newHealthState != HealthState.Alive)
        {
            //stänger av viruset
            enabled = false;

            Debug.Log($"{nameOfVirus} The player is now {newHealthState} The virus is therfore  deactivating");

        }
    }
   
    //metoden nedan anropas då spelaren exempelvis använder ett botemedel vilket orsakar att viruset botas
    //exempelvis med medicinering 
    private void CureVirus()
    {

        OnCured?.Invoke();
    }
    //denna metod är till för de olika barnklasserna 
    //är därmed abstractc
    protected abstract void IndividualPlayerEffect();

    //På en skala mellan 0 och 1 hämtar jag här hur pass långt viruset har verkat
    public float GetInfectionprogress() => Mathf.Clamp01(infectionTime / totInfectionTime);

    public VirusStages GetCurretStage() => curretStage; // möjligör en extern hämtning utav den nuvariga fasen 

    public float GetMutationLevel() => mutationLevel; // möjliggör en extern hämt utav mutationsnivån 

    public float GetImmuneResitanceLevel() => currentResistance;



    public string GetVirusName() => nameOfVirus;
    

    





}







