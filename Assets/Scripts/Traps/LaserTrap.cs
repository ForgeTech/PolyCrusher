using UnityEngine;
using System.Collections;

public class LaserTrap : Trap, ITriggerable
{
    [SerializeField]
    public Transform startPos;

    [SerializeField]
    public Transform endPos;

    [SerializeField]
    public LineRenderer line;

    public override void Trigger(Collider other)
    {
        if (!gameObject.GetComponent<Laser>())
        {
            gameObject.AddComponent<Laser>();
        }
    }
}
