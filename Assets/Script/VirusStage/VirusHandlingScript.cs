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
