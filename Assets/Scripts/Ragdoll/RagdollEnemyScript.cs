using UnityEngine;
using System.Collections;

public class RagdollEnemyScript : MonoBehaviour
{
    // Name of the enemy
    [SerializeField]
    private string enemyName = "undentified";

    // Array of the powerUps
    [SerializeField]
    private GameObject[] boneArray;

    public GameObject[] BoneArray
    {
        get { return this.boneArray; }
    }

    public string EnemyName
    {
        get { return this.enemyName; }
    }
}

