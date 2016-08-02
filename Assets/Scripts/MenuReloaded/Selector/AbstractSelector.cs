using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The selector is used to handle the menu element selection.
/// It defines the the behaviour of 'scrolling' through the menu items.
/// </summary>
public abstract class AbstractSelector : SelectorInterface
{
    protected int currentSelectionIndex;

    // A map of all menu item components which can be scrolled through.
    protected readonly Dictionary<int, GameObject> components;

    public AbstractSelector(int startIndex, Dictionary<int, GameObject> components) {
        this.currentSelectionIndex = startIndex;
        this.components = components;
    }

    public int Current
    {
        get { return this.currentSelectionIndex; }
        protected set { this.currentSelectionIndex = value; }
    }

    /// <summary>
    /// Checks if the index is valid.
    /// </summary>
    protected bool CheckIndex(int key)
    {
        return components.ContainsKey(key);
    }

    protected GameObject GetElementyByKey(int key)
    {
        GameObject gameobject = null;
        try
        {
            if (CheckIndex(key))
                components.TryGetValue(key, out gameobject);

            return gameobject;
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogException(e);
            throw e;
        }
    }

    public void Next()
    {
        BeforeSelection(GetElementyByKey(Current));
        OnNext();
        AfterSelection(GetElementyByKey(Current));
    }

    protected virtual void BeforeSelection(GameObject elementBefore)
    {
    }

    protected virtual void AfterSelection(GameObject elementAfter)
    {
    }

    public void Previous()
    {
        BeforeSelection(GetElementyByKey(Current));
        OnPrevious();
        AfterSelection(GetElementyByKey(Current));
    }

    #region Abstract Methods
    protected abstract void OnNext();
    protected abstract void OnPrevious();
    #endregion
}