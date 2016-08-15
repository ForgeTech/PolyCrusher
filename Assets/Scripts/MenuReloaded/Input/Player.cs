using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public PlayerControlActions Actions { get; set; }

    Renderer cachedRenderer;


    void OnDisable()
    {
        if (Actions != null)
        {
            Actions.Destroy();
        }
    }


    void Start()
    {
        cachedRenderer = GetComponent<Renderer>();
    }


    void Update()
    {
        if (Actions == null)
        {
            // If no controller exists for this cube, just make it translucent.
            cachedRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
        }
        else
        {
            // Set object material color.
            cachedRenderer.material.color = GetColorFromInput();

            // Rotate target object.
            transform.Rotate(Vector3.down, 500.0f * Time.deltaTime * Actions.Back, Space.World);
            transform.Rotate(Vector3.right, 500.0f * Time.deltaTime * Actions.LeftHorizontal, Space.World);


        }
    }


    Color GetColorFromInput()
    {
        if (Actions.Join)
        {
            return Color.green;
        }

        if (Actions.Back)
        {
            return Color.red;
        }

        if (Actions.Pause)
        {
            return Color.blue;
        }

        if (Actions.Ability)
        {
            return Color.yellow;
        }

        return Color.white;
    }
}

