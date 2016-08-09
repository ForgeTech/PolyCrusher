using System;

public class DefaultMenuInputHandler : MenuInputHandler
{
    readonly InputInterface input;
    float stickDeadZone = 0.4f;

    public DefaultMenuInputHandler(InputInterface input)
    {
        this.input = input;
    }

    public void HandleBackInput(string playerPrefix, Action onInput)
    {
        // TODO: Wait for InControl implementation
    }

    public void HandleHorizontalInput(string playerPrefix, Action onInputLeft, Action onInputRight)
    {
        if (input.GetHorizontal(playerPrefix) > stickDeadZone)
            onInputRight();
        else if (input.GetHorizontal(playerPrefix) < -stickDeadZone)
            onInputLeft();
    }

    public void HandleSelectInput(string playerPrefix, Action onInput)
    {
        if (input.GetButtonDown(playerPrefix + "Ability"))
            onInput();
    }

    public void HandleVerticalInput(string playerPrefix, Action onInputLeft, Action onInputRight)
    {
        if (input.GetVertical(playerPrefix) > stickDeadZone)
            onInputRight();
        else if (input.GetVertical(playerPrefix) < -stickDeadZone)
            onInputLeft();
    }
}