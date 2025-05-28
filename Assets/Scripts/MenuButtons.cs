using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void SaveLevel()
    {
        SaveAndLoad.instance.SaveLevel("test");
    }

    public void LoadLevel()
    {
        LevelLoaderData.levelName = "test";
        SceneManager.LoadScene("LevelEditor");
    }
    
}
