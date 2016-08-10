using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script for the visual specialities of the game mode selection menu.
/// This script is currently only made for 2 game modes!
/// </summary>
[RequireComponent(typeof(AbstractMenuManager))]
public class GameModeSelectionHelper : MonoBehaviour
{
    #region Inspector values
    [Header("General settings")]
    [SerializeField]
    private float tweenTime = 0.35f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [Header("Scene items")]
    [SerializeField]
    private RectTransform wavesTextContainer;

    [SerializeField]
    private RectTransform yoloTextContainer;

    [SerializeField]
    private Vector2 textPosition;
    #endregion

    #region Internal members
    private AbstractMenuManager menuManager;
    private SelectorInterface selector;
    private ImageData[] gameModes;
    private Vector2 outerBorderPointUp;
    private Vector2 outerBorderPointDown;
    private bool waveText = true;       // Assume that the wave is selected first
    #endregion

    void Start ()
    {
        Initialize();
	}

    private void Initialize()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        menuManager.NavigationNext += HandleSelectionChange;
        menuManager.NavigationPrevious += HandleSelectionChange;

        menuManager.NavigationNext += TextChangedNext;
        menuManager.NavigationPrevious += TextChangedPrevious;

        selector = menuManager.Selector;

        gameModes = new ImageData[menuManager.MenuComponents.Count];
        for (int i = 0; i < gameModes.Length; i++)
        {
            GameObject tmp;
            menuManager.MenuComponents.TryGetValue(i, out tmp);
            gameModes[i] = new ImageData(tmp.GetComponent<RectTransform>());
        }

        outerBorderPointUp = new Vector2(textPosition.x, 1200f);
        outerBorderPointDown = new Vector2(textPosition.x, -outerBorderPointUp.y);
        TweenText(wavesTextContainer, outerBorderPointUp, textPosition);
    }

    private void HandleSelectionChange()
    {
        SetToForeground(gameModes[selector.Current].rect.gameObject);
    }

    private void TextChangedNext()
    {
        if (waveText)
        {
            // Tween wave text down and tween yolo text from top to the text position
            TweenText(wavesTextContainer, textPosition, outerBorderPointDown);
            TweenText(yoloTextContainer, outerBorderPointUp, textPosition);
        }
        else
        {
            // Tween yolo text down and tween wave text from top to the text position
            TweenText(yoloTextContainer, textPosition, outerBorderPointDown);
            TweenText(wavesTextContainer, outerBorderPointUp, textPosition);
        }

        waveText = !waveText;
    }


    private void TextChangedPrevious()
    {
        if (waveText)
        {
            // Tween wave text up and tween the yolo text from bottom to text position
            TweenText(wavesTextContainer, textPosition, outerBorderPointUp);
            TweenText(yoloTextContainer, outerBorderPointDown, textPosition);
        }
        else
        {
            // Tween yolo text up and tween the wave text from bottom to text position
            TweenText(yoloTextContainer, textPosition, outerBorderPointUp);
            TweenText(wavesTextContainer, outerBorderPointDown, textPosition);
        }

        waveText = !waveText;
    }

    private void TweenText(RectTransform rect, Vector2 from, Vector2 to)
    {
        LeanTween.cancel(rect.gameObject);
        LeanTween.value(rect.gameObject, from, to, tweenTime)
            .setEase(easeType)
            .setOnUpdate((Vector2 val) => {
                rect.anchoredPosition = val;
            });
    }

    private void SetToForeground(GameObject shouldBeInForeground)
    {
        shouldBeInForeground.transform.SetAsLastSibling();
    }

    private int CalculateIndex(int index)
    {
        return CalculatePositiveMod(index, gameModes.Length);
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
}