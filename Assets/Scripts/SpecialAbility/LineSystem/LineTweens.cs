using UnityEngine;
using System.Collections;

public class LineTweens : MonoBehaviour {

    #region variables
    private LineSystem lineSystem;

    private float colorChangeSpeed = 0.4f;
    private float amplitudeChangeSpeed = 0.4f;
    #endregion

    #region property
    public LineSystem LineSystem
    {
        set { lineSystem = value; }
    }
    #endregion

    #region methods

    #region colorTween
    public void TweenColor(int lineNumber, int endColor, bool setActive)
    {
        int lerpcolor = endColor;

        if (endColor==-1)
        {
            lerpcolor = lineSystem.ActiveColor[lineNumber];
        }

        lineSystem.IsChangingColor[lineNumber] = true;
        LeanTween.value(gameObject, lineSystem.LineShaderUtilities[lineNumber].lineColor, lineSystem.Colors[lerpcolor], colorChangeSpeed)
           .setOnUpdate((Color lerpColor) => { lineSystem.LineShaderUtilities[lineNumber].lineColor = lerpColor; })
           .setEase(LeanTweenType.pingPong).setOnComplete(() =>
           {
               lineSystem.IsChangingColor[lineNumber] = false;
               if (setActive)
               {
                   lineSystem.ActiveColor[lineNumber] = endColor;
               }
           });
    }
    #endregion

    #region amplitudeTween
    public void TweenAmplitude(int lineNumber, float endAmplitude, LineStatus status)
    {
        lineSystem.IsChangingAmplitude[lineNumber] = true;
        LeanTween.value(gameObject, lineSystem.LineShaderUtilities[lineNumber].amplitude, endAmplitude, amplitudeChangeSpeed)
           .setOnUpdate((float lerpAmplitude) => { lineSystem.LineShaderUtilities[lineNumber].amplitude = lerpAmplitude; })
           .setEase(LeanTweenType.pingPong).setOnComplete(() =>
           {
               lineSystem.IsChangingAmplitude[lineNumber] = false;
               lineSystem.Status[lineNumber] = status; 
           });
    }

    public void TweenAmplitude(int lineNumber, float endAmplitude)
    {
        lineSystem.IsChangingAmplitude[lineNumber] = true;
        LeanTween.value(gameObject, lineSystem.LineShaderUtilities[lineNumber].amplitude, endAmplitude, amplitudeChangeSpeed)
           .setOnUpdate((float lerpAmplitude) => { lineSystem.LineShaderUtilities[lineNumber].amplitude = lerpAmplitude; })
           .setEase(LeanTweenType.pingPong).setOnComplete(() =>
           {
               lineSystem.IsChangingAmplitude[lineNumber] = false;
           });
    }

    public void TweenAmplitude(int lineNumber, float endAmplitude, LineShaderType lineType)
    {
        lineSystem.IsChangingAmplitude[lineNumber] = true;
        LeanTween.value(gameObject, lineSystem.LineShaderUtilities[lineNumber].amplitude, endAmplitude, amplitudeChangeSpeed)
           .setOnUpdate((float lerpAmplitude) => { lineSystem.LineShaderUtilities[lineNumber].amplitude = lerpAmplitude; })
           .setEase(LeanTweenType.pingPong).setOnComplete(() =>
           {
               lineSystem.IsChangingAmplitude[lineNumber] = false;
               lineSystem.LineShaderUtilities[lineNumber].functionType = lineType;
           });
    }


    #endregion

    #endregion

}
