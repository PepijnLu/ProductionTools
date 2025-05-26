using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Reflection;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    List<Transform> menuPanels = new();
    Transform currentPanel;
    Dictionary<string, GameObject> uiElementsDict;
    Dictionary<string, Sprite> ruleTilesDefaultSpritesDict;
    Image selectedTile;
    [Header("UI Elements")]
    [SerializeField] List<GameObject> uiElements;
    [SerializeField] TextMeshProUGUI selectPanelText;
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
    }

    void Start()
    {
        uiElementsDict = new();
        foreach(GameObject _element in uiElements)
        {
            uiElementsDict.Add(_element.name, _element);
        }   

        SetupAllTileMenus();
    }

    public GameObject GetUIElementFromDict(string _element)
    {
        return uiElementsDict[_element];
    }
    
    public void ToggleUIElement(GameObject _element, bool _active)
    {
        _element.SetActive(_active);
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
}
