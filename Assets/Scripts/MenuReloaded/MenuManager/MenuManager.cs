public class MenuManager : AbstractMenuManager
{
    public void RegisterSubMenu()
    {
        this.isInputActive = false;
    }

    public void DeRegisterSubMenu()
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