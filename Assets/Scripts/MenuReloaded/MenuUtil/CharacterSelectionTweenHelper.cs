using System;
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

    [Header("Arrows")]
    [SerializeField]
    private Image leftArrow;

    [SerializeField]
    private Image rightArrow;

    [SerializeField]
    private float arrowOffset = 35f;

    [Header("Sounds")]
    [SerializeField]
    private AudioClip registerSound;

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
        joinInfoBox = FindChildObjectWithTag(dynamicPlayerInfos, "Enemy").GetComponent<RectTransform>();
        selectInfoBox = FindChildObjectWithTag(dynamicPlayerInfos, "Bullet").GetComponent<RectTransform>();
        readyInfoBox = FindChildObjectWithTag(dynamicPlayerInfos, "Boss").GetComponent<RectTransform>();
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
        SoundManager.SoundManagerInstance.Play(registerSound, SoundManager.SoundManagerInstance.transform, 1f, 1f, AudioGroup.MenuSounds);

        // Position fade for info boxes
        LeanTween.moveY(joinInfoBox, topAnchoredPosition.y, tweenTime).setEase(easeType);
        LeanTween.moveY(selectInfoBox, bottomAchoredPosition.y, tweenTime).setEase(easeType);

        // Question mark scale
        LeanTween.scale(questionMark.rectTransform, Vector3.zero, tweenTime).setEase(easeType);

        // Background circle color
        NavigationInformation info = GetInfoOfCurrentlySelected();
        characterBackgroundCircle.color = info.PressedColor;

        // Arrow initial placement
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);
        TweenArrowAlpha(1f);
    }

    private void TweenArrowAlpha(float destinationAlpha)
    {
        LeanTween.alpha(leftArrow.rectTransform, destinationAlpha, tweenTime).setEase(easeType);
        LeanTween.alpha(rightArrow.rectTransform, destinationAlpha, tweenTime).setEase(easeType);
    }

    private void HandleCharacterSelected(int index, PlayerSlot slot)
    {
        // Info box tweens
        if (slot == menuManager.PlayerSlot)
        {
            LeanTween.moveY(selectInfoBox, topAnchoredPosition.y, tweenTime).setEase(easeType);
            LeanTween.scale(readyInfoBox, Vector3.one, tweenTime).setEase(easeType);
            TweenArrowAlpha(0f);
            
            // Selected overlay text
            Text selectedText = FindPlayerSubObjectTextByTag(index, "Pie");
            LeanTween.rotateZ(selectedText.gameObject, 15f, textTweenTime).setEase(easeType);
            LeanTween.scale(selectedText.rectTransform, Vector3.one, textTweenTime).setEase(easeType);
        }
        SetImageGrayscale(1f, index);
    }

    private void HandleCharacterDeselected(int index, PlayerSlot slot)
    {
        // Info box tweens
        if (slot == menuManager.PlayerSlot)
        {
            LeanTween.moveY(selectInfoBox, bottomAchoredPosition.y, tweenTime).setEase(easeType);
            LeanTween.scale(readyInfoBox, Vector3.zero, tweenTime).setEase(easeType);

            TweenArrowAlpha(1f);

            // Selected overlay text
            Text selectedText = FindPlayerSubObjectTextByTag(index, "Pie");
            LeanTween.rotateZ(selectedText.gameObject, 0f, textTweenTime).setEase(easeType);
            LeanTween.scale(selectedText.rectTransform, Vector3.zero, textTweenTime).setEase(easeType);
        }
        SetImageGrayscale(0f, index);
    }

    private Text FindPlayerSubObjectTextByTag(int index, string tag)
    {
        GameObject g;
        menuManager.MenuComponents.TryGetValue(index, out g);

        Text selectedText = FindChildObjectWithTag(g, tag).GetComponent<Text>();
        if (selectedText == null)
            Debug.LogError("Text with tag '" + tag + "' was not found!");

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

        // Right arrow size tween
        NavigationInformation arrowInfo = rightArrow.GetComponent<NavigationInformation>();
        LeanTween.scale(rightArrow.rectTransform, arrowInfo.PressedScale, tweenTime * 0.5f).setEase(easeType)
            .setOnComplete(() => {
                LeanTween.scale(rightArrow.rectTransform, arrowInfo.OriginalScale, tweenTime * 0.5f).setEase(easeType);
            });
        LeanTween.color(rightArrow.rectTransform, arrowInfo.PressedColor, tweenTime * 0.5f).setEase(easeType)
            .setOnComplete(() => {
                LeanTween.color(rightArrow.rectTransform, arrowInfo.NormalColor, tweenTime * 0.5f).setEase(easeType);
            });

        // Character tweens
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

        // Left arrow size & color tween
        NavigationInformation arrowInfo = leftArrow.GetComponent<NavigationInformation>();
        LeanTween.scale(leftArrow.rectTransform, arrowInfo.PressedScale, tweenTime).setEase(easeType)
            .setOnComplete(() => {
                LeanTween.scale(leftArrow.rectTransform, arrowInfo.OriginalScale, tweenTime * 0.5f).setEase(easeType);
            }); ;
        LeanTween.color(leftArrow.rectTransform, arrowInfo.PressedColor, tweenTime * 0.5f).setEase(easeType)
            .setOnComplete(() => {
                LeanTween.color(leftArrow.rectTransform, arrowInfo.NormalColor, tweenTime * 0.5f).setEase(easeType);
            });

        // Character tweens
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

    private void SetImageGrayscale(float destinationValue, int index)
    {
        Image selectedComponent = menuManager.MenuComponents[index].GetComponent<Image>();
        selectedComponent.material = new Material(selectedComponent.material);      // Hack :/
        selectedComponent.material.SetFloat("_EffectAmount", destinationValue);
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

    private GameObject FindChildObjectWithTag(GameObject g, string tag)
    {
        foreach (Transform child in g.transform)
        {
            if (child.tag == tag)
                return child.gameObject;
        }
        return null;
    }
}
