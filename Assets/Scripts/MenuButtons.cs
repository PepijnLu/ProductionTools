using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] LevelBuilder levelBuilder;
    [SerializeField] PlayerController playerController;
    [SerializeField] GridRenderer gridRenderer;
    public void ToggleBlockSelecter()
    {

        GameObject blockSelect = UIManager.instance.GetUIElementFromDict("BlockSelect");
        if(UIManager.instance.inMenu && !blockSelect.activeSelf) return;

        GameObject gridRenderer = UIManager.instance.GetUIElementFromDict("GridRenderer");

        UIManager.instance.ToggleUIElement(blockSelect, !blockSelect.activeSelf);
        UIManager.instance.ToggleUIElement(gridRenderer, !blockSelect.activeSelf);
        UIManager.instance.inMenu = blockSelect.activeSelf;
    }

    public void ChangeBlockPage(int _increment)
    {
        UIManager.instance.ChangeBlockSelectPange(_increment);
    }

    public void SaveLevel()
    {
        SaveAndLoad.instance.SaveLevel(LevelLoaderData.loadedLevelName);
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene("LevelEditor");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void CreateToMainMenu(bool _toMain)
    {
        GameObject createMenu = UIManager.instance.GetUIElementFromDict("Create");
        GameObject mainMenu = UIManager.instance.GetUIElementFromDict("MainMenu");

        createMenu.SetActive(!_toMain);
        mainMenu.SetActive(_toMain);
    }

    public void CreateNewName(bool _create)
    {
        GameObject chooseName = UIManager.instance.GetUIElementFromDict("ChooseName");
        GameObject normalButtons = UIManager.instance.GetUIElementFromDict("NormalButtons");

        normalButtons.SetActive(!_create);
        chooseName.SetActive(_create);
    }

    public void CreateNewLevel(TMP_InputField _inputField)
    {
        if(ShowLevels.loadingLevel || _inputField.text == "") return;

        string levelName = _inputField.text;
        string fileName = _inputField.text + ".json";
        List<string> existingNames = ShowLevels.GetJsonFileNames(Application.persistentDataPath);
        if(existingNames.Contains(fileName))
        {
            Debug.Log("Level name already exists");
            return;
        }


        LevelLoaderData.loadedLevelName = levelName;
        ShowLevels.loadingLevel = true;
        SceneManager.LoadScene("LevelEditor");
    }

    public void EditLevel(bool _edit)
    {
        GameObject editMenu = UIManager.instance.GetUIElementFromDict("Edit");
        GameObject createMenu = UIManager.instance.GetUIElementFromDict("Create");

        editMenu.SetActive(_edit);
        createMenu.SetActive(!_edit);
    }

    public void StartLevelClearing(bool _start)
    {
        GameObject selectedBlockButton = UIManager.instance.GetUIElementFromDict("SelectedBlockButton");
        GameObject blockSelect = UIManager.instance.GetUIElementFromDict("BlockSelect");
        GameObject escapeMenu = UIManager.instance.GetUIElementFromDict("EscapeMenu");

        escapeMenu.SetActive(false);

        GameObject initialEscapeMenu = UIManager.instance.GetUIElementFromDict("InitialEscMenu");
        GameObject clearingEscapeMenu = UIManager.instance.GetUIElementFromDict("ClearingEscMenu");

        initialEscapeMenu.SetActive(!_start);
        clearingEscapeMenu.SetActive(_start);

        selectedBlockButton.SetActive(!_start);
        blockSelect.SetActive(false);

        gridRenderer.gameObject.SetActive(!_start);
    
        levelBuilder.enabled = !_start;;
        playerController.enabled = _start;
    }
 
}
