using UnityEngine;
using System.Collections;

public class Deactivator : MonoBehaviour
{

    public Rigidbody attachedRigid;
    public MeshRenderer attachedRenderer;
    public BoxCollider attachedCollider;
    public MeshFilter attachedFilter;



    void OnEnable()
    {
        Invoke("Deactivate", Random.Range(5.5f, 10.0f));
    }





    void Deactivate()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Destroy(box);
        }

        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            Destroy(renderer);
            attachedRenderer = gameObject.AddComponent<MeshRenderer>();

            attachedFilter = gameObject.AddComponent<MeshFilter>();

        }


        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
        {
            attachedFilter = gameObject.AddComponent<MeshFilter>();


        }

        gameObject.name = "Dummy(Clone)";
        this.gameObject.SetActive(false);
    }



    void OnDisable()
    {
        CancelInvoke();

    }


}
