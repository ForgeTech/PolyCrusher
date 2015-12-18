using UnityEngine;
using System.Collections.Generic;

public class DestructibleRespawn : MonoBehaviour {


    private Transform originalTransform;
    private MeshRenderer meshRenderer;
    private Collider attachedCollider;
    private Rigidbody attachedRigidBody;
    private PolyExplosionThreeDimensional poly3DScript;
   

	void Awake()
    {
        originalTransform = this.transform;
        meshRenderer = GetComponent<MeshRenderer>();
        attachedCollider = GetComponent<Collider>();
        attachedRigidBody = GetComponent<Rigidbody>();
        poly3DScript = GetComponent<PolyExplosionThreeDimensional>();
    }

    public void Respawn()
    {
       
        foreach(Deactivator deactivator in poly3DScript.deactivators)
        {
            deactivator.TriggerDeactivation(0.0f);
        }
        transform.position = originalTransform.position;
        transform.rotation = originalTransform.rotation;
        transform.localScale = originalTransform.localScale;

        if (attachedCollider != null)
        {
            attachedCollider.enabled = true;
        }
       
        if(meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        attachedRigidBody.isKinematic = false;
     

        
    }
}
