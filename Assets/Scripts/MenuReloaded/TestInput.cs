using UnityEngine;

public class TestInput : InputInterface
{
    public bool GetButtonDown(string buttonName)
    {
        return Input.GetButtonDown(buttonName);
    }

    public float GetHorizontal(string playerPrefix)
    {
        return Input.GetAxis(playerPrefix + "Horizontal");
    }

    public float GetVertical(string playerPrefix)
    {
        return Input.GetAxis(playerPrefix + "Vertical");
    }
}
