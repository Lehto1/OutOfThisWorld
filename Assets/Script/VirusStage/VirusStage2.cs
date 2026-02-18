using Unity.VisualScripting;
using UnityEngine;

public class VirusStage2 : VirusHandlingScript
{

    //Stage 2 inställningar

    [Header("Stge 2 Active settings")]
    [SerializeField] private float slownessFactor = 0.85f; //15% låmgsammare
    [SerializeField] private float virusSlownessDuration = 3f; //Hur länge Slowness varar
    [SerializeField] private float virussSlownessApplyingInterval = 2f; //Applicerar var 2:a sekund

    //rUnntime variabler
    private Movement playerMovement;
    private float slownessTimer = 0f;
    private bool isStage2Active = false; //Är virus i fas 2?

    //Finner rörelsekoden först

    protected override void Awake()
    {
        //iNITIALISERAR VIRUSbasklassen
        //anropar parent
        base.Awake();

        //Hittar movement-scriptet på GameObject
        playerMovement = GetComponent<Movement>();

        if(playerMovement == null )
        {
            Debug.LogWarning("Stage2 not functional, Movement-script not found");
        }
    }

    //Regristrerar events 
    protected override void OnEnable()
    {
        base.OnEnable();

        //Regristrerar alla listeners
        //Barnkalssen lyssnar på vad som sker viruset
        onActivatedVirus += OnStage2Activated; //lyssnare för aktiveing

        OnCriticalVirus += OnStage2Worsened; //Lyssnare för förvärrning

        OnChangedMutation += OnScaledMutation; //Lyssnare för mutationer

        OnRemovedVIrus += OnStage2Removed; //lyssnare för botning/ norttagning

        Debug.Log("Stage 2 Registered it's listenrers");
    }

    protected override void OnDisable()
    {
        //Avregisterar eventlisteners
        onActivatedVirus -= OnStage2Activated; //lyssnare för aktiveing

        OnCriticalVirus -= OnStage2Worsened; //Lyssnare för förvärrning

        OnChangedMutation -= OnScaledMutation; //Lyssnare för mutationer

        OnRemovedVIrus -= OnStage2Removed; //lyssnare för botning/ norttagning

        //Anropar parent
        base.OnDisable();

        Debug.Log("Stage 2 unegistered it's listenrers");
    }

    //Funktion som reiggras när virust blir aktivt
    private void OnStage2Activated()
    {
        // kollar om koden när i Activce stadiet
        if(GetCurretStage() == VirusStages.Active)
        {
            //Aktiverar fas 2 effekter
            isStage2Active=true;

            Debug.Log("[sTAGE 2]  Virus activated, slowness begins");
        }
    }
    
    //Funktionen nedan triggras när viruset blir krititskt
    private void OnStage2Worsened()
    {
        //Dubbelkollar att stadiet stämmer 
        if(GetCurretStage() == VirusStages.Critical)
        {
            //Ökar slowneess-effekten
            slownessFactor = 0.75f;

            Debug.Log("[Stage 2] Critical Worsening -- increased slownewss");
        }
    }

    //Funtion som triggras vid virus.mutation
    private void OnScaledMutation(float mutationLevel)
    {
        //Ju högre desto mer slowness
    //    float virusScaledSloewness = slownessFactor. (mutationLevel * 0.05f);

     //   Debug.Log($"Stage 2] Scaled mutation, Slowness {virusScaledSloewness} , Mutation {mutationLevel}");

    }

    //Funktionen triggras när viruset inte längre finns 
    //Rensar effekter
    private void OnStage2Removed()
    {
        //Deaktiverar effekter
        isStage2Active = false;
        slownessTimer = 0f;

    }

    //
    protected override void IndividualPlayerEffect()
    {
        //Kollar om Stage 2 Är aktivkt , om Momvement finns 
        if (!isStage2Active || playerMovement == null)
        {
            return; //Inget sker
        }

        //Ökar timern
        slownessTimer += Time.deltaTime;

        //Applicerar slowness var variabelbestämd sekund
        if (slownessTimer >= virussSlownessApplyingInterval)
        {
            // Applicerar slowess
            //på spelaren
            playerMovement.ApplySlowness(slownessFactor, virusSlownessDuration);

            Debug.Log($"Stage 2-- Slowness APll : {slownessFactor}, Hastighet {virusSlownessDuration}");

            //Nolställer timern
            slownessTimer = 0f;
        }

    }
}
