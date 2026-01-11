using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;


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
    [SerializeField][Range(0f, 1f)] protected float maxinumVirusResistans = 0.6f;

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


    //





    
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
        No= 0, // Spelaren har ännu inte smittats och har därför inte något försvar mot viruset
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
    void Update()
    {
        
    }
}
