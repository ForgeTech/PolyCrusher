using UnityEngine;
using System.Collections;

public class AnimCurveContainer : MonoBehaviour 
{
    [Header("AnimationCurves")]
    public AnimationCurve slightUpscale;

    public AnimationCurve upscale;

    public AnimationCurve pingPong;

    public AnimationCurve shortGrow;

	public AnimationCurve grow;

	public AnimationCurve shortUpscale;

    public AnimationCurve downscale;

    // private reference
    private static AnimCurveContainer animCurve;

    /// <summary>
    /// Gets the AnimCurveContainer instance.
    /// </summary>
    public static AnimCurveContainer AnimCurve
    {
        get
        {
            //If the instance isn't set yet, it will be set (Happens only the first time!)
            if (animCurve == null)
            {
                animCurve = GameObject.FindObjectOfType<AnimCurveContainer>();
            }

            return animCurve;
        }
    }
}
