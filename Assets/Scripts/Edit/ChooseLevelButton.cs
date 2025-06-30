using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseLevelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject buttonsToShow;
    [SerializeField] GameObject editButtons, playButtons;
    [SerializeField] GameObject clearButton, uploadButton;
    [SerializeField] GameObject beatenIcons;
    public TextMeshProUGUI levelName, beatTime, maxCoins;
    public Image thumbnailImg, clearedImg, beatenImg;
    public Color clearedColor, beatenColor;
    ShowLevels showLevels;
    string levelPath;
    bool showBeatenIcons;

    public void SetCorrectIcons(bool _edit, bool _clearedOrBeaten, ShowLevels _showLevels = null, float _completionTime = 0, int _coinsCollected = 0)
    {
        showLevels = _showLevels;
        //From the edit meun
        if (_edit)
        {
            buttonsToShow = editButtons;
            clearedImg.gameObject.SetActive(true);
            levelPath = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
            if (_clearedOrBeaten)
            {
                clearedImg.color = clearedColor;
                clearButton.SetActive(false);
                uploadButton.SetActive(true);
            }
        }
        //From the play menu
        else
        {
            buttonsToShow = playButtons;
            beatenImg.gameObject.SetActive(true);
            levelPath = Path.Combine(Application.persistentDataPath, "Levels", "Play");

            if (_clearedOrBeaten)
            {
                showBeatenIcons = true;
                beatenImg.color = beatenColor;
                if (_completionTime != 0)
                {
                    string formattedCompletionTime = LevelFunctions.instance.GetFormattedTimeFromFloat(_completionTime);
                    beatTime.text = formattedCompletionTime;
                }
                maxCoins.text = $"{_coinsCollected}";
            }
        }

    }

    public void DisableIcons()
    {
        clearedImg.enabled = false;
        beatenImg.enabled = false;
    }

    public void UploadLevel()
    {
        SceneData.loadedLevelName = levelName.text;
        UIManager.instance.ToggleUIElement("UploadLevel", true);
        Transform levelTransform = UIManager.instance.GetUIElementFromDict("UploadLevelTransform").transform;
        string _levelName = levelName.text;
        ChooseLevelButton newButton = UIManager.instance.InstantiateLevelObject(levelTransform, _levelName, levelPath, true, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonsToShow != null) buttonsToShow.SetActive(true);
        if (showBeatenIcons) beatenIcons.SetActive(true);
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonsToShow != null) buttonsToShow.SetActive(false);
        if (showBeatenIcons) beatenIcons.SetActive(false);
    }

    public void LoadLevel(string _loadBehaviour)
    {
        SceneData.loadBehaviour = _loadBehaviour;
        Debug.Log($"Changed scene load behaviour to: {_loadBehaviour}");
        showLevels.LoadLevel(levelName.text);
    }

    public void DeleteLevel()
    {
        StartCoroutine(showLevels.DeleteLevel(this, levelName.text, levelPath));
    }
}
