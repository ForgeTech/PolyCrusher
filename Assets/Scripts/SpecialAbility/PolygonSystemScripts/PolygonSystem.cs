using UnityEngine;

public class PolygonSystem : MonoBehaviour
{
    #region variables
    //contains all the animations and graphical properties
    private PolygonTweens polygonTweens;

    //handles sound playback and other stuff
    private PolygonUtil polygonUtil;

    //all the feature specific parts concerning the polygon 
    private PolygonCoreLogic polygonCoreLogic;

    //enemy detection and destruction
    private PolygonEnemyDetection polygonEnemyDetection;

    //hint arrow system
    private HintArrowSystem hintArrowSystem;

    //mesh updater and modifier
    private PolygonMeshBuilder polygonMeshBuilder;

    //killed enemies displayer
    private KilledEnemyDisplayer killedEnemyDisplayer;



    //properties for setting up the polygon system and its components
    [SerializeField]
    private PolygonProperties polygonProperties;
    #endregion

   
    #region methods
    // Use this for initialization
    void Start()
    {
        polygonTweens = gameObject.AddComponent<PolygonTweens>();
        polygonTweens.PolygonSystem = this;
        polygonTweens.PolygonProperties = polygonProperties;

        polygonUtil = gameObject.AddComponent<PolygonUtil>();

        polygonMeshBuilder = gameObject.AddComponent<PolygonMeshBuilder>();
        polygonMeshBuilder.PolygonProperties = polygonProperties;

        polygonCoreLogic = gameObject.AddComponent<PolygonCoreLogic>();
        polygonCoreLogic.PolygonSystem = this;
        polygonCoreLogic.PolygonUtil = polygonUtil;
        polygonCoreLogic.PolygonMeshBuilder = polygonMeshBuilder;
        polygonCoreLogic.PolygonProperties = polygonProperties;
        PolygonCoreLogic.PolyStarted += OnPolyStarted;
        PolygonCoreLogic.PolyEnded += OnPolyEnded;
        PolygonCoreLogic.PolyExecuted += OnPolyExecuted;

        polygonEnemyDetection = gameObject.AddComponent<PolygonEnemyDetection>();
        polygonEnemyDetection.PolygonCoreLogic = polygonCoreLogic;
        polygonEnemyDetection.PolygonProperties = polygonProperties;

        hintArrowSystem = gameObject.AddComponent<HintArrowSystem>();
        hintArrowSystem.PolygonProperties = polygonProperties;
        hintArrowSystem.PolygonCoreLogic = polygonCoreLogic;

        killedEnemyDisplayer = gameObject.AddComponent<KilledEnemyDisplayer>();
        killedEnemyDisplayer.PolygonProperties = polygonProperties;
    }


    private void OnPolyStarted()
    {
        polygonTweens.InitiatePolyStartAnimation(polygonCoreLogic.PolygonPartHeightOffsets);
        hintArrowSystem.ShowHintArrows();
    }


    private void OnPolyEnded()
    {
        polygonTweens.InitiatePolygonExecutedAnimation(polygonCoreLogic.PolygonPartHeightOffsets);
        hintArrowSystem.HideHintArrows();
    }

    /// <summary>
    /// Event method for the "PolyExecuted" event.
    /// </summary>
    protected void OnPolyExecuted()
    {
      
        hintArrowSystem.HideHintArrows();
    }

    #endregion
}