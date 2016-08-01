using UnityEngine;
using System.Collections;

public class NavigationInformation : MonoBehaviour {

    [SerializeField]
    private int selectionID;

    public int SelectionID
    {
        get
        {
            return selectionID;
        }
    }
}
