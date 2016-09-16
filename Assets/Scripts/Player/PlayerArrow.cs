using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 30f;

    private float currentYRotation = 0f;
    private Quaternion arrowRotation;
    private Quaternion originalRotation;

    private void Start()
    {
        originalRotation = transform.rotation;
        arrowRotation = originalRotation;
    }

	private void Update ()
    {
        ResetParentRotation();
        RotateYAxis();
	}

    private void ResetParentRotation()
    {
        transform.rotation = originalRotation;
    }

    private void RotateYAxis()
    {
        //currentYRotation += Time.deltaTime * rotationSpeed;
        //transform.Rotate(new Vector3(-90, currentYRotation, 0));

        arrowRotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * rotationSpeed);
        transform.rotation = arrowRotation;
    }
}
