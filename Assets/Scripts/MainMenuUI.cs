using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MultiplayerLibrary;

public class MainMenuUI : MonoBehaviour
{
    #region Private references
    private Client client;
    #endregion

    #region Public references
    public InputField UsernameField;
    public InputField PasswordField;
    public InputField IPField;
    public InputField PortField;
    public InputField KeyField; 

    public Button SearchGameButton;
    public Button EndSearchButton;
    public Button CreateRoom;
    public Button JoinRoom;
    public Button ExitWindow;
    public Button LobbyButton;

    public Text InfoLabel;
    
    public Slider MusicSlider;
    public Slider SoundSlider;
    public Slider GraphicsSlider;
    
    public GameObject IpChangeMenu;
    public GameObject LobbyWindow;
    public GameObject AuthorizationWindow;
    public GameObject OptionsWindow;
    public GameObject Menu;
    public GameObject AcceptGameWindow;
    public GameObject UIGame;
    
    public Transform CameraPlace;
    
    public Camera mainCamera;
    
    public SoundManager SoundManager;
    #endregion

    private bool _authorization;
    public bool Authorization
    {
        get { return _authorization; }
        set
        {
            _authorization = value;
            AuthorizationWindow.SetActive(value);
        }
    }
    private bool _gameFound;
    public bool GameFound
    {
        get { return _gameFound; }
        set
        {
            _gameFound = value;
            AcceptGameWindow.SetActive(value);
        }
    }

    #region Monodevelop constructions
    private void Awake()
    {
        SoundManager.PlayMenuMusic();
        client = FindObjectOfType<Client>() as Client;
    }
    private void Update()
    {
        InfoLabel.text = $"Games Played: {client.user.Matches} Victories: {client.user.Victories} Defeats: {client.user.Defeats}";
        Authorization = client.AuthorizationInProgress;
        GameFound = client.GameFound;
        if (Authorization)
            Menu.SetActive(false);
        else
            Menu.SetActive(true);
    }
    #endregion

    #region Buttons methods
    public void LogIn()
    {
        if (UsernameField.text == ""
            | PasswordField.text == "")
        {
            return;
        }
        client.PlayerName = UsernameField.text;
        client.Run();
        client.AuthorizeUser(UsernameField.text, PasswordField.text);
    }
    public void Register()
    {
        if (UsernameField.text == ""
            | PasswordField.text == "")
        {
            return;
        }
        client.PlayerName = UsernameField.text;
        client.Run();
        client.RegisterUser(UsernameField.text, PasswordField.text);
    }
    public void SearchGame()
    {
        SearchGameButton.gameObject.SetActive(false);
        EndSearchButton.gameObject.SetActive(true);
        client.Send(new Message()
        {
            Text = "/search game"
        });
    }
    public void EndSearch()
    {
        SearchGameButton.gameObject.SetActive(true);
        EndSearchButton.gameObject.SetActive(false);
        client.Send(new Message()
        {
            Text = "/stop search"
        });
    }

    public void Options()
    {
        OptionsWindow.SetActive(true);
    }
    public void ExitGame()
    {
        client.Send(new Message()
        {
            Text = "/disconnect room"
        });
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void LogOut()
    {
        client.Send(new Message()
        {
            Text = "/log out"
        });
        client.AuthorizationInProgress = true;
    }
    public void AcceptGame()
    {
        mainCamera.transform.position = CameraPlace.position;
        mainCamera.transform.rotation = CameraPlace.rotation;
        SoundManager.PlayGameMusic();
        client.GameFound = false;
        UIGame.SetActive(true);
        SearchGameButton.gameObject.SetActive(true);
        EndSearchButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
    public void DeclineGame()
    {
        client.Send(new Message()
        {
            Text = "/disconnect room"
        });
        EndSearch();
        client.GameFound = false;
    }
    public void ConfirmOptionsChanges()
    {
        SoundManager.SetMusicVolume(MusicSlider.value);
        SoundManager.SetSoundVolume(SoundSlider.value);
        switch (GraphicsSlider.value)
        {
            case 0:
                QualitySettings.SetQualityLevel(0);
                break;
            case 1:
                QualitySettings.SetQualityLevel(1);
                break;
            case 2:
                QualitySettings.SetQualityLevel(2);
                break;
            case 3:
                QualitySettings.SetQualityLevel(3);
                break;
            default:
                break;
        }
        OptionsWindow.SetActive(false);
    }
    public void ChangeIP()
    {
        IpChangeMenu.SetActive(true);
    }
    public void ConfirmIP()
    {
        if (IPField.text == ""
            | PortField.text == "")
        {
            return;
        }
        else
        {
            client.ServerIp = IPField.text;
            client.ServerPort = int.Parse(PortField.text);
        }
        IpChangeMenu.SetActive(false);
    }
    public void LobbyMenu()
    {
        LobbyWindow.SetActive(true);
        Menu.SetActive(false);
    }
    public void ExitLobbyMenu()
    {
        LobbyWindow.SetActive(false);
        Menu.SetActive(true);
    }
    public void CreateLobby()
    {
        int rk;
        if (KeyField.text == "")
        {
            return;
        }
        else
        {
            rk = int.Parse(KeyField.text);
        }
        client.Send(new Message()
        {
            Text = "/create room",
            Key = rk
        }) ;
        ExitLobbyMenu();
    }
    public void JoinLobby()
    {
        int rk;
        if (KeyField.text == "")
        {
            return;
        }
        else
        {
            rk = int.Parse(KeyField.text);
        }
        client.Send(new Message()
        {
            Text = "/join room",
            Key = rk
        });
        ExitLobbyMenu();
    }
    #endregion
}
