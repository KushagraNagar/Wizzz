using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public List<GameObject> allTiles = new List<GameObject>();      // List of all the tiles we can spawn.

    // We'll create a list of the tile's we've spawned in before so they
    // can load in faster and not need to loop through all of the tiles.
    private List<GameObject> cachedTiles = new List<GameObject>();

    private List<GameObject> map = new List<GameObject>();          // List of all tiles in the map.

    public Vector3[] surfacePositions;

    // Instance
    public static MapLoader inst;

    void Awake()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != this && inst != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    // Called when the Game scene is loaded - creates the map.
    public void LoadMap (string mapName)
    {
        // Load the map file and deserialize it.
        TextAsset mapFile = Resources.Load<TextAsset>("Maps/" + mapName);
        MapData data = JsonUtility.FromJson<MapData>(mapFile.text);

        // Spawn tiles.
        GameObject mapContainer = new GameObject("_Map");

        foreach(MapTileData tile in data.tiles)
        {
            GameObject tileObj = Instantiate(GetTile(tile.tileName), tile.position, Quaternion.identity, mapContainer.transform);
            if (tile.isBGtile)
                Destroy(tileObj.GetComponent<BoxCollider2D>());
            map.Add(tileObj);
        }

        // Center the map.
        Vector3 centerOffset = GetCenterPoint(mapContainer.transform);
        mapContainer.transform.position -= centerOffset;

        // Set spawn points.
        for(int x = 0; x < data.spawnPoints.Length; ++x)
            data.spawnPoints[x] -= centerOffset;

        GameManager.inst.spawnPoints = data.spawnPoints;

        // Call onMapLoaded event.
        if(GameManager.inst.onMapLoaded != null)
            GameManager.inst.onMapLoaded.Invoke();
    }

    // Returns center point of the map.
    Vector3 GetCenterPoint (Transform mapContainer)
    {
        Rect bounds = new Rect(0, 0, 0, 0);

        for(int x = 0; x < mapContainer.childCount; ++x)
        {
            Vector3 pos = mapContainer.GetChild(x).position;

            if(pos.x < bounds.xMin)
                bounds.xMin = pos.x;
            if(pos.x > bounds.xMax)
                bounds.xMax = pos.x;
            if(pos.y < bounds.yMin)
                bounds.yMin = pos.y;
            if(pos.y > bounds.yMax)
                bounds.yMax = pos.y;
        }

        return new Vector3(bounds.center.x, bounds.center.y, 0);
    }

    // Returns the tile prefab with the given name.
    GameObject GetTile (string tileName)
    {
        GameObject tile = null;

        // Check if we have it in the cached tiles list.
        tile = cachedTiles.Find(x => x.name == tileName);

        if(tile != null)
            return tile;
        // Otherwise, let's just get it from the all tiles list.
        else
        {
            tile = allTiles.Find(x => x.name == tileName);

            if(tile == null)
                Debug.LogError("The tile '" + tileName + "' cannot be found.");
            else
                cachedTiles.Add(tile);

            return tile;
        }
    }

    // Returns an array of all positions above a tile for pickups to spawn on, etc.
    // Fairly expensive to run, so only do it once upon initiation.
    public Vector3[] GetSurfacePositions ()
    {
        // If we already have surface positions, return them.
        if(surfacePositions.Length > 0)
            return surfacePositions;

        List<Vector3> positions = new List<Vector3>();

        foreach(GameObject tile in map)
        {
            Vector3 abovePos = tile.transform.position + Vector3.up;

            if(map.Find(x => x.transform.position == abovePos) == null)
                positions.Add(abovePos);
        }

        surfacePositions = positions.ToArray();
        return surfacePositions;
    }

    // Returns the lowest Y pos on the map.
    // Used to calculate the min Y kill pos.
    public float GetLowestPoint ()
    {
        float minY = 0.0f;

        foreach(GameObject tile in map)
        {
            if(tile.transform.position.y < minY)
                minY = tile.transform.position.y;
        }

        return minY;
    }
}