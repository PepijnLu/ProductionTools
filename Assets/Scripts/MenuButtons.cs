using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public void ToggleBlockSelecter()
    {
       GameObject blockSelect = UIManager.instance.GetUIElementFromDict("BlockSelect");
       GameObject gridRenderer = UIManager.instance.GetUIElementFromDict("GridRenderer");

       UIManager.instance.ToggleUIElement(blockSelect, !blockSelect.activeSelf);
       UIManager.instance.ToggleUIElement(gridRenderer, !blockSelect.activeSelf);
    }

    public void ChangeBlockPage(int _increment)
    {
        UIManager.instance.ChangeBlockSelectPange(_increment);
    }
}
