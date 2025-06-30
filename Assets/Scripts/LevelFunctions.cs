using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFunctions : MonoBehaviour
{
    public static LevelFunctions instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicate
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateFieldInJson(string pathToLevel, string fieldToUpdate, string variableType, bool newBool = false, float newFloat = 0, string newString = "")
    {
        string json = File.ReadAllText(pathToLevel);

        JObject obj = JObject.Parse(json);
        switch (variableType)
        {
            case "string":
                obj[fieldToUpdate] = newString;
                break;
            case "bool":
                obj[fieldToUpdate] = newBool;
                break;
            case "float":
                obj[fieldToUpdate] = newFloat;
                break;
            case "int":
                obj[fieldToUpdate] = (int)newFloat;
                break;


        }
        File.WriteAllText(Path.Combine(pathToLevel), obj.ToString(Formatting.Indented));
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
            StartCoroutine(UIManager.instance.ShowTextForSeconds("UP_NameInUse", "name already in use!", 2f));
            Debug.LogWarning("File already exists: " + destinationLevelPath);
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

    public LevelData GetJsonFromPath(string pathToLevel)
    {
        if(File.Exists(pathToLevel))
        {
            string json = File.ReadAllText(pathToLevel);
            return JsonConvert.DeserializeObject<LevelData>(json);
        }
        else return null;
    }

    public string GetFormattedTimeFromFloat(float _timer)
    {
        int minutes = (int)(_timer / 60);
        int seconds = (int)(_timer % 60);
        int milliseconds = (int)((_timer * 1000) % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}
