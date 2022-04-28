using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool Turn;
    public GameState State;
    public static event Action<GameState> OnGameStateChange;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        UpdateGameState(GameState.EnemyTurn);
    }
    public void UpdateGameState(GameState newState)
    {
        State = newState;
        switch (newState)
        {
            case GameState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case GameState.EnemyTurn:
                HandleEnemyTurn();
                break;
            case GameState.Lose:
                HandleLose();
                break;
            case GameState.Win:
                HandleWin();
                break;
            default:
                break;
        }
        OnGameStateChange?.Invoke(newState);
    }

    private void HandleWin()
    {
        throw new NotImplementedException();
    }

    private void HandleLose()
    {
        throw new NotImplementedException();
    }

    private void HandleEnemyTurn()
    {
        Turn = false;
        Debug.Log("Enemy");
    }

    private void HandlePlayerTurn()
    {
        Turn = true;
        Debug.Log("Player");
    }
}
public enum GameState{
    PlayerTurn,
    EnemyTurn,
    Lose,
    Win
}
