using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Reflection;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    List<Transform> menuPanels = new();
    Transform currentPanel;
    Dictionary<string, GameObject> uiElementsDict;
    Dictionary<string, TextMeshProUGUI> textElementsDict;
    Dictionary<string, Sprite> ruleTilesDefaultSpritesDict;
    Image selectedTile;
    public bool inMenu;
    [Header("UI Elements")]
    [SerializeField] List<GameObject> uiElements;
    [SerializeField] List<TextMeshProUGUI> textElements;
    [SerializeField] TextMeshProUGUI selectPanelText;
    [SerializeField] ChooseLevelButton chooseLevelButtonPrefab;
    [SerializeField] Image panelLeftArrow;
    [SerializeField] Image panelRightArrow;
    [Header("Tiles")]
    [SerializeField] List<TileBase> groundTiles;
    [SerializeField] List<TileBase> itemTiles;
    [SerializeField] PickTile baseTilePicker;
    [SerializeField] List<Sprite> ruleTilesDefaultSprites;
    void Awake()
    {
        instance = this;
        uiElementsDict = new();
        foreach(GameObject _element in uiElements)
        {
            uiElementsDict.Add(_element.name, _element);
        }  
        textElementsDict = new();
        foreach(TextMeshProUGUI _element in textElements)
        {
            textElementsDict.Add(_element.name, _element);
        }  
    }

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "LevelEditor") SetupAllTileMenus();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenCloseMenu();
        }   
    }

    public GameObject GetUIElementFromDict(string _element)
    {
        if(uiElementsDict.ContainsKey(_element)) return uiElementsDict[_element];
        Debug.LogWarning(_element + " not found in dictionary");
        return null;
    }
    public TextMeshProUGUI GetTextElementFromDict(string _element)
    {
        if(textElementsDict.ContainsKey(_element)) return textElementsDict[_element];
        Debug.LogWarning(_element + " not found in dictionary");
        return null;
    }
    
    public void ToggleUIElement(string _element, bool _active)
    {
        if(_element == null) return; 
        GetUIElementFromDict(_element).SetActive(_active);
    }

    public void OpenCloseMenu()
    {
        GameObject escapeMenu = GetUIElementFromDict("EscapeMenu");
        ToggleUIElement("EscapeMenu", !escapeMenu.activeSelf);
        inMenu = escapeMenu.activeSelf;
    }

    public void UpdateSelectedTile(Sprite _newSprite)
    {
        if(selectedTile == null)
        {
            selectedTile = GetUIElementFromDict("SelectedTile").GetComponent<Image>();
        }
        selectedTile.sprite = _newSprite;
    }

    public void ChangeBlockSelectPange(int _increment)
    {
        int currentIndex = menuPanels.IndexOf(currentPanel);

        if(currentIndex + _increment < menuPanels.Count)
        {
            if(currentIndex + _increment < 0)
            {
                currentIndex = menuPanels.Count - 1;
            }
            else
            {
                currentIndex += _increment;
            }
        }
        else
        {
            currentIndex = 0;
        }

        currentPanel.gameObject.SetActive(false);
        currentPanel = menuPanels[currentIndex];
        currentPanel.gameObject.SetActive(true);

        selectPanelText.text = currentPanel.gameObject.name;
        Color currentPanelColor = currentPanel.gameObject.GetComponent<Image>().color;
        selectPanelText.color = currentPanelColor;
        panelLeftArrow.color = currentPanelColor;
        panelRightArrow.color = currentPanelColor;
    }

    void SetupAllTileMenus()
    {
        ruleTilesDefaultSpritesDict = new();
        foreach(Sprite _sprite in ruleTilesDefaultSprites)
        {
            ruleTilesDefaultSpritesDict.Add(_sprite.name, _sprite);
        }
        //Ground tiles
        SetupTileMenu(GetUIElementFromDict("Base Blocks").transform, groundTiles);

        //Item tiles
        SetupTileMenu(GetUIElementFromDict("Items").transform, itemTiles);
    }

    void SetupTileMenu(Transform _panel, List<TileBase> _tiles)
    {
        Color currentPanelColor = _panel.gameObject.GetComponent<Image>().color;

        foreach(TileBase _tile in _tiles)
        {
            PickTile newTilePicker = Instantiate(baseTilePicker, _panel);
            Image borderImage = newTilePicker.borderImage;
            Image tileImage = newTilePicker.tileImage;
            newTilePicker.tilemap = _panel.gameObject.name;

            Tile tile = _tile as Tile;

            if (tile != null)
            {
                tileImage.sprite = tile.sprite;
                newTilePicker._newSprite = tile.sprite;
            }
            else
            {
                if(ruleTilesDefaultSpritesDict.ContainsKey(_tile.name))
                {
                    Sprite _newSprite = ruleTilesDefaultSpritesDict[_tile.name];
                    tileImage.sprite = _newSprite;
                    newTilePicker._newSprite = _newSprite;
                }
                else throw new System.Exception($"No Sprite Info found for {_tile.name}");
            }
            borderImage.color = currentPanelColor;
            newTilePicker._tileToPick = _tile;
        }
        if(menuPanels.Count <= 0) 
        {
            currentPanel = _panel;
            panelLeftArrow.color = currentPanelColor;
            panelRightArrow.color = currentPanelColor;
        }
        else
        {
            _panel.gameObject.SetActive(false);
        }
        menuPanels.Add(_panel);
    }

    public ChooseLevelButton InstantiateLevelObject(Transform _levelTransform, string _levelName, string levelPath, bool _edit, bool _clearedOrBeaten)
    {
        ChooseLevelButton newButton = Instantiate(chooseLevelButtonPrefab, _levelTransform);
        newButton.levelName.text = _levelName;
        newButton.SetCorrectIcons(_edit, _clearedOrBeaten);

        string path = Path.Combine(levelPath, "Thumbnails");
        Texture2D thumbnail = LoadThumbnail(path, _levelName + ".png");
        if(thumbnail != null)
        {
            Sprite sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f));
            newButton.thumbnailImg.sprite = sprite;
        }

        newButton.gameObject.SetActive(true);
        return newButton;
    }

    public Texture2D LoadThumbnail(string _path, string _fileName)
    {
        string path = Path.Combine(_path, _fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Thumbnail not found: " + path);
            return null;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2); // Size will be overwritten
        tex.LoadImage(fileData); // Load the PNG data
        return tex;
    }

    public IEnumerator ShowTextForSeconds(string _element, string _text, float duration)
    {
        TextMeshProUGUI _tmpro = GetTextElementFromDict(_element);
        _tmpro.text = _text;
        yield return new WaitForSeconds(duration);
        _tmpro.text = "";

    }

    public void ChangeLevelName(TMP_InputField _name)
    {
        Debug.Log($"New Level Name: {_name}");
        if(_name.text == "")
        {
            GetUIElementFromDict("PickName").SetActive(true);
        }
        else
        {
            GetUIElementFromDict("PickName").SetActive(false);
        }
    }
}
