using UnityEngine;
using System.Collections;

public interface NetControllInterface {
    
    float GetLeftAnalogStickHorizontal(int smartphoneIndex);

    float GetLeftAnalogStickVertical(int smartphoneIndex);

    float GetRightAnalogStickHorizontal(int smartphoneIndex);

    float GetRightAnalogStickVertical(int smartphoneIndex);

    bool GetAbilityButton(int smartphoneIndex);

    bool GetJoinButton(int smartphoneIndex);

    bool GetBackButton(int smartphoneIndex);

    bool GetPauseButton(int smartphoneIndex);

    bool GetConnectionStatus(int smartphoneIndex);
}
