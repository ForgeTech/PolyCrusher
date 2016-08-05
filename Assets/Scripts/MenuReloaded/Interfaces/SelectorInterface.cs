public interface SelectorInterface
{
    int Current { get; }
       
    void Next();
    void Previous();
    void HandleSelectedElement();
}