using System;

public interface MenuInputHandler
{
    void HandleHorizontalInput(Action onInputLeft, Action onInputRight);
    void HandleVerticalInput(Action onInputLeft, Action onInputRight);
    void HandleSelectInput(Action onInput);
    void HandleBackInput(Action onInput);
    void DestroyPlayerAction();
}
