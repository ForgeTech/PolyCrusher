using UnityEngine;
using System.Collections;

public interface InputInterface {

    float GetHorizontal(string playerPrefix);

    float GetVertical(string playerPrefix);

    bool GetButtonDown(string buttonName);
}
