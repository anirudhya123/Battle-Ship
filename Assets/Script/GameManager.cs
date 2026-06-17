using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject missle;
    public GameObject enemyMissile;
    public GameObject blast;

    public GameObject[] ships;
    List<Vector3> shipsInitialPositions = new();
    private ShipScript shipBrain;
    private int shipIndex = 0;
    public EnemyScript enemyBrain;
    private List<int[]> enemyShips;
    private List<GameObject> blasts = new List<GameObject>();
    private List<GameObject> Botblasts = new List<GameObject>();
    private List<int> destroyedEnemyShipsIndex = new List<int>();   
    public List<TileScript> allTiles;
    int shipCount = 5;
    int enemyShipCount = 5;
    bool settedUp = false;
    bool playerTurn = true;

    [Header("Colors")]
    public Color alreadyMissed;
    public Color alreadyHit;
    public Color alreadySank;

    [Header("Text Units")]
    public TextMeshProUGUI message;
    public TextMeshProUGUI playerScore;
    public TextMeshProUGUI enemyScore;

    public int hardness = 1;

    private void Start()
    {
        shipBrain = ships[shipIndex].GetComponent<ShipScript>();
        for (int i = 0; i < ships.Length; i++)
        {
            shipsInitialPositions.Add(ships[i].transform.position);
        }
    }

    
    public void TileTouched(GameObject tile)
    {
        if(settedUp && playerTurn)
        {
            //Debug.Log("Player Turn");
            Vector3 tilePos = tile.transform.position;
            playerTurn = false;
            Instantiate(missle, tilePos, missle.transform.rotation);
        }
        else if (!settedUp)
        {
             PositionShips(tile);
             shipBrain.SetTile(tile);
            
        }
    }

    private void PositionShips(GameObject tile)
    {
        shipBrain = ships[shipIndex].GetComponent<ShipScript>();
        shipBrain.ClearTileList();
        Vector3 newVec = shipBrain.GetOffset(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;  
    }

    public void NextShip()
    {
        if(!shipBrain.OnGameBoard())
        {
            shipBrain.FlashColor(Color.red);
        }
        else
        {
            if(shipIndex <= ships.Length - 2)
            {
                shipBrain.Checking();
                shipIndex++;
                shipBrain = ships[shipIndex].GetComponent<ShipScript>();
                shipBrain.FlashColor(Color.yellow);
            }
            else
            {
                // restrcit btn acces
                message.text = "Choose a Tile!";
                settedUp = true;
                enemyShips = enemyBrain.PlaceEnemyScript();
                enemyBrain.PlaceShipPrefabs(enemyShips);
                foreach (var ship in enemyBrain.enemyShips){ ship.SetActive(false);}
                for (int i = 0; i < ships.Length; i++) { ships[i].SetActive(false); }
            }            
        }
    }

    public void SuffleShips()
    {
        List<int[]> tempShips = enemyBrain.PlaceEnemyScript();
        enemyBrain.PlaceShipAutomatic(tempShips, ships, allTiles);
        shipIndex = 5;
    }

    public void ResetShips()
    {
            for (int i = 0; i < ships.Length; i++)
            {
                if (ships[i] != null && shipsInitialPositions[i] != null)
                {
                    ships[i].transform.position = shipsInitialPositions[i];
                    ships[i].transform.rotation = Quaternion.identity; // Default rotation (0,0,0)
                    ships[i].GetComponent<ShipScript>().ClearTileList();
                }
            }
            shipIndex = 0;
    }

    public void RotatePressed()
    {
        shipBrain.Rotate();
    }

    public void CheckHit(GameObject tile)
    {
        int tileNo = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;
        int shipNo = 0;
        foreach (int[] tileNumArray in enemyShips)
        {
            if(tileNumArray.Contains(tileNo))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
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
                if(hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    //Debug.Log("Sunk. Enemy Ships Remains -> " + enemyShipCount);
                    message.text = "Ship Destroyed!";
                    Botblasts.Add(Instantiate(blast, tile.transform.position, Quaternion.identity));
                    destroyedEnemyShipsIndex.Add(shipNo);
                    enemyBrain.enemyShips[shipNo].SetActive(true);
                    tile.GetComponent<TileScript>().SetTileColor(1, alreadySank);
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    // hit
                    //Debug.Log("Hit");
                    message.text = "Ship Hit!";
                    Botblasts.Add(Instantiate(blast, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, alreadyHit);
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                break;
            }
            shipNo++;

        }
        if(hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, alreadyMissed);
            tile.GetComponent<TileScript>().SwitchColors(1);
            //Debug.Log("Missed");
            message.text = "No Ship Found!";

        }
        Invoke("PlayerTurnEnds", 2f);
        //PlayerTurnEnds();
    }

    public void EnemyHits(Vector3 tilePos, int tileNum, GameObject hit)
    {
        enemyBrain.MissileHit(tileNum);
        blasts.Add(Instantiate(blast,tilePos,Quaternion.identity));
        if(hit.GetComponent<ShipScript>().Sanked()) {
            shipCount--;
            message.text = "Ship Destroyed";
            playerScore.text = shipCount.ToString();
            //Debug.Log("Sunked Player ship Remains ->" +  shipCount);
            enemyBrain.SunkPlayer(hit);
        }
        else   message.text = "Robot Hits";

        Invoke("BotTurnEnds", 2.0f);
        
    }

    void PlayerTurnEnds()
    {
        //Debug.Log("Player Turn Ends");
        for(int i = 0;i < ships.Length;i++) ships[i].SetActive(true);
        foreach (GameObject blast in blasts) blast.SetActive(true); 
        foreach (GameObject blast in Botblasts) blast.SetActive(false);
        foreach (var index in destroyedEnemyShipsIndex) enemyBrain.enemyShips[index].SetActive(false);
        enemyScore.text = enemyShipCount.ToString();
        message.text = "Bot's Turn";
        playerTurn = false;
        if (enemyShipCount < 1) GameOver("Player");
        else 
        { 
            ColorTiles(0); 
            Invoke("BotTurn", 2f); 
        }
        //enemyBrain.Bot();
        
    }
    public void BotTurnEnds()
    {
        //Debug.Log("Bot Turn Ends");
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
        foreach (GameObject blast in blasts) blast.SetActive(false);
        foreach (GameObject blast in Botblasts) blast.SetActive(true);
        foreach (var index in destroyedEnemyShipsIndex) enemyBrain.enemyShips[index].SetActive(true);
        playerScore.text = shipCount.ToString();
        message.text = "Choose a tile!";
        if (shipCount < 1) GameOver("Bot");
        else
        {
            playerTurn = true;
            ColorTiles(1);

        }
    }

    void ColorTiles(int colorIndex)
    {
        foreach(TileScript tilescript in allTiles)
        {
            tilescript.SwitchColors(colorIndex);
        }
    }

    void GameOver(string winner)
    {
        //Debug.Log(message);
        message.text = winner + "Wins!!";
        playerTurn = false;
        Invoke("Reset", 10f);


    }
    private void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void BotTurn()
    {
        enemyBrain.Bot();
    }


}
