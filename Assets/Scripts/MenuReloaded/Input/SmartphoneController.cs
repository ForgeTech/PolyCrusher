using UnityEngine;
using InControl;

public class SmartphoneController : InputDevice {

    #region variables
    private VirtualController virtualController;

    private Vector2 leftAnalogStick;
    private Vector2 rightAnalogStick;

    private bool abilityPressed;
    private bool joinPressed;
    private bool pausePressed;
    private bool backPressed;
    #endregion

    #region properties
    public Vector2 SetLeftAnalogStick
    {
        set { leftAnalogStick.Set(value.x, value.y); }
    }

    public Vector2 SetRightAnalogStick
    {
        set { rightAnalogStick.Set(value.x, value.y); }
    }

    public bool SetAbilityPressed
    {
        set { abilityPressed = value;
            Debug.Log("Ability was pressed");
        }
    }

    public bool SetJoinPressed
    {
        set { joinPressed = value; }
    }

    public bool SetPausePressed
    {
        set { pausePressed = value; }
    }

    public bool SetBackPressed
    {
        set { backPressed = value; }
    }
    #endregion

    #region methods

    #region le constructeur de malheur
    public SmartphoneController(VirtualController virtualController) : base( "Smartphone Controller" )
	{
        //the reference to the virtual controller
        this.virtualController = virtualController;

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

    #region controller states update
    public override void Update(ulong updateTick, float deltaTime)
    { 
        //updating left analog stick
        UpdateLeftStickWithValue(leftAnalogStick, updateTick, deltaTime);
        leftAnalogStick.Set(0, 0);

        //updating right analog stick
        UpdateRightStickWithValue(rightAnalogStick, updateTick, deltaTime);
        rightAnalogStick.Set(0, 0);

        //updating ability button
        UpdateWithState(InputControlType.LeftBumper, abilityPressed, updateTick, deltaTime);
        
        //updating join button
        UpdateWithState(InputControlType.Action1, abilityPressed, updateTick, deltaTime);
        abilityPressed = false;

        //updating back button
        UpdateWithState(InputControlType.Action2, backPressed, updateTick, deltaTime);
        backPressed = false;

        //updating pause button
        UpdateWithState(InputControlType.Action8, pausePressed, updateTick, deltaTime);
        pausePressed = false;
        
        //apply changes
        Commit(updateTick, deltaTime);
    }
    #endregion

    #region overriden rumble method for seperate smartphone handling
    public override void Vibrate(float leftMotor, float rightMotor)
    {
        base.Vibrate(leftMotor, rightMotor);
        Debug.Log("Smartphone Controller rumble");
    }
    #endregion

    #endregion
}
