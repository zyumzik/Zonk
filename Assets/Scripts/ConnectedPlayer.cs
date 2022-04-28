using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectedPlayer : MonoBehaviour
{
    public LocalPlayer localPlayer;
    public List<Transform> DiceSpawns;
    public Text AllPointsText;
    public Text RoundPointsText;

    public int[] tempDicesValues = new int[6];
    public bool sameData = false;

    public void SaveData(int allPoints, int roundPoints, int[] diceValues)
    {
        string dices = "";
        for (int j = 0; j < tempDicesValues.Length; j++)
        {
            if (tempDicesValues[j] != diceValues[j])
            {
                sameData = false;
                break;
            }
            else
                continue;
        }
        if (!sameData)
        {
            for (int i = 0; i < diceValues.Length; i++)
            {
                dices += diceValues[i] + " ";
                Dice dice = localPlayer.Dices[i];
                if(dice.Value != diceValues[i])
                    dice.Value = diceValues[i];
                if (dice.Value != 0)
                    UnityMainThread.wkr.AddJob(() => dice.gameObject.SetActive(true));
                else if (dice.Value == 0)
                    UnityMainThread.wkr.AddJob(() => dice.gameObject.SetActive(false));
            }
        }
        UpdateUI(allPoints, roundPoints);
        tempDicesValues = diceValues;
        sameData = true;
    }
    void UpdateUI(int allPoints, int roundPoints)
    {
        AllPointsText.text = allPoints.ToString() + "/5000";
        RoundPointsText.text = roundPoints.ToString(); 
    }
}
