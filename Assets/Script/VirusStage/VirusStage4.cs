using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class VirusStage4 : VirusHandlingScript
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Stage 4 Terminal settings")]

    [SerializeField] private float seizingIntensity = 1.0f; // Dess styrka /kraft
    [SerializeField] private float seizingInterval = 0.5f; //Applicerars var 0.5 sekund
    [SerializeField] private float extraSlowness = 0.3f; // låhastighet

    //Runtime variabler
    private float siezeTimer = 0f; //Räknar
    private bool isStage4Active = false; //Boolean flag, indikerar om koden befinner sig i stadie 4 eller ej
    private Movement playerMovement;

    //Finn momvement-scriptet
    protected override void Awake()
    {
        base.Awake();

        //hittar Movementscriptet
        playerMovement = GetComponent<Movement>();

        if(playerMovement == null )
        {
            Debug.LogWarning("Stage 4, Movement-scirpt was NOT found ");
        } else
        {
            Debug.Log("[sTAGE 4] Movement has been found");
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        //Registreara event listeners för stage 4 
        OnTerminalVirus += OnStage4Activated; //Lyssnare för Terminal aktivring

        OnChangedMutation += OnEscelatedMutation; //Lyssnarre för mutationer

        OnRemovedVIrus += OnStage4Removed; //Kyssnare för borttagning

        Debug.Log("[Stage 4] Registered event lsiteners");

    }

    protected override void OnDisable()
    {
        // AvRegistrera event listeners
        OnTerminalVirus -= OnStage4Activated;
        OnChangedMutation -= OnEscelatedMutation;
        OnRemovedVIrus -= OnStage4Removed; //

        base.OnDisable();

        Debug.Log("[Stage 4] Unregistered event lsiteners"); 
    }

    //Denna funktion tiggras när viruset blir terminalt 
    //Virus stadium
    private void OnStage4Activated()
    {
        //säkerhetskoll 
        if(GetCurretStage() == VirusStages.Terminal)
        {
            //Sätter boolean flag till true
            isStage4Active = true;

            Debug.Log("Stage 4 : Virus Terminal, Seizure and slowness");

        }
      
    }
   
    //Denna funktion troiggras när mutationen ökar 
    //Stage 4 eskalaerar baserat på mutations-nivå
    private void OnEscelatedMutation(float virusMutationLevel)
    {
        // Ju Högre mutationen, desto bäre "seizure" -lika effekter
        seizingIntensity = 1f + (mutationLevel * 0.5f);

        Debug.Log($"[Stage 4] : Mutation escalated, Siezure intense : {seizingIntensity}");

    }

    //Funktionen nedan TIGGRAS NÄR VIRUST försvinner
    private void OnStage4Removed()
    {
        isStage4Active = false; //Inte längre påslagen 
        siezeTimer = 0f; //nollställer

        Debug.Log("[Stage 4] Virus GONE, Terminal effects have been removed");
    }

    //FUNktionen nedan applicerar "sieuzire" och en extemslowness
    protected override void IndividualPlayerEffect()
    {
        //Säkerhetcheck
        if (!isStage4Active || playerMovement == null)
            return; //avbryter

        siezeTimer += Time.deltaTime; //Ökar 

        //Applicerar effekter var variabelbestämda sekund
        if(siezeTimer >= seizingInterval)
        {
            // Applicerar slowness
            playerMovement.ApplySlowness(extraSlowness, 1f);

            //Applicerar  
            ////////////7
            ///
            ///
         //   Placeholder

           /////////
           ///
           

        }

    }
}
