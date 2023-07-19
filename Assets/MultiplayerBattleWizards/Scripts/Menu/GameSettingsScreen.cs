using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsScreen : MonoBehaviour
{
    [Header("Maps")]
    public GameObject mapButtonPrefab;
    public RectTransform mapContainer;

    private MenuUI menu;

    void Awake ()
    {
        menu = GetComponent<MenuUI>();
    }

    // Called when the screen is activated.
    public void OnSetScreen()
    {

    }

    void Start ()
    {
        LoadMaps();
    }

    // Loads in the map buttons.
    void LoadMaps ()
    {
        // Get array of all maps.
        TextAsset[] mapNames = Resources.LoadAll<TextAsset>("Maps");

        // Create button and set text and onClick event.
        foreach(TextAsset map in mapNames)
        {
            GameObject mapObj = Instantiate(mapButtonPrefab, mapContainer.transform);
            mapObj.GetComponentInChildren<Text>().text = map.name;

            mapObj.GetComponent<Button>().onClick.AddListener(() => { OnSelectMap(map.name); });
        }
    }

    // Called when a map button is pressed - sends over a map name.
    // Sets the map to the room properties, then go back to the lobby screen.
    public void OnSelectMap (string mapName)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("map", mapName);
        PhotonNetwork.room.SetCustomProperties(hash);

        // Go back to lobby.
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }

    // Called when a "_ Kills" button is pressed to change game mode.
    // Sends over the amount of kills needed.
    public void OnGameModeKills (int kills)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("gamemode", (int)GameModeType.ScoreBased);
        hash.Add("gamemodeprop", kills);
        PhotonNetwork.room.SetCustomProperties(hash);

        // Go back to lobby.
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }

    // Called when a "_ Mins" button is pressed to change game mode.
    // Sends over the time in seconds.
    public void OnGameModeTime (int time)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("gamemode", (int)GameModeType.TimeBased);
        hash.Add("gamemodeprop", time);
        PhotonNetwork.room.SetCustomProperties(hash);

        // Go back to lobby.
        menu.SetScreen(MenuUI.MenuScreen.Lobby);
    }
}