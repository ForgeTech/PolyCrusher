using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script cooperates with the CharacterSelectionManager.
/// Since each player selection is an own Menu, this script manages the already selected characters.
/// In order to work properly there have to be some preparations:
///     - The character count must be set correctly
///     - The selection index of each character in each menu has to be the SAME,
///       or this helper script tracks the indices in a wrong way.
/// </summary>
public class CharacterSelectionHelper : MonoBehaviour
{
    [SerializeField]
    private int characterCount = 6;

    [Header("End Screen elements")]
    [SerializeField]
    private Image overlayBackground;

    [SerializeField]
    private RectTransform infoBar;

    // Key: Selection index of character, Value: Selected or not.
    private Dictionary<int, SelectionData> selectionMap;

    private int characterSelectedCount = 0;
    private MultiplayerManager multiplayerManager;

    #region Delegates & Events
    public delegate void CharacterSelectedHandler(int index, PlayerSlot player);
    public event CharacterSelectedHandler OnCharacterSelected;
    public event CharacterSelectedHandler OnCharacterDeselected;
    #endregion

    #region Properties
    public Dictionary<int, SelectionData> SelectionMap
    {
        get { return this.selectionMap; }
    }

    /// <summary>
    /// The current count of the characters which are marked as 'selected'.
    /// </summary>
    public int CharacterSelectedCount { get { return this.characterSelectedCount; } }
    #endregion

    private void Start ()
    { 
        Initialize();
	}

    private void Initialize()
    {
        selectionMap = new Dictionary<int, SelectionData>(characterCount);
        for (int i = 0; i < characterCount; i++)
            selectionMap.Add(i, new SelectionData());

        FindMultiplayerManager();
    }

    private void FindMultiplayerManager()
    {
        GameObject g = GameObject.FindGameObjectWithTag("MultiplayerManager");
        if (g != null)
        {
            multiplayerManager = g.GetComponent<MultiplayerManager>();
            if (multiplayerManager != null)
            {
                multiplayerManager.FinalSelectionExecuted += HandleFinalSelectionStart;
                multiplayerManager.FinalSelectionStoped += HandleFinalSelectionStop;
            }
            else
                Debug.LogError("No Multiplayer Manager Component found!");
        }
        else
            Debug.LogError("No Multiplayer Manager GameObject found!");
    }

    private void HandleFinalSelectionStart(float tweenTime)
    {
        NavigationInformation navInfo = overlayBackground.gameObject.GetComponent<NavigationInformation>();

        LeanTween.scale(infoBar, Vector3.one, tweenTime).setEase(LeanTweenType.easeOutSine);
        LeanTween.value(navInfo.gameObject, navInfo.NormalColor, navInfo.HighlightedColor, tweenTime)
            .setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((Color val) => {
                overlayBackground.color = val;
            });
    }

    private void HandleFinalSelectionStop(float tweenTime)
    {
        NavigationInformation navInfo = overlayBackground.gameObject.GetComponent<NavigationInformation>();

        LeanTween.scale(infoBar, new Vector3(1f, 0f, 1f), tweenTime).setEase(LeanTweenType.easeOutSine);
        LeanTween.value(navInfo.gameObject, navInfo.HighlightedColor, navInfo.NormalColor, tweenTime)
            .setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((Color val) => {
                overlayBackground.color = val;
            });
    }

    public void SelectAt(int index, PlayerSlot playerSlot)
    {
        if (selectionMap[index].selected)
            Debug.LogError("Index " + index + " already selected!");
        else
        {
            selectionMap[index].selected = true;
            selectionMap[index].selectedBySlot = playerSlot;
            characterSelectedCount++;
            OnSelected(index, playerSlot);
        }
    }

    public void DeselectAt(int index, PlayerSlot playerSlot)
    {
        if (!selectionMap[index].selected)
            Debug.LogError("Index " + index + " already deselected!");
        else
        {
            selectionMap[index].selected = false;
            selectionMap[index].selectedBySlot = PlayerSlot.None;
            characterSelectedCount--;
            OnDeselected(index, playerSlot);
        }
    }

    #region Event methods
    private void OnSelected(int index, PlayerSlot player)
    {
        if (OnCharacterSelected != null)
            OnCharacterSelected(index, player);
    }

    private void OnDeselected(int index, PlayerSlot player)
    {
        if (OnCharacterDeselected != null)
            OnCharacterDeselected(index, player);
    }
    #endregion

    public class SelectionData
    {
        public bool selected = false;
        public PlayerSlot selectedBySlot = PlayerSlot.None;
    }
}