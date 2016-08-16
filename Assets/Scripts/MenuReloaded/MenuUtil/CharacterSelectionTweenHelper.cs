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

    #region Internal Members
    private AbstractMenuManager menuManager;
    private MultiplayerManager multiplayerManager;
    private ImageData[] characters;
    private Vector2 destinationPosition;
    #endregion

    // Use this for initialization
    void Start ()
    {
        Initialize();
	}

    private void Initialize()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        menuManager.NavigationNext += HandleNextChange;
        menuManager.NavigationPrevious += HandlePreviousChange;

        multiplayerManager = FindObjectOfType<MultiplayerManager>();
        multiplayerManager.FinalSelectionExecuted += HandleFinalSelectionStart;
        multiplayerManager.FinalSelectionStoped += HandleFinalSelectionStop;

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
    }

    private void HandleCharacterSelected(int index)
    {
        Text selectedText = FindSelectedText(index);
        LeanTween.rotateZ(selectedText.gameObject, 15f, textTweenTime).setEase(easeType);
        LeanTween.scale(selectedText.rectTransform, Vector3.one, textTweenTime).setEase(easeType);
    }

    private void HandleCharacterDeselected(int index)
    {
        Text selectedText = FindSelectedText(index);
        LeanTween.rotateZ(selectedText.gameObject, 0f, textTweenTime).setEase(easeType);
        LeanTween.scale(selectedText.rectTransform, Vector3.zero, textTweenTime).setEase(easeType);
    }

    private Text FindSelectedText(int index)
    {
        GameObject g;
        menuManager.MenuComponents.TryGetValue(index, out g);

        Text selectedText = null;
        foreach (Transform child in g.transform)
        {
            if (child.tag == "Pie")
                selectedText = child.GetComponent<Text>();
        }

        if (selectedText == null)
            Debug.LogError("'Selected' Text not found!");

        return selectedText;
    }

    private void HandleNextChange()
    {
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
        ImageData oldCurrent = characters[CalculateIndex(menuManager.Selector.Current + 1)];
        TweenElement(oldCurrent.rect,
            oldCurrent.originalPosition,
            oldCurrent.originalPosition - new Vector2(sideGap, 0f));

        ImageData newCurrent = characters[menuManager.Selector.Current];
        TweenElement(newCurrent.rect,
            newCurrent.originalPosition + new Vector2(sideGap, 0f),
            newCurrent.originalPosition);
    }

    private void HandleFinalSelectionStart()
    {
        //TODO: final selection screen in animation
        Debug.Log("final selection started");
    }


    private void HandleFinalSelectionStop()
    {
        //TODO: final selection screen out animation
        Debug.Log("final selection stopped");

    }




    private void TweenElement(RectTransform rect, Vector2 from, Vector2 to)
    {
        LeanTween.value(rect.gameObject, from, to, tweenTime)
            .setEase(easeType)
            .setOnUpdate((Vector2 val) => {
                rect.anchoredPosition = val;
            });
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
}
