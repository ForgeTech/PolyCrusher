using UnityEngine;
using InControl;
using System.Collections.Generic;


public class PlayerSelectionContainer : MonoBehaviour {

    public InputDevice[] playerInputDevices = new InputDevice[4];
    public bool[] playerActive = new bool[4];
    public int[] playerPrefabIndices = new int[4];
    public int levelIndex;


    public GameObject[] playerPrefabs;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

     

    
}
