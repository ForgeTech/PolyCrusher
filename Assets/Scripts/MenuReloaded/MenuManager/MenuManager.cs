public class MenuManager : AbstractMenuManager
{
    public void DeactivateInput()
    {
        this.isInputActive = false;
    }

    public void ActivateInput()
    {
        this.isInputActive = true;
    }

    public override void DestroyMenuManager()
    {
        // Destroy children
        foreach (var pair in components)
            Destroy(pair.Value);

        // Destroy manager
        Destroy(this.gameObject);
    }
}