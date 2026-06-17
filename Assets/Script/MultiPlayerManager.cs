using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiPlayerManager : MonoBehaviour
{
    [Header("Respective Boards")]
    public GameObject playerOneBoard;
    public GameObject playerTwoBoard;

    [Header("Prefabs")]
    public GameObject playerOneMissle;
    public GameObject playerTwoMissle;
    public GameObject blast;

    public GameObject[] playerOneShips;
    public GameObject[] playerTwoShips;

    List<Vector3> playerOneShipsInitialPositions = new();
    List<Vector3> playerTwoShipsInitialPositions = new();

    private List<List<int>> playerOneShipIndex = new();
    private List<List<int>> playerTwoShipIndex = new();

    private ShipScript shipBrain;
    private int firstshipIndex = 0;
    private int secondshipIndex = 0;
    
    private List<GameObject> playerOneBlasts = new List<GameObject>();
    private List<GameObject> playerTwoblasts = new List<GameObject>();
    private List<int> destroyedPlayerOneShipsIndex = new List<int>();
    private List<int> destroyedPlayerTwoShipsIndex = new List<int>();
    public List<TileScript> oneTiles;
    public List<TileScript> twoTiles;
    int playerOneShipCount = 5;
    int playerTwoShipCount = 5;
    bool firstSettedUp = false;
    bool secondSettedUp = false;
    bool playerOneTurn = true;

    [Header("Colors")]
    public Color alreadyMissed;
    public Color alreadyHit;
    public Color alreadySank;

    [Header("Text Units")]
    public TextMeshProUGUI message;
    public TextMeshProUGUI playerOneScore;
    public TextMeshProUGUI playerTwoScore;

    public EnemyScript automation;
    public AnimationManager animationManager;


    string firstPlayerName, secondPlayerName;
    public TextMeshProUGUI currentTurnText, winnerText, nameInpuText;

    [Header("Animation Components")]
    public GameObject transition, nameInput, gameOver, warningText;

    private void Start()
    {
        shipBrain = playerOneShips[firstshipIndex].GetComponent<ShipScript>();
        for (int i = 0; i < playerOneShips.Length; i++)
        {
            playerOneShipsInitialPositions.Add(playerOneShips[i].transform.position);
            playerTwoShipsInitialPositions.Add(playerTwoShips[i].transform.position);
        }
        animationManager.FadeIn(nameInput,0.5f);
    }


    public void TileTouched(GameObject tile)
    {
        if (firstSettedUp && secondSettedUp && playerOneTurn)
        {
            //Debug.Log("Player Turn");
            Vector3 tilePos = tile.transform.position;
            //playerOneTurn = false;
            Instantiate(playerOneMissle, tilePos, playerOneMissle.transform.rotation);
        }
        else if(firstSettedUp && secondSettedUp && !playerOneTurn)
        {
            Vector3 tilePos = tile.transform.position;
            //playerOneTurn = true;
            Instantiate(playerTwoMissle, tilePos, playerTwoMissle.transform.rotation);
        }

        else if (firstSettedUp && !secondSettedUp)
        {
            PositionTwoShips(tile);
            shipBrain.SetTile(tile);
        }
        else if (!firstSettedUp)
        {
            PositionOneShips(tile);
            shipBrain.SetTile(tile);
        }
    }

    private void PositionOneShips(GameObject tile)
    {
        shipBrain = playerOneShips[firstshipIndex].GetComponent<ShipScript>();
        shipBrain.ClearTileList();
        Vector3 newVec = shipBrain.GetOffset(tile.transform.position);
        playerOneShips[firstshipIndex].transform.localPosition = newVec;
    }

    private void PositionTwoShips(GameObject tile)
    {
        shipBrain = playerTwoShips[secondshipIndex].GetComponent<ShipScript>();
        shipBrain.ClearTileList();
        Vector3 newVec = shipBrain.GetOffset(tile.transform.position);
        playerTwoShips[secondshipIndex].transform.localPosition = newVec;
    }

    public void NextShip()
    {
        if (!shipBrain.OnGameBoard())
        {
            shipBrain.FlashColor(Color.red);
        }
        else if(!firstSettedUp && !secondSettedUp)
        {
            Debug.Log("First Player Setting UP!!");
            if(playerOneShipIndex.Count < 5)
                playerOneShipIndex.Add(shipBrain.OccupiedTiles());
            if (firstshipIndex <= playerOneShips.Length - 2)
            {
                firstshipIndex++;
                shipBrain = playerOneShips[firstshipIndex].GetComponent<ShipScript>();
                shipBrain.FlashColor(Color.yellow);
            }
            else
            {
                Debug.Log("First PLayer Set Up Done!!");
                Debug.Log(secondshipIndex + "" + playerOneShipIndex.Count);
                // restrcit btn acces
                // TO DO: Open the Transition Panel and Enable Player Two Tiles and Docks
                message.text = "{Player Two Name} Turn";
                shipBrain = playerTwoShips[secondshipIndex].GetComponent<ShipScript>();
                // Turn the table 
                TurnTable(false);
                firstSettedUp = true;
                for (int i = 0; i < playerOneShips.Length; i++) { playerOneShips[i].SetActive(false); }
                animationManager.FadeIn(nameInput, 0.5f);
                // hide both's ships and open player one tiles
                Debug.Log("Player One Ship Locations");
                PrintShipIndexes(playerOneShipIndex);
            }
        }
        else if (firstSettedUp && !secondSettedUp)
        {
            Debug.Log("Second PLAyer Set Up!!");
            if(playerTwoShipIndex.Count < 5)
                playerTwoShipIndex.Add(shipBrain.OccupiedTiles());
            if (secondshipIndex <= playerTwoShips.Length - 2)
            {
                secondshipIndex++;
                shipBrain = playerTwoShips[secondshipIndex].GetComponent<ShipScript>();
                shipBrain.FlashColor(Color.yellow);
            }
            else
            {
                Debug.Log("Second PLayer Set Up done");
                message.text = "{Player One name Choose a Tile}";
                secondSettedUp = true;
                for (int i = 0; i < playerTwoShips.Length; i++) { playerTwoShips[i].SetActive(false); }
                currentTurnText.text = firstPlayerName;
                animationManager.FadeInAndOut(transition, 0.5f, 0.5f);
                Debug.Log("PLayer Two Ship Locations");
                PrintShipIndexes(playerTwoShipIndex);
                //TurnTable(true);
            }
        }
    }

    public void SuffleShips()
    {
        if(!firstSettedUp && !secondSettedUp)
        {
            List<int[]> tempShips = automation.PlaceEnemyScript();
            automation.PlaceShipAutomatic(tempShips, playerOneShips, oneTiles);
            playerOneShipIndex.Clear();
            foreach (int[] array in tempShips)
            {
                playerOneShipIndex.Add(new List<int>(array));
            }
            Debug.Log(playerOneShipIndex.Count);
            firstshipIndex = 5;
        }
        else if(!secondSettedUp)
        {
            List<int[]> tempShips = automation.PlaceEnemyScript();
            automation.PlaceShipAutomatic(tempShips, playerTwoShips, twoTiles);
            playerTwoShipIndex.Clear();
            foreach (int[] array in tempShips)
            {
                playerTwoShipIndex.Add(new List<int>(array));
            }
            Debug.Log(playerTwoShipIndex.Count);
            secondshipIndex = 5;
        }
    }

    public void RotatePressed()
    {
        shipBrain.Rotate();
    }

    public void CheckHit(GameObject tile)
    {
        if (playerOneTurn)
        {
            FirstPlayerCheckHit(tile);
            playerOneTurn = false;
        }
        else
        {
            SecondPlayerCheckHit(tile);
            playerOneTurn = true;
        }
    }

    public void FirstPlayerCheckHit(GameObject tile)
    {
        Debug.Log("First Player Hit Check");
        int tileNo = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;
        int shipNo = 0;
        foreach (List<int> tileNumArray in playerTwoShipIndex)
        {
            if (tileNumArray.Contains(tileNo))
            {
                for (int i = 0; i < tileNumArray.Count; i++)
                {
                    if (tileNumArray[i] == tileNo)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileNumArray.Count)
                {
                    playerTwoShipCount--;
                    //Debug.Log("Sunk. Enemy Ships Remains -> " + enemyShipCount);
                    message.text = "Ship Destroyed!";
                    playerTwoblasts.Add(Instantiate(blast, tile.transform.position, Quaternion.identity));
                    destroyedPlayerTwoShipsIndex.Add(shipNo);
                    playerTwoShips[shipNo].SetActive(true);
                    tile.GetComponent<TileScript>().SetTileColor(1, alreadySank);
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    // hit
                    //Debug.Log("Hit");
                    message.text = "Ship Hit!";
                    playerTwoblasts.Add(Instantiate(blast, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, alreadyHit);
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                break;
            }
            shipNo++;

        }
        if (hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, alreadyMissed);
            tile.GetComponent<TileScript>().SwitchColors(1);
            //Debug.Log("Missed");
            message.text = "No Ship Found!";

        }
        Invoke("PlayerOneTurnEnds", 2f);
        //PlayerTurnEnds();
    }


    public void SecondPlayerCheckHit(GameObject tile)
    {
        Debug.Log("Second Player Hit Check");
        int tileNo = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;
        int shipNo = 0;
        foreach (List<int> tileNumArray in playerOneShipIndex)
        {
            if (tileNumArray.Contains(tileNo))
            {
                for (int i = 0; i < tileNumArray.Count; i++)
                {
                    if (tileNumArray[i] == tileNo)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileNumArray.Count)
                {
                    playerOneShipCount--;
                    //Debug.Log("Sunk. Enemy Ships Remains -> " + enemyShipCount);
                    message.text = "Ship Destroyed!";
                    playerOneBlasts.Add(Instantiate(blast, tile.transform.position, Quaternion.identity));
                    destroyedPlayerOneShipsIndex.Add(shipNo);
                    playerOneShips[shipNo].SetActive(true);
                    tile.GetComponent<TileScript>().SetTileColor(1, alreadySank);
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    // hit
                    //Debug.Log("Hit");
                    message.text = "Ship Hit!";
                    playerOneBlasts.Add(Instantiate(blast, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, alreadyHit);
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                break;
            }
            shipNo++;

        }
        if (hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, alreadyMissed);
            tile.GetComponent<TileScript>().SwitchColors(1);
            //Debug.Log("Missed");
            message.text = "No Ship Found!";

        }
        Invoke("PlayerTwoTurnEnds", 2f);
        //PlayerTurnEnds();
    }


    void PlayerOneTurnEnds()
    {
        //Debug.Log("Player Turn Ends");
        //for (int i = 0; i < ships.Length; i++) ships[i].SetActive(true);

        currentTurnText.text = secondPlayerName;
        playerOneTurn = false;
        if (playerTwoShipCount < 1) GameOver(firstPlayerName);
        else
        {
            animationManager.FadeInAndOut(transition, 0.2f, 0.8f, () =>
            {
                PlayerTwoTurn();
            });
        }

        foreach (GameObject blast in playerOneBlasts) blast.SetActive(true);
        foreach (GameObject blast in playerTwoblasts) blast.SetActive(false);
        foreach (var index in destroyedPlayerTwoShipsIndex) playerTwoShips[index].SetActive(false);
        playerOneScore.text = playerOneShipCount.ToString();
        playerTwoScore.text = playerTwoShipCount.ToString();
        message.text = "Player Two's Turn";
        
        //enemyBrain.Bot();

    }

    void PlayerTwoTurnEnds()
    {
        //Debug.Log("Player Turn Ends");
        //for (int i = 0; i < ships.Length; i++) ships[i].SetActive(true);

        currentTurnText.text = firstPlayerName;
        playerOneTurn = true;
        if (playerOneShipCount < 1) GameOver(secondPlayerName);
        else
        {
            animationManager.FadeInAndOut(transition, 0.2f, 0.8f, () =>
            {
                PlayerOneTurn();
            });
        }


        foreach (GameObject blast in playerTwoblasts) blast.SetActive(true);
        foreach (GameObject blast in playerOneBlasts) blast.SetActive(false);
        foreach (var index in destroyedPlayerOneShipsIndex) playerOneShips[index].SetActive(false);
        playerTwoScore.text = playerTwoShipCount.ToString();
        playerOneScore.text = playerOneShipCount.ToString();
        message.text = "Player One's Turn";
        
        //enemyBrain.Bot();

    }




    void GameOver(string winner)
    {
        //Debug.Log(message);
        message.text = winner + "Wins!!";
        winnerText.text = winner;
        playerOneTurn = false;

        animationManager.FadeIn(gameOver, 0.5f);

        // enable gameoverpanel


    }
    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void PlayerOneTurn()
    {
        Debug.Log("Player One Turn");
        TurnTable(false);
    }

    private void PlayerTwoTurn()
    {
        Debug.Log("Player Two Turn");
        TurnTable(true);
    }

    void TurnTable(bool playerOne)
    {
        playerOneBoard.SetActive(playerOne);
        playerTwoBoard.SetActive(!playerOne);
    }

    public void ResetShips()
    {
        if(!firstSettedUp && !secondSettedUp)
        {
            for (int i = 0; i < playerOneShips.Length; i++)
            {
                if (playerOneShips[i] != null && playerOneShipsInitialPositions[i] != null)
                {
                    playerOneShips[i].transform.position = playerOneShipsInitialPositions[i];
                    playerOneShips[i].transform.rotation = Quaternion.identity; // Default rotation (0,0,0)
                    playerOneShips[i].GetComponent<ShipScript>().ClearTileList();
                }
            }
            firstshipIndex = 0;
        }
        else if (!secondSettedUp)
        {
            for (int i = 0; i < playerTwoShips.Length; i++)
            {
                if (playerTwoShips[i] != null && playerTwoShipsInitialPositions[i] != null)
                {
                    playerTwoShips[i].transform.position = playerTwoShipsInitialPositions[i];
                    playerTwoShips[i].transform.rotation = Quaternion.identity; // Default rotation (0,0,0)
                    playerTwoShips[i].GetComponent<ShipScript>().ClearTileList();
                }
            }
            secondshipIndex = 0;
        }
    }

    public void SetPlayerName(TMP_InputField input)
    {
        if (!string.IsNullOrWhiteSpace(input.text))
        {
            if(!firstSettedUp && !secondSettedUp)
            {
                firstPlayerName = input.text;
                animationManager.FadeOut(nameInput, 0.5f);
                input.text = string.Empty;
                nameInpuText.text = "Player Two";
            }
            else if (!secondSettedUp)
            {
                secondPlayerName = input.text;
                animationManager.FadeOut(nameInput, 0.5f);
            }
        }
        else
        {
            animationManager.FadeIn(warningText,0.1f);
        }
    }

    void PrintShipIndexes(List<List<int>> playerOneShipIndex)
    {
        foreach (var innerList in playerOneShipIndex)
        {
            Debug.Log(string.Join(" ", innerList));
        }
    }

}
