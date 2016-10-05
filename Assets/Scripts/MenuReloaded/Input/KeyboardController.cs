using UnityEngine;
using InControl;

public class KeyboardController : InputDevice {

    #region variables
    private Vector2 leftVector;
    private Vector2 rightVector;
    #endregion

    #region methods

    #region constructor
    public KeyboardController() : base( "Keyboard Controller" )
	{      
        leftVector = new Vector2();
        rightVector = new Vector2();

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
    #endregion

    #region updating the controls for the virtual controller
    public override void Update(ulong updateTick, float deltaTime)
    {
        //updating left analog stick
        leftVector.Set(CalculateInputValue(Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D)), CalculateInputValue(Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W)));
        UpdateLeftStickWithValue(leftVector, updateTick, deltaTime);

        //updating right analog stick
        rightVector.Set(CalculateInputValue(Input.GetKey(KeyCode.LeftArrow), Input.GetKey(KeyCode.RightArrow)), CalculateInputValue(Input.GetKey(KeyCode.DownArrow), Input.GetKey(KeyCode.UpArrow)));
        UpdateRightStickWithValue(rightVector, updateTick, deltaTime);

        //updating ability button
        UpdateWithState(InputControlType.LeftBumper, Input.GetKeyDown(KeyCode.Space), updateTick, deltaTime);

        //updating join button
        UpdateWithState(InputControlType.Action1, Input.GetKeyDown(KeyCode.Return), updateTick, deltaTime);

        //updating back button
        UpdateWithState(InputControlType.Action2, Input.GetKeyDown(KeyCode.Backspace), updateTick, deltaTime);

        //updating pause button
        UpdateWithState(InputControlType.Action8, Input.GetKeyDown(KeyCode.Escape), updateTick, deltaTime);

        //apply changes
        Commit(updateTick, deltaTime);
    }
    #endregion

    #region calculate movement vector
    private float CalculateInputValue(bool inputOne, bool inputTwo)
    {
        float value = 0.0f;
        if (inputOne)
        {
            value -= 1.0f;
        }

        if (inputTwo)
        {
            value += 1.0f;
        }
        return value;
    }
    #endregion

    #endregion
}
