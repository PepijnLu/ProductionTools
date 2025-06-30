using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] LevelBuilder levelBuilder;
    [SerializeField] PlayerController playerController;
    [SerializeField] GridRenderer gridRenderer;
    [SerializeField] ShowLevels showLevels;
    public void ToggleBlockSelecter()
    {
        UIManager.instance.ToggleBlockSelector();
    }

    public void ChangeBlockPage(int _increment)
    {
        UIManager.instance.ChangeBlockSelectPange(_increment);
    }

    public void SaveLevel()
    {
        SaveAndLoad.instance.SaveLevel(SceneData.loadedLevelName);
    }

    public void SetMenuToLoad(string _menuToLoad)
    {
        SceneData.menuToLoad = _menuToLoad;
    }

    public void SetLevelsToLoad(string _levels)
    {
        SceneData.levelsToLoad = _levels;
    }

    public void LoadScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void CreateToMainMenu(bool _toMain)
    {
        UIManager.instance.ToggleUIElement("Create", !_toMain);
        UIManager.instance.ToggleUIElement("MainMenu", _toMain);
    }

    public void CreateNewName(bool _create)
    {
        UIManager.instance.ToggleUIElement("ChooseName", _create);
        UIManager.instance.ToggleUIElement("NormalButtons", !_create);
    }

    public void ChangeLevelHealth(int _increment)
    {
        levelBuilder.ChangeLevelVariable("health", _increment);
    }

    public void ChangeLevelTime(int _increment)
    {
        levelBuilder.ChangeLevelVariable("timer", _increment);
    }

    public void RetryLevel(bool _clearing)
    {
        levelBuilder.StartLevelClearing(false, _clearing);
        levelBuilder.StartLevelClearing(true, _clearing);
    }

    public void CreateNewLevel(TMP_InputField _inputField)
    {
        if(ShowLevels.loadingLevel || _inputField.text == "") 
        {
            Debug.Log("Already loading level");
            return;
        }

        string levelName = _inputField.text;
        string fileName = _inputField.text + ".json";
        List<string> existingNames = ShowLevels.GetJsonFileNames(Path.Combine(Application.persistentDataPath, "Levels", "Edit"));
        if(existingNames.Contains(fileName))
        {
            Debug.Log("Level name already exists");
            StartCoroutine(UIManager.instance.ShowTextForSeconds("NameInUse", "name already in use!", 2f));
            return;
        }
        else Debug.Log("Loading level");

        SceneData.loadBehaviour = "Edit";
        SceneData.loadedLevelName = levelName;
        ShowLevels.loadingLevel = true;
        SceneManager.LoadScene("LevelEditor");
    }

    public void PlayMenu(bool _play)
    {
        UIManager.instance.ToggleUIElement("Play/Edit", _play);
        UIManager.instance.ToggleUIElement("MainMenu", !_play);

        if(_play) showLevels.LoadLevels(false);
        else showLevels.UnloadLevels();
    }

    public void DisableUploadScreen()
    {
        UIManager.instance.ToggleUIElement("UploadLevel", false);
    }

    public void UploadLevel(TMP_InputField _textInput)
    {
        LevelFunctions.instance.UploadLevel(_textInput);
    }

    public void EditLevel(bool _edit)
    {
        UIManager.instance.ToggleUIElement("Play/Edit", _edit);
        UIManager.instance.ToggleUIElement("Create", !_edit);

        if (_edit) showLevels.LoadLevels(true);
        else showLevels.UnloadLevels();
    }

    public void StartLevelClearing(bool _start)
    {
        SceneData.loadBehaviour = "Clear";
        levelBuilder.StartLevelClearing(_start, true);
    }

    public void ConfirmDelete(bool _confirm)
    {
        showLevels.ConfirmDelete(_confirm);
    }
 
}
