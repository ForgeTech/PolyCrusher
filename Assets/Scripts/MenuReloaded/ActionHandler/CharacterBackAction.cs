public class CharacterBackAction : LevelLoadByName
{
    public override void PerformAction<T>(T triggerInstance)
    {
        CharacterMenuManager menu = GetComponent<CharacterMenuManager>();
        CharacterSelector selector = menu.Selector as CharacterSelector;

        bool wasSelected = selector.Deselect();

        if (wasSelected)
        {
            menu.Deselect();
            OnActionPerformed();
        }
        else
            base.PerformAction<T>(triggerInstance);
    }
}
