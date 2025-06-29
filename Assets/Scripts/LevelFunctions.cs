using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LevelFunctions : MonoBehaviour
{
    public static LevelFunctions instance;
    void Awake()
    {
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

    public LevelData GetJsonFromPath(string pathToLevel)
    {
        string json = File.ReadAllText(pathToLevel);
        return JsonConvert.DeserializeObject<LevelData>(json);
    }

    public string GetFormattedTimeFromFloat(float _timer)
    {
        int minutes = (int)(_timer / 60);
        int seconds = (int)(_timer % 60);
        int milliseconds = (int)((_timer * 1000) % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}
