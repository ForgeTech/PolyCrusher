using UnityEngine;
using UnityEngine.UI;

public class ShadowTransition : TransitionHandlerInterface
{
    private const float LERP_TIME = 0.2f;

    public void OnDefocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        Material mat = GetMaterialFrom(gameobject);
        Color shadowColor = mat.GetColor("_ShadowColor");

        LeanTween.value(gameobject, shadowColor.a * 255f, 0f, LERP_TIME).setEase(info.EaseType)
            .setOnUpdate((float val) => {
                shadowColor.a = val / 255f;
                mat.SetColor("_ShadowColor", shadowColor);
            }).setUseEstimatedTime(true);
    }

    public void OnFocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        Material mat = GetMaterialFrom(gameobject);
        Color shadowColor = mat.GetColor("_ShadowColor");

        LeanTween.value(gameobject, 0f, info.ShadowAlphaSelected, LERP_TIME).setEase(info.EaseType)
            .setOnUpdate((float val) => {
                shadowColor.a = val / 255f;
                mat.SetColor("_ShadowColor", shadowColor);
            }).setUseEstimatedTime(true);
    }

    private Material GetMaterialFrom(GameObject g)
    {
        Material mat = null;

        Image img = g.GetComponent<Image>();
        Text txt = g.GetComponent<Text>();

        if (img != null)
            mat = img.material;
        else if (txt != null)
            mat = txt.material;

        return mat;
    }
}