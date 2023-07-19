#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;

public class MapEditor : MonoBehaviour
{
    public string resourceFolderPath;       // Path to the "Resources" folder.

    public List<Sprite> allSprites = new List<Sprite>();        // List of all the available sprites to use (for loading in maps).

    [HideInInspector]
    public List<GameObject> tiles = new List<GameObject>();     // List of all tiles currently in our map.

    [HideInInspector]
    public List<int> bgTilesIndexes = new List<int>();

    public GameObject tilePrefab;           // Prefab we instantiate when we place a tile.
    public Sprite defaultTile;              // The default sprite at runtime.
    public Sprite spawnPointTile;           // Sprite to represent the player spawn points.

    private Sprite curTile;                 // Our currently selected tile sprite.

    private int minSpawnPoints = 4;         // Minimum amount of spawn points required.

    [Header("UI")]
    public InputField mapNameInput;         // Input field to write name of map to save or load.
    public Text spawnPointsText;            // Text displaying current amount of spawn points.
    public Image currSelectImage;           // Image showing our current tile.

    void Awake ()
    {
        SetCurTile(defaultTile);
    }

    void Update ()
    {
        // Place tile.
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Get world position of mouse and round to int.
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PlaceTile(new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0));
        }

        // Place tile as background tile.
        if (Input.GetMouseButtonDown(3) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Get world position of mouse and round to int.
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PlaceTile(new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0), true);
        }


        // Remove tile.
        else if(Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Get world position of mouse and round to int.
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryRemoveTile(new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0));
        }
    }

    // Called when the user LEFT clicks on the grid.
    // Sends over the click position.
    void PlaceTile (Vector3 pos)
    {
        // Gets the existing tile (if any) at the position.
        GameObject existingTile = TileAtPosition(pos);

        // If we do have an existing tile here, remove it.
        if(existingTile != null)
        {
            if(!existingTile.name.Equals(curTile.name))
                RemoveTile(existingTile);
            else
                return;
        }

        // Create our new tile.
        GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

        tile.name = curTile.name;
        tile.GetComponent<SpriteRenderer>().sprite = curTile;
        tiles.Add(tile);

        // If this was a spawn point tile, update the text.
        if(tile.name == spawnPointTile.name)
            spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";
    }

    void PlaceTile(Vector3 pos, bool isBGTile)
    {
        // Gets the existing tile (if any) at the position.
        GameObject existingTile = TileAtPosition(pos);

        // If we do have an existing tile here, remove it.
        if (existingTile != null)
        {
            if (!existingTile.name.Equals(curTile.name))
                RemoveTile(existingTile);
            else
                return;
        }

        // Create our new tile.
        GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

        tile.name = curTile.name;
        tile.GetComponent<SpriteRenderer>().sprite = curTile;
        tiles.Add(tile);
        bgTilesIndexes.Add(tiles.FindIndex(x => x == tile));
        print("tile index: " + tiles.FindIndex(x => x == tile));
        // If this was a spawn point tile, update the text.
        if (tile.name == spawnPointTile.name)
            spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";
    }

    // Called when the user RIGHT clicks on the grid.
    // Sends over the click position.
    void TryRemoveTile (Vector3 pos)
    {
        // Gets the existing tile (if any) at the position.
        GameObject existingTile = TileAtPosition(pos);

        // Return if there's no tile.
        if(existingTile == null)
            return;

        // If this was a spawn point tile, update the text.
        if(existingTile.name == spawnPointTile.name)
            spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";

        // Remove it.
        RemoveTile(existingTile);
    }

    // Removes and destroys the given tile.
    void RemoveTile (GameObject tile)
    {
        if (tiles.Contains(tile))//remove BG tile index
            bgTilesIndexes.Remove(tiles.FindIndex(x => x == tile));
        tiles.Remove(tile);

        Destroy(tile);
    }

    // Returns the tile at the requested position. Null if no tile there.
    GameObject TileAtPosition (Vector3 pos)
    {
        return tiles.Find(x => x.transform.position == pos);
    }
    
    // Called when a UI tile button is selected.
    // Sets that as our current tile.
    public void SetCurTile (Sprite tile)
    {
        curTile = tile;
        currSelectImage.sprite = tile;
    }

    // Returns the amount of spawn points currently placed down.
    int AmountOfSpawnPoints ()
    {
        return tiles.FindAll(x => x.name == spawnPointTile.name).Count;
    }

    // Called when the "Save" button is pressed.
    // Serializes and saves the map to the Resources/Maps folder.
    public void Save ()
    {
        if(!CanSave())
            return;

        MapData data = new MapData(mapNameInput.text);

        // Gather spawn points.
        List<GameObject> spawnPointObjects = tiles.FindAll(x => x.name == spawnPointTile.name);
        data.spawnPoints = new Vector3[spawnPointObjects.Count];

        for(int x = 0; x < spawnPointObjects.Count; ++x)
            data.spawnPoints[x] = spawnPointObjects[x].transform.position;

        // Gather tile data.
        List<MapTileData> tileData = new List<MapTileData>();
        print("bgTilesIndexes "+ bgTilesIndexes);
        print("tiles " + tiles);
        print("tileData " + tileData);
        for (int x = 0; x < tiles.Count; ++x)
        {
            if(tiles[x].name != spawnPointTile.name)
                tileData.Add(new MapTileData(tiles[x].name, tiles[x].transform.position,(bgTilesIndexes.Contains(x))));
        }

        data.tiles = tileData.ToArray();

        // Serialize our data.
        string rawData = JsonUtility.ToJson(data);
        string path = resourceFolderPath + "/Maps/" + mapNameInput.text + ".json";

        // Save the JSON file to the Resources/Maps folder.
        using(FileStream fs = new FileStream(path, FileMode.Create))
        {
            using(StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(rawData);
            }
        }

        // Refresh the assets so it will appear in folder.
        UnityEditor.AssetDatabase.Refresh();

        Debug.Log("<b>" + mapNameInput.text + "</b> saved to: " + path);
    }

    // Called when the "Load" button is pressed.
    // Deserializes and loads the map from the Resources/Maps folder.
    public void Load ()
    {
        // Get the map file.
        TextAsset mapFile = (TextAsset)AssetDatabase.LoadAssetAtPath(resourceFolderPath + "/Maps/" + mapNameInput.text + ".json", typeof(TextAsset));

        // Does this map exist?
        if(mapFile == null)
        {
            Debug.LogError("Map with the name of '" + mapNameInput.text + "' not found.");
            return;
        }

        // Remove all existing tiles.
        for(int x = 0; x < tiles.Count; ++x)
            Destroy(tiles[x]);

        tiles.Clear();

        // Deserialize the map from JSON to MapData.
        MapData data = JsonUtility.FromJson<MapData>(mapFile.text);

        // Create tiles.
        foreach(MapTileData tile in data.tiles)
        {
            GameObject tileObj = Instantiate(tilePrefab, tile.position, Quaternion.identity, transform);
            tileObj.name = tile.tileName;
            tileObj.GetComponent<SpriteRenderer>().sprite = allSprites.Find(x => x.name == tile.tileName);

            tiles.Add(tileObj);
        }

        // Create spawn points.
        foreach(Vector3 sp in data.spawnPoints)
        {
            GameObject tileObj = Instantiate(tilePrefab, sp, Quaternion.identity, transform);
            tileObj.name = spawnPointTile.name;
            tileObj.GetComponent<SpriteRenderer>().sprite = spawnPointTile;

            tiles.Add(tileObj);
        }

        // Set the spawn points text.
        spawnPointsText.text = "Currently   <b>" + AmountOfSpawnPoints() + "</b>";
    }

    // Are we able to save our map in its current state?
    // Does it have enough player spawn points?
    bool CanSave ()
    {
        bool can = false;

        // Get list of all player spawn point tiles.
        List<GameObject> spawnPoints = tiles.FindAll(x => x.name == spawnPointTile.name);

        // There is a min number of spawn points required.
        if(spawnPoints.Count >= minSpawnPoints)
            can = true;
        else
        {
            Debug.LogError("A map requires a minimum of <b>" + minSpawnPoints + "</b> spawn points");
            return false;
        }

        // There needs to be a map name.
        if(mapNameInput.text.Length > 0)
            can = true;
        else
        {
            Debug.LogError("The map needs a name.", mapNameInput);
            return false;
        }

        return can;
    }

    // Called when the "File" (folder icon) button is pressed.
    // Navigates to the maps folder.
    public void OnFileButton ()
    {
        Object obj = AssetDatabase.LoadAssetAtPath(resourceFolderPath + "/Maps", typeof(Object));
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
}
#endif