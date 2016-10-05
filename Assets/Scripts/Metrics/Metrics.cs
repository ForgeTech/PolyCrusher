using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MetricsDisplay))]
public class Metrics : MonoBehaviour {

    //general variables
    private bool metricsEnabled;
    private float refreshFrequency;
    private MetricsDisplay metricsDisplay;

    //FPS Module variables
    private bool isFPSRunning;
    private bool frameRateEnabled;
    int lastFrameCount;
    float lastTime;
    float timeSpan;
    int frameCount;

    //average FPS
    private float FPSSum;
    private int FPSCount;
    private float averageFPS;

    private WaitForSeconds frequency;
    private string fpsText;
    private string avgText;
    private List<float> lastFpsCounts;
    private float fpsAverage;
   

	// Use this for initialization
	void Start () {        
        FPSSum = 0.0f;
        FPSCount = 0;
        lastFpsCounts = new List<float>(10);

        frameRateEnabled = false;
        refreshFrequency = 0.25f;
     
        metricsDisplay = GetComponent<MetricsDisplay>();

        frequency = new WaitForSeconds(refreshFrequency);

        StartCoroutine(CalculateFPS());
	}

    void Update()
    {
        HandleInput();
        HandleModuleActivation();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            frameRateEnabled = !frameRateEnabled;       
        }       
    }

    private void HandleModuleActivation()
    {
        if(!isFPSRunning && frameRateEnabled)
        {
            StartCoroutine(CalculateFPS());
        }
    }

    private IEnumerator CalculateFPS()
    {
        isFPSRunning = true;
        int count = 0;
        while (frameRateEnabled)
        {           
            lastFrameCount = Time.frameCount;
            lastTime = Time.realtimeSinceStartup;
            yield return frequency;
            timeSpan = Time.realtimeSinceStartup - lastTime;
            frameCount = Time.frameCount - lastFrameCount;

            float fps = frameCount / (float)timeSpan;
            lastFpsCounts.Add(fps);
            if(lastFpsCounts.Count > 10)
            {
                lastFpsCounts.RemoveAt(0);
            }
            fpsText = fps.ToString("f" + Mathf.Clamp(1, 0, 10));
            count++;
            if (count >= 4)
            {
                count = 0;
                avgText = SmoothFPS().ToString("f" + Mathf.Clamp(1, 0, 10));
            }
            metricsDisplay.CurFPS = "FPS: " + fpsText + " \nAVG: " + avgText;
        }
        isFPSRunning = false;    
    }

    private float SmoothFPS()
    {       
        float fps = 0;
        for(int i = 0; i < lastFpsCounts.Count; i++)
        {
            fps += lastFpsCounts[i];
        }

        return fps /(float) lastFpsCounts.Count;
    }
}
