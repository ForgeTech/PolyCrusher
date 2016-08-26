using UnityEngine;

[CreateAssetMenu(fileName = "Hint", menuName = "ScriptableObject/Hint", order = 1)]
public class Hint : ScriptableObject {

    public Sprite hintImage;
    public string hintTitle;
    public string hintText;
}
