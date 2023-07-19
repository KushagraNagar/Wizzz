using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages navigation between screens and button presses.
/// </summary>
public class MenuUI : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreenObj;
    public GameObject lobbyScreenObj;
    public GameObject connectScreenObj;
    public GameObject settingsScreenObj;
    public GameObject gameSettingsScreenObj;

    [Header("Buttons")]
    public Button createGameButton;
    public Button joinGameButton;
    public Button settingsButton;
    public Button quitButton;
    public Button lobbyButton;

    [Header("Screen Components")]
    public LobbyScreen lobbyScreen;
    public ConnectScreen connectScreen;
    public SettingsScreen settingsScreen;
    public GameSettingsScreen gameSettingsScreen;

    [Header("Components")]
    public Animator screenAnimator;
    public PhotonView photonView;

    public enum MenuScreen
    {
        Start,
        Lobby,
        Connect,
        Settings,
        GameSettings
    }

    void Start ()
    {
        // Disable network-based buttons.
        createGameButton.interactable = false;
        joinGameButton.interactable = false;
        settingsButton.interactable = false;

        // If we just finished a game, return to the lobby.
        if(PhotonNetwork.inRoom)
            EndGameReturnToLobby();
        else
        {
            // Set the initial screen to be the "Start" screen.
            SetScreen(MenuScreen.Start);
        }
    }

    #region Photon Events

    // Called when the player connects to the master server.
    public void OnConnectedToMaster ()
    {
        // Enable network-based buttons.
        createGameButton.interactable = true;
        joinGameButton.interactable = true;
        settingsButton.interactable = true;
    }

    // Called when the player creates a room.
    public void OnCreatedRoom()
    {
        SetScreen(MenuScreen.Lobby);
    }

    #endregion

    // Sets the currently visible screen.
    public void SetScreen (MenuScreen screen)
    {
        // Disable all screens.
        startScreenObj.SetActive(false);
        lobbyScreenObj.SetActive(false);
        connectScreenObj.SetActive(false);
        settingsScreenObj.SetActive(false);
        gameSettingsScreenObj.SetActive(false);

        // Enable desired screen.
        switch(screen)
        {
            case MenuScreen.Start:
            {
                startScreenObj.SetActive(true);
                break;
            }
            case MenuScreen.Lobby:
            {
                lobbyScreenObj.SetActive(true);
                lobbyScreen.OnSetScreen();
                break;
            }
            case MenuScreen.Connect:
            {
                connectScreenObj.SetActive(true);
                connectScreen.OnSetScreen();
                break;
            }
            case MenuScreen.Settings:
            {
                settingsScreenObj.SetActive(true);
                settingsScreen.OnSetScreen();
                break;
            }
            case MenuScreen.GameSettings:
            {
                gameSettingsScreenObj.SetActive(true);
                gameSettingsScreen.OnSetScreen();
                break;
            }
        }

        // Play screen enter animation.
        screenAnimator.Rebind();
    }

    // Called when the "Create Game" button gets pressed.
    public void OnCreateGameButton ()
    {
        NetworkManager.inst.CreateRoom();
    }

    // Called when the "Join Game" button gets pressed.
    public void OnJoinGameButton()
    {
        SetScreen(MenuScreen.Connect);
    }

    // Called when the "Settings" button gets pressed.
    public void OnSettingsButton ()
    {
        SetScreen(MenuScreen.Settings);
    }

    // Called when the "Quit" button gets pressed.
    public void OnQuitButton ()
    {
        Application.Quit();
    }

    // Called when the "Lobby" button gets pressed.
    // Only visible if the player is in a lobby.
    public void OnLobbyButton()
    {
        SetScreen(MenuScreen.Lobby);
    }

    // Called when the player joins a room in the lobby.
    // Enables the "Lobby" button and disables the ability to create/join a game.
    public void EnableLobbyButton ()
    {
        lobbyButton.gameObject.SetActive(true);
        createGameButton.gameObject.SetActive(false);
        joinGameButton.interactable = false;
    }

    // Called when the player disconnects from a room.
    public void DisableLobbyButton ()
    {
        lobbyButton.gameObject.SetActive(false);
        createGameButton.gameObject.SetActive(true);
        joinGameButton.interactable = true;
    }

    // Called when the player was just in a game and is now returning to the Menu scene.
    void EndGameReturnToLobby ()
    {
        EnableLobbyButton();
        SetScreen(MenuScreen.Lobby);
    }
}