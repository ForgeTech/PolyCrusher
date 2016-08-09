using System;

public interface MenuInputHandler
{
    void HandleHorizontalInput(string playerPrefix, Action onInputLeft, Action onInputRight);
    void HandleVerticalInput(string playerPrefix, Action onInputLeft, Action onInputRight);
    void HandleSelectInput(string playerPrefix, Action onInput);
    void HandleBackInput(string playerPrefix, Action onInput);
}
