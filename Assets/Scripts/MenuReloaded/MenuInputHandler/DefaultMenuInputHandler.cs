using System;

public class DefaultMenuInputHandler : MenuInputHandler
{
    private readonly PlayerControlActions playerControlActions;
    public PlayerControlActions ControlAction { get { return playerControlActions; } }

    private float stickDeadZone = 0.4f;

    public DefaultMenuInputHandler(PlayerControlActions playerControlActions)
    {
        this.playerControlActions = playerControlActions;
    }

    public void HandleBackInput(Action onInput)
    {
        if (playerControlActions.Back.WasPressed)
            onInput();
    }

    public void HandleHorizontalInput(Action onInputLeft, Action onInputRight)
    {
        if (playerControlActions.LeftHorizontal > stickDeadZone)
            onInputRight();
        else if (playerControlActions.LeftHorizontal < -stickDeadZone)
            onInputLeft();
    }

    public void HandleSelectInput(Action onInput)
    {
        if (playerControlActions.Ability.WasPressed || playerControlActions.Join.WasPressed)
            onInput();
    }

    public void HandleVerticalInput(Action onInputLeft, Action onInputRight)
    {
        if (playerControlActions.LeftVertical > stickDeadZone)
            onInputRight();
        else if (playerControlActions.LeftVertical < -stickDeadZone)
            onInputLeft();
    }

    public void DestroyPlayerAction()
    {
        playerControlActions.Destroy();
    }
}