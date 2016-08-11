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

    // Key: Selection index of character, Value: Selected or not.
    private Dictionary<int, bool> selectionMap;

    public Dictionary<int, bool> SelectionMap
    {
        get { return this.selectionMap; }
    }

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
        selectionMap[index] = true;
    }

    public void DeselectAt(int index)
    {
        selectionMap[index] = false;
    }
}