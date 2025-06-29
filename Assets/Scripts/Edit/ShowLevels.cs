using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowLevels : MonoBehaviour
{
    List<string> loadedLevels = new();
    [SerializeField] ChooseLevelButton chooseLevelButtonPrefab;
    [SerializeField] ChooseLevelButton currentActiveDropdown;
    // [SerializeField] Transform editGridLayout, playGridLayout;
    // [SerializeField] ScrollRect editScrollRect, playScrollRect; 
    [SerializeField] RectTransform editContent;
    [SerializeField] List<GameObject> loadedLevelButtons = new();
    [SerializeField] GridLayoutGroup gridLayout;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform deleteLevel;
    public static bool loadingLevel;
    string editPath, playPath;
    int confirmDelete = -1;
    bool deletingLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Initialize directories
        editPath = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
        playPath = Path.Combine(Application.persistentDataPath, "Levels", "Play");

        if (!Directory.Exists(editPath)) Directory.CreateDirectory(editPath);
        if (!Directory.Exists(playPath)) Directory.CreateDirectory(playPath);

        string editThumbnailPath = Path.Combine(editPath, "Thumbnails");
        string playThumbnailPath = Path.Combine(playPath, "Thumbnails");

        if (!Directory.Exists(editThumbnailPath)) Directory.CreateDirectory(editThumbnailPath);
        if (!Directory.Exists(playThumbnailPath)) Directory.CreateDirectory(playThumbnailPath);

        //Load correct menu
        UIManager.instance.ToggleUIElement(SceneData.menuToLoad, true);

        if(SceneData.levelsToLoad == "Play") LoadLevels(false);
        if(SceneData.levelsToLoad == "Edit") LoadLevels(true);
    }
    public void LoadLevels(bool _edit)
    {
        loadingLevel = false;
        string path;

        if(_edit) path = editPath;
        else path = playPath;

        loadedLevels = GetJsonFileNames(path);
        int loadedButtons = 0;

        foreach (string _level in loadedLevels)
        {
            Debug.Log($"Level found: {_level}");

            string json = File.ReadAllText(Path.Combine(path, _level));
            LevelData _data = JsonConvert.DeserializeObject<LevelData>(json);
            bool levelCleared = _data.isCleared;
            bool beatenOrCleared = false;

            
            //Check if the level has been beaten/cleared
            if(_edit) beatenOrCleared = levelCleared;
            else beatenOrCleared = _data.isBeaten;
            //Instantiate new level object
            ChooseLevelButton newButton = UIManager.instance.InstantiateLevelObject(gridLayout.transform, _data.levelName, path, _edit, beatenOrCleared);
            //Set the right icons/references on the object
            newButton.SetCorrectIcons(_edit, beatenOrCleared, this, _data.fastestTime, _data.coinsCollected);
            //Add it to the list for clearing later
            loadedLevelButtons.Add(newButton.gameObject);

            loadedButtons++;
            
        }
        
        if(loadedButtons > 10) gridLayout.padding.left = 4;
        else gridLayout.padding.left = 12;
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(editContent);
        scrollRect.verticalNormalizedPosition = 1f;

        SceneData.loadedLevelName = "";
    }

    public void UploadLevel(TMP_InputField _textInput)
    {
        string oldLevelName = SceneData.loadedLevelName;
        string newLevelName = _textInput.text;

        if (newLevelName == "") return;

        string sourcePath = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
        string destinationPath = Path.Combine(Application.persistentDataPath, "Levels", "Play");

        string sourceLevelPath = Path.Combine(sourcePath, oldLevelName + ".json");
        string destinationLevelPath = Path.Combine(destinationPath, newLevelName + ".json");

        string sourceThumbnailPath = Path.Combine(sourcePath, "Thumbnails", oldLevelName + ".png");
        string destinationThumbnailPath = Path.Combine(destinationPath, "Thumbnails", newLevelName + ".png");

        if (File.Exists(destinationLevelPath))
        {
            Debug.LogWarning("File already exists: " + destinationLevelPath);
            StartCoroutine(UIManager.instance.ShowTextForSeconds("UP_NameInUse", "name already in use!", 2f));
        }
        else
        {
            //Copy the level to "Play" folder
            File.Copy(sourceLevelPath, destinationLevelPath);

            //Update the level name
            string json = File.ReadAllText(destinationLevelPath);
            JObject obj = JObject.Parse(json);
            obj["levelName"] = newLevelName;
            File.WriteAllText(destinationLevelPath, obj.ToString(Formatting.Indented));

            //Duplicate the thumbnail
            File.Copy(sourceThumbnailPath, destinationThumbnailPath);

            SceneData.menuToLoad = "Play/Edit";
            SceneData.levelsToLoad = "Play";
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void UnloadLevels()
    {
        Debug.Log("Unloading levels");
        foreach (GameObject obj in loadedLevelButtons)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        loadedLevelButtons.Clear();
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

    public void LoadLevel(string _levelName)
    {
        if(loadingLevel) return;
        SceneData.loadedLevelName = _levelName;
        loadingLevel = true;
        SceneManager.LoadScene("LevelEditor");
    }

    public void ConfirmDelete(bool _confirm)
    {
        if(_confirm) confirmDelete = 1;
        else confirmDelete = 0;
    }

    public IEnumerator DeleteLevel(ChooseLevelButton _button, string _levelName, string levelPath)
    {
        if(!deletingLevel)
        {
            deletingLevel = true;
            UIManager.instance.ToggleUIElement("DeleteLevel?", true);

            ChooseLevelButton newButton = UIManager.instance.InstantiateLevelObject(deleteLevel, _levelName, levelPath, false, false);
            newButton.DisableIcons();

            while (confirmDelete == -1)
            {
                yield return null;
            }
            
            if(confirmDelete == 1)
            {
                string jsonFileName = _levelName  + ".json";
                string thumbnailFileName = _levelName + ".png";

                string jsonPath = Path.Combine(levelPath, jsonFileName);
                string thumbnailPath = Path.Combine(levelPath, "Thumbnails", thumbnailFileName);

                //Delete json file
                if (File.Exists(jsonPath)) File.Delete(jsonPath);
                else Debug.LogWarning("File not found: " + jsonPath);

                //Delete thumbnail
                if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);
                else Debug.LogWarning("File not found: " + thumbnailPath);

                Destroy(_button.gameObject);
            }
            else if (confirmDelete != 0) throw new System.Exception($"Confirm delete has the wrong value: {confirmDelete}");

            UIManager.instance.ToggleUIElement("DeleteLevel?", false);
            Destroy(newButton);

            confirmDelete = -1;
            deletingLevel = false;
        }

    }
}
