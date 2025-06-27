using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseLevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ShowLevels showLevels;
    GameObject buttonsToShow;
    [SerializeField] GameObject editButtons, playButtons;
    public TextMeshProUGUI levelName;
    public Image thumbnailImg, clearedImg, beatenImg;
    public Color clearedColor, beatenColor;
    string levelPath;

    public void SetCorrectIcons(bool _edit, bool clearedOrBeaten, ShowLevels _showLevels)
    {
        showLevels = _showLevels;

        //From the edit meun
        if(_edit) 
        {
            buttonsToShow = editButtons;
            clearedImg.gameObject.SetActive(true);
            levelPath = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
            if(clearedOrBeaten) clearedImg.color = clearedColor;
        }
        //From the play menu
        else 
        {
            buttonsToShow = playButtons;
            beatenImg.gameObject.SetActive(true);
            levelPath = Path.Combine(Application.persistentDataPath, "Levels", "Play");

            if(clearedOrBeaten) beatenImg.color = beatenColor;
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(buttonsToShow != null) buttonsToShow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(buttonsToShow != null) buttonsToShow.SetActive(false);
    }

    public void LoadLevel(string _loadBehaviour)
    {
        SceneData.loadBehaviour = _loadBehaviour;
        showLevels.LoadLevel(levelName.text);
    }

    public void DeleteLevel()
    {
        
        StartCoroutine(showLevels.DeleteLevel(this, levelName.text, levelPath));
    }
}
