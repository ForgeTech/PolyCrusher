using UnityEngine;
using System.Collections;
using InControl;

public class SmartphoneController : InputDevice {

    private int smartphoneID;

    //private Vector2 leftVector;
    //private Vector2 rightVector;
    
    public int SmartphoneIndex
    {
        get
        {
            return smartphoneID;
        }
    }

    public SmartphoneController(int smartphoneIndex) : base( "Smartphone Controller" )
	{
        this.smartphoneID = smartphoneIndex;

        //leftVector = new Vector2();
        //rightVector = new Vector2();

        //left analog stick 
        AddControl(InputControlType.LeftStickLeft, "Left Stick Left");
        AddControl(InputControlType.LeftStickRight, "Left Stick Right");
        AddControl(InputControlType.LeftStickUp, "Left Stick Up");
        AddControl(InputControlType.LeftStickDown, "Left Stick Down");

        //right analog stick
        AddControl(InputControlType.RightStickLeft, "Right Stick Left");
        AddControl(InputControlType.RightStickRight, "Right Stick Right");
        AddControl(InputControlType.RightStickUp, "Right Stick Up");
        AddControl(InputControlType.RightStickDown, "Right Stick Down");

        //ability button
        AddControl(InputControlType.LeftBumper, "Ability Button");

        //join button
        AddControl(InputControlType.Action1, "Join Button");

        //back button
        AddControl(InputControlType.Action2, "Back Button");

        //pause button
        AddControl(InputControlType.Action8, "Pause Button"); 
    }


    public override void Update(ulong updateTick, float deltaTime)
    {
        //if (networkController != null)
        //{
        //    //updating left analog stick
        //    leftVector.Set(networkController.GetLeftAnalogStickHorizontal(smartphoneID), networkController.GetLeftAnalogStickVertical(smartphoneID));
        //    UpdateLeftStickWithValue(leftVector, updateTick, deltaTime);

        //    //updating right analog stick
        //    rightVector.Set(networkController.GetRightAnalogStickHorizontal(smartphoneID), networkController.GetRightAnalogStickVertical(smartphoneID));
        //    UpdateRightStickWithValue(rightVector, updateTick, deltaTime);

        //    //updating ability button
        //    UpdateWithState(InputControlType.LeftBumper, networkController.GetAbilityButton(smartphoneID), updateTick, deltaTime);

        //    //updating join button
        //    UpdateWithState(InputControlType.Action1, networkController.GetJoinButton(smartphoneID), updateTick, deltaTime);

        //    //updating back button
        //    //UpdateWithState(InputControlType.Button1, networkController.GetBackButton(smartphoneID), updateTick, deltaTime);

        //    //updating pause button
        //    //UpdateWithState(InputControlType.Button6, networkController.GetPauseButton(smartphoneID), updateTick, deltaTime);

        //    //apply changes
        //    Commit(updateTick, deltaTime);
        //}
    }

    public override void Vibrate(float leftMotor, float rightMotor)
    {
        base.Vibrate(leftMotor, rightMotor);
        Debug.Log("Smartphone Controller rumble");
    }
}
