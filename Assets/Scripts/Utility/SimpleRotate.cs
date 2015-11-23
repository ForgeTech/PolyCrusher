using UnityEngine;
using System.Collections;

public class SimpleRotate : MonoBehaviour
{
    public Vector3 rotation = Vector3.zero;

    [SerializeField]
    [Tooltip("Activates a time based rotation.")]
    protected bool timeBasedRotation = false;

    [SerializeField]
    protected float rotateInterval = 0.3f;

    protected float currentInterval = 0f;

	// Update is called once per frame
	void Update ()
    {
        if (!timeBasedRotation)
            transform.Rotate(rotation);
        else
        {
            if (currentInterval >= rotateInterval)
            {
                transform.Rotate(rotation);
                currentInterval = 0;
            }

            currentInterval += Time.deltaTime;
        }
	}
}
