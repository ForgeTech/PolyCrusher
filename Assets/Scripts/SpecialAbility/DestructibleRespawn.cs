using UnityEngine;
using System.Collections.Generic;

public class DestructibleRespawn : MonoBehaviour {

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private MeshRenderer meshRenderer;
    private Collider attachedCollider;
    private Rigidbody attachedRigidBody;
    private PolyExplosionThreeDimensional poly3DScript;
    private NavMeshObstacle attachedObstacle;

    void Awake()
    {
        originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        meshRenderer = GetComponent<MeshRenderer>();
        attachedCollider = GetComponent<Collider>();
        attachedRigidBody = GetComponent<Rigidbody>();
        poly3DScript = GetComponent<PolyExplosionThreeDimensional>();
        attachedObstacle = GetComponent<NavMeshObstacle>();
    }

    public void Respawn()
    {
        foreach(Deactivator deactivator in poly3DScript.deactivators)
        {
            deactivator.TriggerDeactivation(0.0f);
        }
        poly3DScript.deactivators.Clear();
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (attachedCollider != null)
        {
            attachedCollider.enabled = true;
        }
       
        if(meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        attachedRigidBody.isKinematic = false;
        attachedObstacle.enabled = true;
    }
}
