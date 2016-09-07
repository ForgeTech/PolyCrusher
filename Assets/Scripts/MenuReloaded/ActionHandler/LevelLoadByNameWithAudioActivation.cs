using UnityEngine;
using System.Collections;
using System;

public class LevelLoadByNameWithAudioActivation : LevelLoadByName
{
    public override void PerformAction<T>(T triggerInstance)
    {
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
            Debug.Log("No global scripts game object found!");

        base.PerformAction(triggerInstance);
    }
}