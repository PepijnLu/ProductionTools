using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseLevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShowLevels showLevels;
    public GameObject buttons;
    public TextMeshProUGUI levelName;
    public Image thumbnailImg, clearedImg;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttons.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttons.SetActive(false);
    }

    // public void SelectLevel()
    // {
    //     showLevels.SelectLevel(this);
    // }

    public void LoadLevel()
    {
        showLevels.LoadLevel(levelName.text);
    }

    public void DeleteLevel()
    {
        showLevels.DeleteLevel(this, levelName.text + ".json");
    }
}
