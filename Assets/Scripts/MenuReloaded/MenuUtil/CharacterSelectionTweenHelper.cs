using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AbstractMenuManager))]
public class CharacterSelectionTweenHelper : MonoBehaviour
{
    [Header("General settings")]
    [SerializeField]
    private float tweenTime = 0.35f;

    [SerializeField]
    private float textTweenTime = 0.25f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [SerializeField]
    private float sideGap = 60f;

    [SerializeField]
    private CharacterSelectionHelper selectionHelper;

    [Header("Tweening input")]
    [SerializeField]
    private GameObject staticJoinElements;

    [SerializeField]
    private GameObject dynamicPlayerInfos;

    [Header("Dynamic Player info tween settings")]
    [SerializeField]
    private Vector2 topAnchoredPosition = new Vector2(0, 540);

    [SerializeField]
    private Vector2 bottomAchoredPosition = new Vector2(0, 267);

    #region Internal Members
    private CharacterMenuManager menuManager;
    private ImageData[] characters;
    private Vector2 destinationPosition;

    // Static player infos
    private Text questionMark;
    private Image characterBackgroundCircle;

    // Dynamic player infos
    private RectTransform joinInfoBox;
    private RectTransform selectInfoBox;
    private RectTransform readyInfoBox;
    #endregion

    // Use this for initialization
    void Start ()
    {
        InitializeDynamicPlayerInfoElements();
        InitializeStaticPlayerInfoElements();
        Initialize();
	}

    private void InitializeDynamicPlayerInfoElements()
    {
        // These elements use already set tags -> Don't want to create new tags only for this
        joinInfoBox = FindGameObjectWithTag(dynamicPlayerInfos, "Enemy").GetComponent<RectTransform>();
        selectInfoBox = FindGameObjectWithTag(dynamicPlayerInfos, "Bullet").GetComponent<RectTransform>();
        readyInfoBox = FindGameObjectWithTag(dynamicPlayerInfos, "Boss").GetComponent<RectTransform>();
    }

    private void InitializeStaticPlayerInfoElements()
    {
        questionMark = staticJoinElements.transform.GetComponentInChildren<Text>();
        characterBackgroundCircle = staticJoinElements.GetComponent<Image>();
    }

    private void Initialize()
    {
        menuManager = (CharacterMenuManager) GetComponent<AbstractMenuManager>();
        menuManager.NavigationNext += HandleNextChange;
        menuManager.NavigationPrevious += HandlePreviousChange;

        characters = new ImageData[menuManager.MenuComponents.Count];
        for (int i = 0; i < characters.Length; i++)
        {
            GameObject tmp;
            menuManager.MenuComponents.TryGetValue(i, out tmp);
            characters[i] = new ImageData(tmp.GetComponent<RectTransform>());
        }

        if (selectionHelper == null)
            Debug.LogError("Selection helper is not assigned!");

        selectionHelper.OnCharacterSelected += HandleCharacterSelected;
        selectionHelper.OnCharacterDeselected += HandleCharacterDeselected;
        menuManager.PlayerRegistered += HandlePlayerRegistered;
    }

    private void HandlePlayerRegistered()
    {
        // Position fade for info boxes
        LeanTween.moveY(joinInfoBox, topAnchoredPosition.y, tweenTime).setEase(easeType);
        LeanTween.moveY(selectInfoBox, bottomAchoredPosition.y, tweenTime).setEase(easeType);

        LeanTween.scale(questionMark.rectTransform, Vector3.zero, tweenTime).setEase(easeType);

        NavigationInformation info = GetInfoOfCurrentlySelected();
        characterBackgroundCircle.color = info.PressedColor;
    }

    private void HandleCharacterSelected(int index, PlayerSlot slot)
    {
        if (slot == menuManager.PlayerSlot)
        {
            LeanTween.moveY(selectInfoBox, topAnchoredPosition.y, tweenTime).setEase(easeType);
            LeanTween.scale(readyInfoBox, Vector3.one, tweenTime).setEase(easeType);
        }

        // Selected overlay text
        Text selectedText = FindSelectedText(index);
        LeanTween.rotateZ(selectedText.gameObject, 15f, textTweenTime).setEase(easeType);
        LeanTween.scale(selectedText.rectTransform, Vector3.one, textTweenTime).setEase(easeType);
    }

