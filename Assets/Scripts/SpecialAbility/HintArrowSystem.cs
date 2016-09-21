using UnityEngine;


public class HintArrowSystem : MonoBehaviour {

    #region variables
    [SerializeField]
    private float hintOffset = 1.5f;

    private PolygonCoreLogic polygonCoreLogic;
    private PolygonProperties polygonProperties;


    private float hintTweenTime = 1.0f;
    private float hintStartHeight = 10.0f;
    private float hintEndHeight = 0.0f;

    private Vector3 hintScale = new Vector3(0.56f, 0.56f, 0.56f);

    private Vector3 hintHeight = new Vector3(0, 0, 0);


    private GameObject[] hints;
    private Vector3[] hintDirections;
    private bool updateHintArrows = false;

    private LeanTweenType leanTweenType = LeanTweenType.easeOutBounce;

    private static int MAX_PLAYERS = 4;
    #endregion

    #region properties

    public PolygonCoreLogic PolygonCoreLogic
    {
        set { polygonCoreLogic = value;
            Initialize();
        }
    }

    public PolygonProperties PolygonProperties
    {
        set { polygonProperties = value; }
    }
    #endregion

    #region methods

    #region initialization
    void Initialize () {
        if (polygonCoreLogic != null)
        {
            if (polygonProperties != null)
            {
                PrepareHintArrows();
            }
            else
            {
                Debug.LogError("polygonproperties is null!");
            }
        }else
        {
            Debug.LogError("polygonCoreLogic is null!");
        }
	}

    private void PrepareHintArrows()
    {
        hints = new GameObject[MAX_PLAYERS];
        hintDirections = new Vector3[MAX_PLAYERS];
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            hints[i] = Instantiate(polygonProperties.hintArrow) as GameObject;
            hints[i].SetActive(false);
            hints[i].transform.SetParent(transform);
            hintDirections[i] = new Vector3();
        }
    }
    #endregion

    #region updateCycle
    void Update () {
        if (updateHintArrows)
        {
            UpdateHintArrows();
        }
	}

    private void UpdateHintArrows()
    {
        for (int i = 0; i < polygonCoreLogic.PlayerGameObjects.Length; i++)
        {
            if (hints[i] != null && hints[i].activeInHierarchy)
            {
                hintDirections[i] = Vector3.Normalize(polygonCoreLogic.PlayerGameObjects[i].transform.position + polygonProperties.heightOffset - polygonCoreLogic.MiddlePoint) * hintOffset + polygonCoreLogic.PlayerGameObjects[i].transform.position+polygonProperties.heightOffset;
                hints[i].transform.position = hintDirections[i]+hintHeight;
                hints[i].transform.localScale = hintScale;
                hints[i].transform.LookAt(polygonCoreLogic.MiddlePoint);
            }
        }

        for(int i = MAX_PLAYERS-1; i > polygonCoreLogic.PlayerGameObjects.Length-1; i--)
        {
            if(hints[i] != null && hints[i].activeInHierarchy)
            {
                hints[i].SetActive(false);
            }
        }
    }
    #endregion

    #region showArrows
    public void ShowHintArrows()
    {
        for (int i = 0; i < polygonCoreLogic.PlayerGameObjects.Length; i++)
        {
            if (!hints[i].activeInHierarchy)
            {
                hintHeight.y = hintStartHeight;
                hints[i].SetActive(true);
            }
        }
        HintStartTween();
    }
    #endregion

    #region hideArrows
    public void HideHintArrows()
    {
        HintEndTween();
    }

    private void DisableHints()
    {
        updateHintArrows = false;
        hintScale = Vector3.one;
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (hints[i].activeInHierarchy)
            {
                hints[i].SetActive(false);
            }
        }
    }
    #endregion

    #region tweens
    
    private void HintStartTween()
    {
        updateHintArrows = true;
        LeanTween.value(gameObject, hintStartHeight, hintEndHeight, hintTweenTime)
            .setOnUpdate((float height) => { hintHeight.y = height; })
            .setEase(leanTweenType);
    }


    private void HintEndTween()
    {
        LeanTween.value(gameObject, Vector3.one, Vector3.zero, hintTweenTime)
            .setOnUpdate((Vector3 scale) => { hintScale = scale; })
            .setEase(LeanTweenType.easeInElastic)
            .setOnComplete(()=> { DisableHints(); });
    }


    #endregion

    #endregion

}
