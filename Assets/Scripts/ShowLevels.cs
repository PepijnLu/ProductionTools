using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowLevels : MonoBehaviour
{
    List<string> loadedLevels = new();
    [SerializeField] ChooseLevelButton chooseLevelButtonPrefab;
    [SerializeField] ChooseLevelButton currentActiveDropdown;
    [SerializeField] Transform gridLayout;
    public static bool loadingLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingLevel = false;
        LevelLoaderData.loadedLevelName = "";
        loadedLevels = GetJsonFileNames(Application.persistentDataPath);

        foreach (string _level in loadedLevels)
        {
            ChooseLevelButton newButton = Instantiate(chooseLevelButtonPrefab, gridLayout);
            newButton.showLevels = this;
            //Remove .json from the text
            string _levelName = _level.Substring(0, _level.Length - 5);
            newButton.levelName.text = _levelName;
        }
    }

    public static List<string> GetJsonFileNames(string folderPath)
    {
        List<string> jsonFileNames = new List<string>();

        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath, "*.json");
            foreach (string file in files)
            {
                jsonFileNames.Add(Path.GetFileName(file)); // Just the file name, not full path
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Folder not found: " + folderPath);
        }

        return jsonFileNames;
    }

    public void SelectLevel(ChooseLevelButton _button)
    {
        bool _disabled = false;
        //Disable dropdown if pressed while dropped down
        if(currentActiveDropdown != null)
        {
            if(currentActiveDropdown == _button)
            {
                currentActiveDropdown.dropDown.SetActive(false);
                currentActiveDropdown = null;
                _disabled = true;
            }
            
            //Disable the previous one
            if(currentActiveDropdown != null)
            {
                currentActiveDropdown.dropDown.SetActive(false);
                currentActiveDropdown = null;
            }
        }

        //Enable the new one
        if(!_disabled)
        {
            currentActiveDropdown = _button;
            currentActiveDropdown.dropDown.SetActive(true);
        }
    }

    public void LoadLevel(string _levelName)
    {
        if(loadingLevel) return;
        LevelLoaderData.loadedLevelName = _levelName;
        loadingLevel = true;
        SceneManager.LoadScene("LevelEditor");
    }

    public void DeleteLevel(ChooseLevelButton _button, string _fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, _fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            UnityEngine.Debug.Log("File deleted successfully.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("File not found: " + path);
        }

        Destroy(_button.gameObject);
    }
}
