using UnityEngine;
using System.Collections;
using System;

public class LevelLoadByNameWithAudioActivation : AbstractActionHandler {
   
    [SerializeField]
    private string levelName = null;

    public override void PerformAction<T>(T triggerInstance)
    {
        //timescale back to normal
        Time.timeScale = 1f;

        //let the menumusic begin
        GameObject globalScripts = GameObject.FindGameObjectWithTag("GlobalScripts");
        if (globalScripts != null)
        {
            AudioSource menuMusic = globalScripts.GetComponent<AudioSource>();
            if (menuMusic != null)
            {
                menuMusic.Stop();
                menuMusic.volume = 1.0f;
                menuMusic.time = 0.0f;
                menuMusic.Play();
            }
        }
        else
        {
            Debug.Log("No global scripts game object found!");
        }

        //load the menu scene
        if (levelName != null)
            Application.LoadLevel(levelName);
        else
            Debug.LogError("Level name is null!");

        OnActionPerformed();
    }


}
