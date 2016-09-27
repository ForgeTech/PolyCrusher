using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AbstractMenuManager))]
public class CrushGuideSelectionHelper : MonoBehaviour
{
    #region Inspector variables
    [Header("Settings")]
    [SerializeField]
    private float gapSize = 660f;

    [SerializeField]
    private Vector3 middlePoint = Vector3.zero;

    [Header("UI Elements")]
    [SerializeField]
    private Image leftArrow;

    [SerializeField]
    private Image rightArrow;

    [SerializeField]
    private Color pressedColor;

    [Header("Tweening options")]
    [SerializeField]
    private float tweenTime = 0.2f;

    [SerializeField]
    private float scaleTweenMultiplier = 2f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutExpo;

    [SerializeField]
    private LeanTweenType transitionEaseType = LeanTweenType.easeOutExpo;
    #endregion

    #region Internal members
    private AbstractMenuManager menuManager;
    private SelectorInterface selector;
    private RectTransform[] tutorialIslands;
    private Color originalArrowColor;
    #endregion

    private Vector2 tweenOutPosition = new Vector2(0f, 5000f);

	private void Start ()
    {
        Initialize();
	}

    private void Initialize()
    {
        // Initialize the menu manager and the selector
        menuManager = GetComponent<AbstractMenuManager>();
        selector = menuManager.Selector;
        menuManager.NavigationNext += HandleNextSelection;
        menuManager.NavigationPrevious += HandlePreviousSelection;

        tutorialIslands = new RectTransform[menuManager.MenuComponents.Count];

        // Fill the internal level array
        for (int i = 0; i < tutorialIslands.Length; i++)
        {
            GameObject tmp;
            menuManager.MenuComponents.TryGetValue(i, out tmp);
            tutorialIslands[i] = tmp.GetComponent<RectTransform>();
        }

        // Other initialization steps
        originalArrowColor = leftArrow.color;
        RepositionElements();
    }

    private void RepositionElements()
    {
        Vector3 offset = new Vector3(gapSize, 0f, 0f);

        for (int i = 0; i < tutorialIslands.Length; i++)
        {
            int index;

            if (i == (index = CalculateIndex(selector.Current - 2)))
                DoReposition(index, middlePoint - offset * 2f);
            else if (i == (index = CalculateIndex(selector.Current - 1)))
                DoReposition(index, middlePoint - offset);
            else if (i == selector.Current)
                DoReposition(selector.Current, middlePoint);
            else if (i == (index = CalculateIndex(selector.Current + 1)))
                DoReposition(index, middlePoint + offset);
            else if (i == (index = CalculateIndex(selector.Current + 2)))
                DoReposition(index, middlePoint + offset * 2f);
            else
                tutorialIslands[i].anchoredPosition = tweenOutPosition;
        }
    }

    private void DoReposition(int index, Vector2 offset)
    {
        RectTransform island = tutorialIslands[index];
        TweenLevelIsland(island, island.anchoredPosition, offset);
    }

    private void TweenLevelIsland(RectTransform island, Vector2 from, Vector2 to)
    {
        LeanTween.value(island.gameObject, from, to, tweenTime)
            .setEase(transitionEaseType)
            .setOnUpdate((Vector2 val) => {
                island.anchoredPosition = val;
            });
    }

    private int CalculateIndex(int index)
    {
        return CalculatePositiveMod(index, tutorialIslands.Length);
    }

    private int CalculatePositiveMod(int a, int n)
    {
        return ((a % n) + n) % n;
    }

    private Text GetTextFrom(int index)
    {
        Text description = tutorialIslands[index].GetChild(0).GetChild(0).GetComponent<Text>();
        return description;
    }

    private void TweenDescriptionText(Text current, Text last)
    {
        LeanTween.alpha(current.rectTransform, 1f, tweenTime).setEase(easeType);
        LeanTween.alpha(last.rectTransform, 0f, tweenTime).setEase(easeType);
    }

