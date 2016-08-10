using UnityEngine;
using System.Collections;

public class MenuReloadedUtil
{
    public static TransitionHandlerInterface[] MapTransitionEnumToHandler(TransitionEnum[] transitions)
    {
        TransitionHandlerInterface[] pickedTransitions = new TransitionHandlerInterface[transitions.Length];

        for (int i = 0; i < pickedTransitions.Length; i++)
        {
            switch (transitions[i])
            {
                case TransitionEnum.TextColorChange:
                    pickedTransitions[i] = new ColorTextTransition();
                    break;
                case TransitionEnum.SizeChange:
                    pickedTransitions[i] = new DefaultScaleTransition();
                    break;
                case TransitionEnum.ImageColorChange:
                    pickedTransitions[i] = new DefaultColorTransition();
                    break;
                case TransitionEnum.ScaleDescale:
                    pickedTransitions[i] = new ScaleDescaleTransition();
                    break;
                case TransitionEnum.NoOp:
                    pickedTransitions[i] = new NoOpTransitionHandler();
                    break;
            }
        }
        return pickedTransitions;
    }

    public static ElementPressedHandler[] MapElementPressedEnumToHandler(ElementPressedEnum[] pressedHandler)
    {
        ElementPressedHandler[] pickedPressedHandler = new ElementPressedHandler[pressedHandler.Length];

        for (int i = 0; i < pickedPressedHandler.Length; i++)
        {
            switch (pressedHandler[i])
            {
                case ElementPressedEnum.TextColorHandler:
                    pickedPressedHandler[i] = new ElementPressedTextColor();
                    break;
                case ElementPressedEnum.ImageColorHandler:
                    pickedPressedHandler[i] = new ElementPressedImageColor();
                    break;
                case ElementPressedEnum.SizeHandler:
                    pickedPressedHandler[i] = new ElementPressedSize();
                    break;
                case ElementPressedEnum.NoOp:
                    pickedPressedHandler[i] = new ElementPressedNoOp();
                    break;
            }
        }
        return pickedPressedHandler;
    }
}
