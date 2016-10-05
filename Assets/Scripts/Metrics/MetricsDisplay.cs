using UnityEngine;
using UnityEngine.UI;

public class MetricsDisplay : MonoBehaviour {

    [SerializeField]
    private Text curFPS;


    public string CurFPS
    {
        set
        {
            curFPS.text = value;
        }
    }
}
