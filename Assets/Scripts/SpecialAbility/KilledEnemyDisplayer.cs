﻿using UnityEngine;
using UnityEngine.UI;

public class KilledEnemyDisplayer : MonoBehaviour {


    #region variables
    private GameObject Canvas;

    private Text enemyCount;
    private Text[] allTextElements;

    private float textAnimationSpeed =1.0f;

    private PolygonProperties polygonProperties;
    private AudioSource audioPlayer;
    #endregion

    #region properties
    public PolygonProperties PolygonProperties
    {
        set { polygonProperties = value;
            Initialize();
        }
    }
    #endregion

    // Use this for initialization
    void Start () {
        PolygonEnemyDetection.PolygonEnemyDeaths += DisplayEnemyCount;
	}
	
	
    private void Initialize()
    {
        GameObject g = Instantiate(polygonProperties.killedEnemiesGameObject) as GameObject;
        g.transform.parent = transform.parent;
        audioPlayer = g.AddComponent<AudioSource>();
        audioPlayer.clip = polygonProperties.polyChashRegister;
        audioPlayer.loop = false;
        audioPlayer.pitch = 1.5f;
        audioPlayer.Stop();
        allTextElements = g.GetComponentsInChildren<Text>();
        for(int i = 0; i < allTextElements.Length; i++)
        {
            if(allTextElements[i].name == "KillCount")
            {
                enemyCount = allTextElements[i];
            }
            allTextElements[i].enabled = false;
        }
    }

    private void DisplayEnemyCount(int bodyCount)
    {
        if (bodyCount != 0)
        {
            ActivateTexts(true);
            enemyCount.text = "0";
            StartTween(bodyCount);
        }
        
    }

    private void ActivateTexts(bool status)
    {
        for (int i = 0; i < allTextElements.Length; i++)
        {
            allTextElements[i].enabled = status;
        }
    }


  
    private void NumberCountUp(int bodyCount)
    {
        audioPlayer.time = 0.0f;
        audioPlayer.Play();
        LeanTween.value(gameObject, 0, bodyCount, textAnimationSpeed/3.0f)
        .setOnUpdate((float count) => {
            enemyCount.text = ((int)count).ToString();
        })
        .setEase(LeanTweenType.linear)
        .setOnComplete(() =>
        {
            SlowBiggerTween();
        });
    }

    private void StartTween(int bodyCount)
    {
        LeanTween.value(gameObject, Vector3.zero, Vector3.one*0.9f, textAnimationSpeed)
        .setOnUpdate((Vector3 scale) => {
            for (int i = 0; i < allTextElements.Length; i++)
            {
                allTextElements[i].rectTransform.localScale = scale;
            }
        })
        .setEase(LeanTweenType.easeInQuad)
        .setOnComplete(() =>
        {
            NumberCountUp(bodyCount);
        });
    }

    private void SlowBiggerTween()
    {
        LeanTween.value(gameObject, Vector3.one*0.9f, Vector3.one,textAnimationSpeed/2.0f)
       .setOnUpdate((Vector3 scale) => {
           for (int i = 0; i < allTextElements.Length; i++)
           {
               allTextElements[i].rectTransform.localScale = scale;
           }
       })
       .setEase(LeanTweenType.easeInOutSine)
       .setOnComplete(() =>
       {
           FastEndTween();
       });

    }

    private void FastEndTween()
    {
        LeanTween.value(gameObject, Vector3.one, Vector3.zero, textAnimationSpeed/5.0f)
      .setOnUpdate((Vector3 scale) => {
          for (int i = 0; i < allTextElements.Length; i++)
          {
              allTextElements[i].rectTransform.localScale = scale;
          }
      })
      .setEase(LeanTweenType.easeInQuad)
      .setOnComplete(() =>
      {
          ActivateTexts(false);
      });
    }



    //tween
//    LeanTween.moveY(rectTrans, rectTrans.position.y + 120, 3.5f).setEase(LeanTweenType.easeOutQuad);
//    LeanTween.alphaText(rectTrans, 0, 3.5f).setEase(LeanTweenType.easeOutQuart).setOnComplete(() => { Destroy(popup);
//});

//            LeanTween.scale(rectTrans, Vector3.one* multiplikator, 0.05f).setEase(LeanTweenType.easeInQuad).setOnComplete(() => { LeanTween.scale(rectTrans, Vector3.one* multiplikatorScale, 0.1f).setEase(LeanTweenType.easeInQuad); });
        
}