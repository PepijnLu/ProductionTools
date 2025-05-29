using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseLevelButton : MonoBehaviour
{
    public ShowLevels showLevels;
    public GameObject dropDown;
    public TextMeshProUGUI levelName;

    public void SelectLevel()
    {
        showLevels.SelectLevel(this);
    }

    public void LoadLevel()
    {
        showLevels.LoadLevel(levelName.text);
    }

    public void DeleteLevel()
    {
        showLevels.DeleteLevel(this, levelName.text + ".json");
    }
}
