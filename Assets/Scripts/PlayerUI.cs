using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    #region

    private Client Client;
    private LocalPlayer LocalPlayer;

    #endregion

    #region Public variables
    public Camera MCamera;
    public Transform CameraOrigin;
    public GameObject UIMenu;
    public GameObject VictoryDefeatWindow;
    public SoundManager soundManager;
    public Text WinText;
    #endregion

    #region UiElementsReferences

    [Header("Buttons")]
    public Button ThrowButton;
    public Button LayOffButton;
    public Button CompleteMarchButton;
    [Space]

    [Header("Text")]
    public Text PlayerName;
    public Text PlayerScorePoints;
    public Text PlayerRoundPoints;
    public Text PlayerLayedPoints;
    [Space]

    public Text OponnentName;
    public Text OpponentScorePoints;
    public Text OpponentRoundPoints;

    #endregion

    #region Monobehavior constructions
    private void Awake()
    {
        soundManager.PlayGameMusic();
    }
    private void Start()
    {
        Client = FindObjectOfType(typeof(Client)) as Client;
        LocalPlayer = FindObjectOfType(typeof(LocalPlayer)) as LocalPlayer;
        OponnentName.text = Client.OpponentsName;
        PlayerName.text = Client.PlayerName;
    }

    #endregion

    #region Buttons methods

    public void Throw()
    {
        ThrowButton.gameObject.SetActive(false);
        LocalPlayer.ThrowDices();
    }
    public void LayOff()
    {
        ThrowButton.gameObject.SetActive(true);
        LocalPlayer.LayOffDices();
    }
    public void CompleteMarch()
    {
        LocalPlayer.CompleteMarch();
    }
    public void BackToMenu()
    {
        MCamera.transform.position = CameraOrigin.position;
        MCamera.transform.rotation = CameraOrigin.rotation;
        soundManager.PlayMenuMusic();
        UIMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    #endregion
    public void DisableButtons(bool value)
    {
        ThrowButton.gameObject.SetActive(value);
        LayOffButton.gameObject.SetActive(value);
        CompleteMarchButton.gameObject.SetActive(value);
    }
    public void VictoryDefeatHandle(bool deside) {
        VictoryDefeatWindow.SetActive(true);
        if (deside)
        {
            WinText.text = "victory!";
        }
        else if (!deside)
        {
            WinText.text = "defeat(";
        }
    }
}
