using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using MultiplayerLibrary;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    TcpClient TcpConnection;
    Thread ListeningThread;

    public string ServerIp = "26.212.183.214";
    public int ServerPort = 7777;

    public ConnectedPlayer connectedPlayer;
    public LocalPlayer player;
    public PlayerUI playerUI;
    public SoundManager soundManager;
    public int RoomKey;
    public User user;
    public bool AuthorizationInProgress = true;
    public bool GameFound = false;
    public bool PlayerTurn = false;
    public string OpponentsName;
    public string PlayerName;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
    }
    /// <summary>
    /// Starts working of client
    /// </summary>
    public void Run()
    {
        try
        {
            TcpConnection = new TcpClient();
            TcpConnection.Connect(ServerIp, ServerPort);

            ListeningThread = new Thread(Listen);
            ListeningThread.IsBackground = true;
            ListeningThread.Start();

        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex.Message);
        }
    }

    /// <summary>
    /// Action for listening incoming data
    /// </summary>
    public void Listen()
    {

        while (true)
        {
            NetworkStream stream = TcpConnection.GetStream();
            BinaryFormatter bf = new BinaryFormatter();
            while (stream.DataAvailable)
            {
                Message ListenedMessage = bf.Deserialize(stream) as Message;
                HandleMessage(ListenedMessage);
                Debug.Log("Handling Message...");
            }
        }
    
        
    }

    /// <summary>
    /// Handles received message and defines further work with it
    /// </summary>
    /// <param name="message">Message for handling</param>
    public void HandleMessage(Message message)
    {
        switch (message.Text)
        {
            //successfully logged in
            case "/authorized":
                Debug.Log("Authorized");
                user = message.PinnedObject as User;
                AuthorizationInProgress = false;
                break;

            //authorization failed
            case "/not authorized":
                Debug.Log("not authorized");
                break;

            //username already used
            case "/invalid name":
                Debug.Log("invalid name");
                break;

            //match found
            case "/set game room":
                RoomKey = message.Key;
                OpponentsName = message.PinnedObject as string;
                player.SendInfoToServer(LocalPlayer.SendAction.sync);
                UnityMainThread.wkr.AddJob(() => soundManager.PlayGameFound());
                GameFound = true;
                break;

            case "/sync":
                Player rp = message.PinnedObject as Player;
                if (rp == null)
                    return;
                UnityMainThread.wkr.AddJob(() => connectedPlayer.SaveData(rp.AllPoints, rp.RoundPoints, rp.DicesValues));
                break;

            case "/change turn":
                PlayerTurn = true;
                UnityMainThread.wkr.AddJob(() => playerUI.DisableButtons(PlayerTurn));
                break;

            case "/disconnect room":
                break;

            case "/victory":
                Debug.Log("Victory");
                UnityMainThread.wkr.AddJob(() => { 
                    playerUI.VictoryDefeatHandle(true);
                    soundManager.PlayVictorySound();
                });
                break;

            case "/defeat":
                Debug.Log("Defeat");
                UnityMainThread.wkr.AddJob(() => {
                    playerUI.VictoryDefeatHandle(false);
                    soundManager.PlayDefeatSound();
                    });
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Sends message to server
    /// </summary>
    /// <param name="message">Sending message</param>
    public void Send(object message)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, message as Message);
        byte[] buf = ms.ToArray();
        TcpConnection.GetStream().Write(buf, 0, buf.Length);
        Thread.Sleep(250);
    }

    /// <summary>
    /// Registrates new user
    /// </summary>
    public void RegisterUser(string username, string password)
    {
        List<string> userInfo = new List<string>() { username, password };
        Send(new Message()
        {
            Text = "/reg user",
            PinnedObject = userInfo
        });
    }

    /// <summary>
    /// Authorizes existing user
    /// </summary>
    public void AuthorizeUser(string username, string password)
    {
        List<string> userInfo = new List<string>() { username, password };
        Send(new Message()
        {
            Text = "/log user",
            PinnedObject = userInfo
        });
    }
}
