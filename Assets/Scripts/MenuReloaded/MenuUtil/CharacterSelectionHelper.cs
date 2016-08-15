using System.Collections.Generic;
using UnityEngine;

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

    // Key: Selection index of character, Value: Selected or not.
    private Dictionary<int, bool> selectionMap;

    private int characterSelectedCount = 0;

    #region Delegates & Events
    public delegate void CharacterSelectedHandler(int index);
    public event CharacterSelectedHandler OnCharacterSelected;
    public event CharacterSelectedHandler OnCharacterDeselected;
    #endregion

    #region Properties
    public Dictionary<int, bool> SelectionMap
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
        selectionMap = new Dictionary<int, bool>(characterCount);
        for (int i = 0; i < characterCount; i++)
            selectionMap.Add(i, false);
    }

    public void SelectAt(int index)
    {
        if (selectionMap[index])
            Debug.LogError("Index " + index + " already selected!");
        else
        {
            selectionMap[index] = true;
            characterSelectedCount++;
            OnSelected(index);
        }
    }

    public void DeselectAt(int index)
    {
        if (!selectionMap[index])
            Debug.LogError("Index " + index + " already deselected!");
        else
        {
            selectionMap[index] = false;
            characterSelectedCount--;
            OnDeselected(index);
        }
    }

    #region Event methods
    private void OnSelected(int index)
    {
        if (OnCharacterSelected != null)
            OnCharacterSelected(index);
    }

    private void OnDeselected(int index)
    {
        if (OnCharacterDeselected != null)
            OnCharacterDeselected(index);
    }
    #endregion
}