using UnityEngine;

public class VirusStage3 : VirusHandlingScript
{
    //WSettings
    [Header("Stage 3 cRITICAL SETTINGS")]
    [SerializeField] private float bluringOfVision = 0.3f; //Blur kraftigheten

    [SerializeField] private float virusScreenShake = 0.5f; //skakningens intensitet

    [SerializeField] private float effektApplicationInterval = 1.5f; //Apllicerar var 1.5 sekund

    //Runtime
    private float effectTimer = 0f;
    private bool isStage3Active = false; //Boolean flag som visar om koden är i stage 3 eller inte

    //Initialisering
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Stage 3 Initialized");

    }

    //Registra events
    protected override void OnEnable()
    {
        base.OnEnable();

        OnCriticalVirus += OnStage3Activated; //lyssnare för aktiveing

        OnTerminalVirus += OnStage3Escalated; //Lyssnare för förvärrning

        OnChangedMutation += OnAdjustedResistance3; //Lyssnare för restitstansörndringar

        OnRemovedVIrus += OnStage3Removed; //lyssnare för botning/ norttagning

        Debug.Log("Stage 3 Registered it's listenrers");
    }
    protected override void OnDisable()
    {
        //Avregistrearar alla event lsiteners

        //Avregisterar eventlisteners
        onActivatedVirus -= OnStage3Activated; //lyssnare för aktiveing

        OnCriticalVirus -= OnStage3Escalated; //Lyssnare för förvärrning

        OnChangedMutation -= OnAdjustedResistance3; //Lyssnare för resistans förändringar

        OnRemovedVIrus -= OnStage3Removed; //lyssnare för botning/ norttagning

        //Anropar parent
        base.OnDisable();

        Debug.Log("Stage 2 unegistered it's listenrers");
        base.OnDisable();
    }

    //Denna funktion triggras när viruset blir kritiskt
    private void OnStage3Activated()
    {
        if(GetCurretStage() == VirusStages.Critical)
        {
            isStage3Active |= true; //flag till true
            Debug.Log("Stage 3 Virus critical, ");
        }
    }

    // En funktion spm tirggrras bär viruset blir terminalt 
    private void OnStage3Escalated()
    {
        if ((GetCurretStage() == VirusStages.Terminal))
        {
            //Ökar stadiets blur-styrka till 
            bluringOfVision = 0.6f;

            // ökar skakningen till 100%
            virusScreenShake = 1f;

            Debug.Log("Stage 3, TERMINA TERMINAL TERMINAL");
            
        }
    }

    //dENNA funktion triggras  nät spelarens resisans ökar, Stage 3 justerar effect styrkan baserat på resistansen
    private void OnAdjustedResistance3(float resistance)
    {
        // ju högre resistance,  desto mindre blur.effekt
        float adjustedBlur = bluringOfVision * (1f - resistance);

        Debug.Log($"Stage 3 resistance adjusted. Blur scaled to {adjustedBlur}, Resistance {resistance}");

    }

    //Denna funktion triggras när birust är borta 
    //Funktionen reducerar och rensar stadie 3 s effekter
    private void OnStage3Removed()
    {
        isStage3Active = false;

        effectTimer = 0f;

        Debug.Log("Stage 3, the virus has dissapeared");
    }

    //Applicera kameraeffekter
    //Applicerar blur och shake
    protected override void IndividualPlayerEffect()
    {
        if (!isStage3Active)
        {
            return;

        }

        //ökar effekt-timern
        effectTimer += Time.deltaTime;

        //Applicerar kamera-effekter var variablebestämd sekund
        if (effectTimer >= effektApplicationInterval)
        {
            //Applicera blur och screen shake 
            ///
            ///
            ////
            //
            //

            effectTimer = 0f; //Nollställer

        }
    }
}
