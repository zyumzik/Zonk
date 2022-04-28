using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MultiplayerLibrary;

public class LocalPlayer : MonoBehaviour
{
    public GameObject DicePrefab;
    public List<Dice> Dices = new List<Dice>();
    
    public List<Dice> SelectedDices;
    public List<Transform> DiceSpawns;
    public int AllPoints = 0;
    public int RoundPoints = 0;
    public int TurnPoints = 0;
    public int LayedPoints = 0;
    public int Action = 0;
    public bool _layPressed = false;
    public GameObject UserInterface;
    public GameObject ConnectedPlayer;
    private ConnectedPlayer connectedPlayer;


    private PlayerUI playerUI;
    private Client client;

    #region UI references
    [Header("UI references")]
    public Text AllPointsText;
    public Text RoundPointsText;
    public Text LayedPointsText;
    //[Space]
    #endregion

    #region Monodevelop constructions

    private void Awake()
    {
        client = FindObjectOfType<Client>().GetComponent<Client>();
        playerUI = UserInterface.GetComponent<PlayerUI>();
        connectedPlayer = ConnectedPlayer.GetComponent<ConnectedPlayer>();
    }

    void Update()
    {
        if (CheckCombination(SelectedDices) > 0)
        {
            playerUI.LayOffButton.gameObject.SetActive(true);
        }
        else
        {
            playerUI.LayOffButton.gameObject.SetActive(false);
        }
        #region Making march
        if (client.PlayerTurn)
        {
            // choosing dice
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                Dice dice = null;
                
                if (hit.collider != null & hit.collider.CompareTag("Dice"))

                {
                    dice = hit.collider.GetComponent<Dice>();
                    for (int i = 0; i < SelectedDices.Count; i++)
                    {
                        if (dice == SelectedDices[i])
                        {
                            SelectedDices.Remove(dice);
                            dice.Selection.SetActive(false);

                            // UI
                            LayedPointsText.text = CheckCombination(SelectedDices).ToString();

                            return;
                        }
                    }
                    SelectedDices.Add(dice);
                    dice.Selection.SetActive(true);

                    // UI
                    LayedPointsText.text = CheckCombination(SelectedDices).ToString();
                }
            }
            
            
        }
        #endregion
    }

    #endregion

    #region Buttons functions
    public void ThrowDices()
    {
        _layPressed = false;
        switch (Action)
        {
            case 0:
                for (int i = 0; i < Dices.Count; i++)
                {
                    Dice dice = Dices[i];
                    dice.gameObject.SetActive(true);
                    dice.Value = Random.Range(1, 7);
                    dice.transform.position = DiceSpawns[i].position;
                }
                Action = 1;
                ChangeTextOnButton("Rethrow(Q)");
                break;
            case 1:
                for (int i = 0; i < Dices.Count; i++)
                {
                    Dice dice = Dices[i];
                    if (dice.Value == 0) continue;
                    dice.gameObject.SetActive(true);
                    dice.Value = Random.Range(1, 7);
                    dice.transform.position = DiceSpawns[i].position;
                }
                break;
            default:
                break;
        }
        ClearSelection(Dices);
        SendInfoToServer(SendAction.sync);
    }    

    /// <summary>
    /// Counts points for {SelectedDices} then destroys them
    /// </summary>
    public void LayOffDices()
    {
        
        TurnPoints = 0;
        if (SelectedDices.Count > 0)
        {
            TurnPoints += CheckCombination(SelectedDices);
            if (TurnPoints > 0)
            {
                RoundPoints += TurnPoints;

                foreach (var item in SelectedDices)
                {
                    item.gameObject.SetActive(false);
                    item.Value = 0;
                }
                SelectedDices.Clear();

                //UI
                RoundPointsText.text = RoundPoints.ToString();
                LayedPointsText.text = "";
            }
            else return;
        }
        _layPressed = true;
        ClearSelection(Dices);
        SendInfoToServer(SendAction.sync);
    }

    /// <summary>
    /// Ends player's march and lets another player make a march. Ends the game if player won
    /// </summary>
    public void CompleteMarch()
    {
        
        if (_layPressed == false)
        {
            RoundPoints = 0;
        }
        AllPoints += RoundPoints;
        foreach (var item in Dices)
        {
            UnityMainThread.wkr.AddJob(() => item.gameObject.SetActive(false));
        }
        
        // UI
        AllPointsText.text = AllPoints.ToString() + "/5000";
        RoundPointsText.text = "";
        RoundPoints = 0;
        foreach (var dice in SelectedDices)
        {
            dice.Selection.SetActive(false);
        }

        

        Action = 0;
        ChangeTextOnButton("Throw(Q)");
        ClearSelection(Dices);
        SendInfoToServer(SendAction.endMove);
    }
    #endregion

    /// <summary>
    /// Checks if array of dices contains any combination.
    /// </summary>
    /// <param name="dices">Array of dices where combination is checking</param>
    /// <returns>Amount of points if there is any combination. 0 if there is no combination or there is more than 1 combination</returns>
    private int CheckCombination(List<Dice> _dices)
    {
        if (_dices.Count == 0 | _dices == null)
            return 0;

        // removing wrong dices
        List<Dice> dices = new List<Dice>();
        for (int i = 0; i < _dices.Count; i++)
        {
            if (_dices[i] != null
                & _dices[i].Value > 0
                & _dices[i].Value <= 6)
            {
                dices.Add(_dices[i]);
            }
        }

        int result = 0;

        SortDices(dices);

        #region three or more same dices
        if (dices.Count >= 3)
        {
            int counter = 0;

            // check if values in array are the same
            for (int i = 0; i < dices.Count; i++)
            {
                if (dices[i].Value == dices[0].Value)
                    counter++;
            }

            if (counter == dices.Count)
            {
                result += dices[0].Value * 100;
                if (dices[0].Value == 1)
                    result = 1000;

                for (int i = 3; i < dices.Count; i++)
                {
                    result *= 2;
                }
                return result;
            }
            
        }
        #endregion

        #region six different values
        if (dices.Count == 6)
        {
            int counter = 0;
            for (int i = 0; i < dices.Count; i++)
            {
                if (dices[i].Value == i + 1)
                {
                    counter++;
                }
                else
                {
                    break;
                }
            }
            if (counter == 6)
            {
                return 1500;
            }
        }
        #endregion

        #region three pairs
        if (dices.Count == 6)
        {
            if ((dices[0].Value == dices[1].Value)
                & (dices[2].Value == dices[3].Value)
                & (dices[4].Value == dices[5].Value))
            {
                return 750;
            }
        }
        #endregion

        #region ones and fives
        if(dices.Count <= 2)
        {
            for (int i = 0; i < dices.Count; i++)
            {
                if(dices[i].Value == 1)
                {
                    result += 100;
                }
            }
            for (int j = 0; j < dices.Count; j++)
            {
                if (dices[j].Value == 5)
                {
                    result += 50;
                }
            }
        }

        LayedPoints = result;
        return result;
        #endregion
    }

    /// <summary>
    /// Sorts array of dices by value in ascending order
    /// </summary>
    /// <param name="dices">Array of dices need to be sorted</param>
    private void SortDices(List<Dice> dices)
    {
        for (int i = 0; i < dices.Count; i++)
        {
            for (int j = i + 1; j < dices.Count; j++)
            {
                if (dices[i].Value > dices[j].Value)
                {
                    var temp = dices[i];
                    dices[i] = dices[j];
                    dices[j] = temp;
                }
            }
        }
    }

    public void SendInfoToServer(SendAction action)
    {
        int[] diceValues = new int[6];
        for (int i = 0; i < 6; i++)
        {
            try
            {
                diceValues[i] = Dices[i].Value;
            }
            catch
            {
                diceValues[i] = 0;
            }
        }

        switch (action)
        {
            case SendAction.sync:
                client.Send(new Message()
                {
                    Text = "/sync",
                    PinnedObject = new Player()
                    {
                        AllPoints = this.AllPoints,
                        RoundPoints = this.RoundPoints,
                        DicesValues = diceValues
                    }
                });
                break;
            case SendAction.endMove: //end move 
                client.Send(new Message()
                {
                    Text = "/end move",
                    PinnedObject = new Player()
                    {
                        AllPoints = this.AllPoints,
                        RoundPoints = this.RoundPoints,
                        DicesValues = diceValues
                    }
                });
                client.PlayerTurn = false;
                playerUI.DisableButtons(client.PlayerTurn);
                break;
            default:
                break;
        }

    }
    private void ChangeTextOnButton(string text)
    {
        playerUI.ThrowButton.GetComponentInChildren<Text>().text = text;
    }
    private void ClearSelection(List<Dice> dices)
    {
        foreach(var dice in dices)
        {
            dice.Selection.SetActive(false);
        }
    }
    public enum SendAction
    {
        sync,
        endMove
    }
}
