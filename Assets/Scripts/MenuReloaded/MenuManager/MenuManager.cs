public class MenuManager : AbstractMenuManager
{
    public override void DestroyMenuManager()
    {
        // Destroy children
        foreach (var pair in components)
            Destroy(pair.Value);

        // Destroy manager
        Destroy(this.gameObject);
    }
}