    /// <summary>
    /// Callback which is registered at the AbstractMenuManager.
    /// </summary>
    private void HandleNextSelection()
    {
        NavigationInformation info = tutorialIslands[CalculateIndex(selector.Current - 1)].GetComponent<NavigationInformation>();
        DoTextShadowTween(GetLevelIslandText(tutorialIslands[CalculateIndex(selector.Current - 1)].gameObject), info.ShadowAlphaSelected, 0f);

        TweenArrow(rightArrow);
        RepositionElements();

        Text currentDescription = GetTextFrom(selector.Current);
        Text lastDescription = GetTextFrom(CalculateIndex(selector.Current - 1));
        TweenDescriptionText(currentDescription, lastDescription);

        info = tutorialIslands[selector.Current].GetComponent<NavigationInformation>();
        DoTextShadowTween(GetLevelIslandText(tutorialIslands[selector.Current].gameObject), 0f, info.ShadowAlphaSelected);
    }

    /// <summary>
    /// Callback which is registered at the AbstractMenuManager.
    /// </summary>
    private void HandlePreviousSelection()
    {
        NavigationInformation info = tutorialIslands[CalculateIndex(selector.Current + 1)].GetComponent<NavigationInformation>();
        DoTextShadowTween(GetLevelIslandText(tutorialIslands[CalculateIndex(selector.Current + 1)].gameObject), info.ShadowAlphaSelected, 0f);

        TweenArrow(leftArrow);
        RepositionElements();

        Text currentDescription = GetTextFrom(selector.Current);
        Text lastDescription = GetTextFrom(CalculateIndex(selector.Current + 1));
        TweenDescriptionText(currentDescription, lastDescription);

        info = tutorialIslands[selector.Current].GetComponent<NavigationInformation>();
        DoTextShadowTween(GetLevelIslandText(tutorialIslands[selector.Current].gameObject), 0f, info.ShadowAlphaSelected);
    }

    private void DoTextShadowTween(Text text, float startAlpha, float endAlpha)
    {
        Material mat = GetMaterialFrom(text.gameObject);
        Color shadowColor = mat.GetColor("_ShadowColor");

        LeanTween.value(text.gameObject, startAlpha, endAlpha, tweenTime).setEase(easeType)
            .setOnUpdate((float val) => {
                shadowColor.a = val / 255f;
                mat.SetColor("_ShadowColor", shadowColor);
            });
    }

    private Text GetLevelIslandText(GameObject levelIsland)
    {
        Text text = null;
        foreach (Transform child in levelIsland.transform)
        {
            if (child.tag == "Terrain")
                text = child.GetComponent<Text>();
        }
        return text;
    }

    private Material GetMaterialFrom(GameObject g)
    {
        Material mat = null;
        Text txt = g.GetComponent<Text>();

        if (txt != null)
            mat = txt.material;

        return mat;
    }

    private void TweenArrow(Image arrow)
    {
        if (leftArrow != null)
        {
            #region Color Tween
            LeanTween.value(arrow.gameObject, originalArrowColor, pressedColor, tweenTime * 0.5f)
                .setEase(easeType)
                .setOnUpdate((Color val) => {
                    arrow.color = val;
                })
                .setOnComplete(
                () => {
                    Color c = new Color(arrow.color.r, arrow.color.g, arrow.color.b);
                    LeanTween.value(arrow.gameObject, c, originalArrowColor, tweenTime * 0.5f)
                    .setEase(easeType)
                    .setOnUpdate((Color val) => {
                        arrow.color = val;
                    });
                });
            #endregion

            #region Position Tween
            LeanTween.scale(arrow.rectTransform, Vector3.one * scaleTweenMultiplier, tweenTime * 0.5f)
                .setEase(easeType)
                .setOnComplete(() => {
                    LeanTween.scale(arrow.rectTransform, Vector3.one, tweenTime * 0.5f).setEase(easeType);
                });
            #endregion
        }
        else
            Debug.LogError("Left arrow image not assigned!");
    }
}