    private void HandleCharacterDeselected(int index, PlayerSlot slot)
    {
        if (slot == menuManager.PlayerSlot)
        {
            LeanTween.moveY(selectInfoBox, bottomAchoredPosition.y, tweenTime).setEase(easeType);
            LeanTween.scale(readyInfoBox, Vector3.zero, tweenTime).setEase(easeType);
        }

        // Selected overlay text
        Text selectedText = FindSelectedText(index);
        LeanTween.rotateZ(selectedText.gameObject, 0f, textTweenTime).setEase(easeType);
        LeanTween.scale(selectedText.rectTransform, Vector3.zero, textTweenTime).setEase(easeType);
    }

    private Text FindSelectedText(int index)
    {
        GameObject g;
        menuManager.MenuComponents.TryGetValue(index, out g);

        Text selectedText = FindGameObjectWithTag(g, "Pie").GetComponent<Text>();
        if (selectedText == null)
            Debug.LogError("'Selected' Text not found!");

        return selectedText;
    }

    private void HandleNextChange()
    {
        // Background circle
        NavigationInformation info = GetInfoOfCurrentlySelected();
        LeanTween.value(characterBackgroundCircle.gameObject, characterBackgroundCircle.color, info.PressedColor, tweenTime)
            .setEase(easeType)
            .setOnUpdate((Color val) => {
                characterBackgroundCircle.color = val;
            });

        ImageData oldCurrent = characters[CalculateIndex(menuManager.Selector.Current - 1)];
        TweenElement(oldCurrent.rect,
            oldCurrent.originalPosition,
            oldCurrent.originalPosition + new Vector2(sideGap, 0f));

        ImageData newCurrent = characters[menuManager.Selector.Current];
        TweenElement(newCurrent.rect,
            newCurrent.originalPosition - new Vector2(sideGap, 0f),
            newCurrent.originalPosition);
    }

    private void HandlePreviousChange()
    {
        NavigationInformation info = GetInfoOfCurrentlySelected();
        LeanTween.value(characterBackgroundCircle.gameObject, characterBackgroundCircle.color, info.PressedColor, tweenTime)
            .setEase(easeType)
            .setOnUpdate((Color val) => {
                characterBackgroundCircle.color = val;
            });

        ImageData oldCurrent = characters[CalculateIndex(menuManager.Selector.Current + 1)];
        TweenElement(oldCurrent.rect,
            oldCurrent.originalPosition,
            oldCurrent.originalPosition - new Vector2(sideGap, 0f));

        ImageData newCurrent = characters[menuManager.Selector.Current];
        TweenElement(newCurrent.rect,
            newCurrent.originalPosition + new Vector2(sideGap, 0f),
            newCurrent.originalPosition);
    }

    private void TweenElement(RectTransform rect, Vector2 from, Vector2 to)
    {
        LeanTween.value(rect.gameObject, from, to, tweenTime)
            .setEase(easeType)
            .setOnUpdate((Vector2 val) => {
                rect.anchoredPosition = val;
            });
    }

    private NavigationInformation GetInfoOfCurrentlySelected()
    {
        return menuManager.MenuComponents[menuManager.Selector.Current].gameObject.GetComponent<NavigationInformation>();
    }

    private int CalculateIndex(int index)
    {
        return CalculatePositiveMod(index, characters.Length);
    }

    private int CalculatePositiveMod(int a, int n)
    {
        return ((a % n) + n) % n;
    }

    /// <summary>
    /// Helper class for tweening whichs save some original values of the image elements.
    /// </summary>
    private class ImageData
    {
        public RectTransform rect;
        public Vector2 originalSize;
        public Vector2 originalPosition;

        public ImageData(RectTransform rect)
        {
            this.rect = rect;
            this.originalPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
            this.originalSize = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);
        }
    }

    private GameObject FindGameObjectWithTag(GameObject g, string tag)
    {
        foreach (Transform child in g.transform)
        {
            if (child.tag == tag)
                return child.gameObject;
        }
        return null;
    }
}
