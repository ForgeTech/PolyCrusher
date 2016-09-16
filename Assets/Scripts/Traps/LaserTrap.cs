using UnityEngine;
using System.Collections;

public class LaserTrap : Trap, ITriggerable
{
    //start position of the laser
    [SerializeField]
    public Transform startPos;

    //end position of the laser
    [SerializeField]
    public Transform endPos;

    //the line renderer in use
    [SerializeField]
    public LineShaderUtility lineShader;

    public AudioClip laserSound;
    public float volume;
    
    public override void Trigger(Collider other)
    {
        if (!gameObject.GetComponent<Laser>())
        {
            if (other.tag == "Player")
            {
                //sound event
                OnTrapTriggered();
            }
            gameObject.AddComponent<Laser>();
        }
    }
}
