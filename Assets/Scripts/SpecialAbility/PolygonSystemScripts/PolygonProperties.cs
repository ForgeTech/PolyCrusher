using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class PolygonProperties : ScriptableObject {

    [Header("Sound files ")]
    public AudioClip polyLoading;
    public AudioClip polyExplosion;
    public AudioClip polyFail;
    public AudioClip polyChashRegister;

    [Header("Materials for the polygon")]
    public Material[] polygonMaterials;

    [Header("Hint arrow")]
    public GameObject hintArrow;

    [Header("Killed enemies canvas")]
    public GameObject killedEnemiesGameObject;

    [Header("The height of the polygon mesh when its not in use")]
    public float polyStartHeight;

    [Header("The final height of the polygon when used")]
    public float polyEndHeight;

    [Header("The height of the polygon")]
    public Vector3 heightOffset;

    [Header("Thickness of the polygon")]
    public Vector3 polygonThickness;

    [Header("The distance the players have to reach to execute the polygon ability")]
    public float[] requiredPolyDistance;

    [Header("Damage the boss takes when the polygon hits")]
    public int[] bossDamage;

    [Header("Required polygon trigger time")]
    public float requiredTriggerTime;



    [HideInInspector]
    public int[] linesNeeded = new int[] { 0, 1, 3, 6 };
    [HideInInspector]
    public int[] firstVertex = new int[] { 0, 1, 2, 0, 1, 2 };
    [HideInInspector]
    public int[] secondVertex = new int[] { 1, 2, 0, 3, 3, 3 };
    [HideInInspector]
    public int[] meshIndices = new int[] { 1, 2, 5, 5, 2, 1 };

    //[HideInInspector]
    //public int[] meshIndices = new int[] { 0, 1, 2, 2, 1, 0, 0, 1, 4, 4, 1, 0, 0, 3, 4, 4, 3, 0, 0, 2, 5, 5, 2, 0, 1, 4, 5, 5, 4, 1, 3, 4, 5, 5, 4, 3, 0, 3, 5, 5, 3, 0, 1, 2, 5, 5, 2, 1 };



}
