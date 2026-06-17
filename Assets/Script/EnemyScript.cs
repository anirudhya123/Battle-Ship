using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    char[] predictedGrid;
    List<int> potentialHits;
    List<int> currentHits;
    int guess;
    public GameObject enemyMissile;
    public GameManager gameManager;
    public GameObject[] enemyShipsprefab;
    [NonSerialized] public List<GameObject> enemyShips = new List<GameObject>();
    private void Start()
    {
        //gameManager = FindObjectOfType<GameManager>();
        potentialHits = new List<int>();
        currentHits = new List<int>();
        predictedGrid = Enumerable.Repeat('O',100).ToArray();
    }

    public List<int[]> PlaceEnemyScript()
    {
        List<int[]> enemyShips = new List<int[]>
        {
            new int[] { -1, -1, -1, -1, -1 },
            new int[] { -1, -1, -1, -1 },
            new int[] { -1, -1, -1 },
            new int[] { -1, -1, -1 },
            new int[] { -1, -1 }
        };
        int[] gridNumbers = Enumerable.Range(0,100).ToArray();
        bool occupied = true;

        foreach (int[] tileNumArray in enemyShips)
        {
            occupied = true;
            while (occupied)
            {

                occupied = false;
                int shipPNo = UnityEngine.Random.Range(0, 99);
                int rotationCheck = UnityEngine.Random.Range(0, 2);
                int negativeValue = rotationCheck == 0 ? 10 : 1;
                for (int i = 0; i < tileNumArray.Length; i++)
                {

                    // checking ship's end will not go off the baord and tile is already occupied
                    if ((shipPNo - (negativeValue * i)) < 0 || gridNumbers[shipPNo - i * negativeValue] < 0)
                    {
                        occupied = true;
                        break;
                    }
                    // Ship is horizontal check ship doesn't go off the sides 0 to 11, 11 to 20
                    else if (negativeValue == 1 && shipPNo / 10 != ((shipPNo - i * negativeValue) - 1) / 10)
                    {
                        occupied = true;
                        break;
                    }
                }
                // if tile is not occupied loop through tile numbers assign them to the array in the list
                if (!occupied)
                {
                    for (int j = 0; j < tileNumArray.Length; j++)
                    {
                        tileNumArray[j] = gridNumbers[shipPNo - j * negativeValue];
                        gridNumbers[shipPNo - j * negativeValue] = -1;

                    }
                }
            }
        }

        // testing return 

        foreach (int[] ships in enemyShips)
        {
            string temp = "";
            for (int i = 0; i < ships.Length; i++)
            {

                temp += ", " + ships[i];
            }
            Debug.Log(temp);
        }


        return enemyShips;
        
    }

    public void Bot()
    {
        List<int> hitIndex = new List<int>();
        for(int i = 0;i < predictedGrid.Length;i++)
        {
            if (predictedGrid[i] == 'X') hitIndex.Add(i);
        }
        //Debug.Log("Hitted Boxes Length -> " + hitIndex.Count());
        if(hitIndex.Count > 1) {
            //Debug.Log("Hit Index Count is Greater than 1");
            int diff = hitIndex[1] - hitIndex[0];
            int coeff = UnityEngine.Random.Range(0,2) * 2 - 1 ;
            int nextIndex = hitIndex[0] + diff;
            bool verified = nextIndex > -1 && nextIndex < 100;
            while (!Verify(nextIndex,'O'))
            {
                if (!verified)
                {
                    diff *= -1;
                }
                else if (predictedGrid[nextIndex] == '-')
                {
                    diff *= -1;
                }
                nextIndex += diff;
                verified = nextIndex > -1 && nextIndex < 100;
            }
            guess = nextIndex;
        }
        else if(hitIndex.Count == 1)
        {
            //Debug.Log("Hit Index Count is same with 1");
            List<int> closedTiles = new List<int>() { 1 , -1, 10, -10};
            int index = UnityEngine.Random.Range(0, closedTiles.Count);
            int choice = hitIndex[0] + closedTiles[index];
            bool verified = choice > -1 && choice < 100;
            while((!verified || predictedGrid[choice] != 'O') && closedTiles.Count > 0) { 
                closedTiles.RemoveAt(index);
                index = UnityEngine.Random.Range(0, closedTiles.Count);
                choice = hitIndex[0] + closedTiles[index];
                verified = choice > -1 && choice < 100; 
            }
            guess = choice;
        }
        else
        {
            //Debug.Log("Random Tile Choosing");
            int nextIndex = UnityEngine.Random.Range(0, 100);

            // Hardness Definer
            for (int i = 0; i < gameManager.hardness; i++)
            {
                nextIndex = HardGuess(nextIndex);
                //Debug.Log("Hardness Check Lvl" + i);
            }

            while (predictedGrid[nextIndex] != 'O') nextIndex = UnityEngine.Random.Range(0, 100);
            guess = nextIndex;
        }
        Transform tile = GameObject.Find("Tile Holder").transform.GetChild(guess);
        //Debug.Log("Final Choosed tile Number is ->" + guess + tile.name);
        predictedGrid[guess] = '-';
        Vector3 vect = tile.position;
        GameObject missile = Instantiate(enemyMissile, vect, enemyMissile.transform.rotation);
        missile.GetComponent<EnemyMissileBrain>().SetTarget(guess);
        missile.GetComponent<EnemyMissileBrain>().targetTileLocation = tile.position;
    }

    public void MissileHit(int hit)
    {
        //Debug.Log(hit + "<->" + guess);
        predictedGrid[hit] = 'X';
        Invoke("EndTurn", 2.0f);
    }

    public void SunkPlayer(GameObject ship)
    {

        List<int> shipIndexes = new List<int>();
        foreach(GameObject tile in ship.GetComponent<ShipScript>().touchedTiles)
        {
            shipIndexes.Add(Int32.Parse(Regex.Match(tile.name, @"\d+").Value));
        }


        for (int i = 0; i < predictedGrid.Length; i++)
        {
            if (predictedGrid[i] == 'X' && shipIndexes.Contains(i)) predictedGrid[i] = '_';
        }
    }

    private void EndTurn()
    {
        gameManager.BotTurnEnds();
    }

    public void PauseAndEnd(int miss)
    {
        gameManager.message.text = "Bot Missed";
        if(currentHits.Count > 0 && currentHits[0] > miss) {
            foreach(int potential in potentialHits) {
                if (currentHits[0] > miss)
                {
                    if(potential < miss) potentialHits.Remove(potential);
                }
                else
                {
                    if (potential > miss) potentialHits.Remove(potential);
                }
            }
        }
        Invoke("EndTurn", 2.0f);
    }

    private bool Verify(int index, char symbol)
    {
        if(index > -1 && index < 100)
        {
            if (predictedGrid[index] == symbol)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else return false; 
    }

    private int HardGuess(int index) {
        int nextIndex = index;
        bool edging = nextIndex < 10 || nextIndex > 89 || nextIndex % 10 == 0 || nextIndex % 10 == 9;
        bool check = false;
        if (nextIndex + 1 < 100) check = predictedGrid[nextIndex + 1] != 'O';
        if (!check && nextIndex - 1 > -1) check = predictedGrid[nextIndex - 1] != 'O';
        if (!check && nextIndex + 10 < 100) check = predictedGrid[nextIndex + 10] != 'O';
        if (!check && nextIndex - 10 > -1) check = predictedGrid[nextIndex - 10] != 'O';

        if (edging || check) nextIndex = UnityEngine.Random.Range(0, 100);
        return nextIndex;


    }

    public void PlaceShipPrefabs(List<int[]> occupiedTiles)
    {
        int shipIndex = 0;
        foreach (int[] tile in occupiedTiles)
        {
            float rotationAmount = MathF.Abs(tile[1] - tile[0]) < 10 ? 90 : 0;
            int middleTile = tile.Length%10 == 0 ? tile.Length/2 - 1 : tile.Length/2 ;

            GameObject ship = Instantiate(enemyShipsprefab[shipIndex]);
            float xOff = ship.GetComponent<EnemyShipScript>().offset.x;
            float yOff = ship.GetComponent<EnemyShipScript>().offset.y;
            if(rotationAmount > 0) {
                xOff = ship.GetComponent<EnemyShipScript>().offset.y;
                yOff = ship.GetComponent<EnemyShipScript>().offset.x;
            }
            Vector3 tilePos = gameManager.allTiles[tile[middleTile]].transform.position;
            ship.transform.position = new Vector3(tilePos.x + xOff , tilePos.y + yOff, tilePos.z);

            ship.transform.localEulerAngles = new Vector3(0, 0, rotationAmount);
            enemyShips.Add(ship);
            //Debug.Log(tile.Length +  "<-->" + MathF.Abs(tile[0] - tile[1]) + "<-->" + gameManager.allTiles[tile[0]].transform.position);
            shipIndex++;
        }
    }

    public void PlaceShipAutomatic(List<int[]> occupiedTiles, GameObject[] ships, List<TileScript> allTiles)
    {
        int shipIndex = 0;
        foreach (int[] tile in occupiedTiles)
        {
            float rotationAmount = MathF.Abs(tile[1] - tile[0]) < 10 ? 90 : 0;
            int middleTile = tile.Length % 10 == 0 ? tile.Length / 2 - 1 : tile.Length / 2;

            GameObject ship = ships[shipIndex];
            ship.GetComponent<ShipScript>().ClearTileList();
            float xOff = ship.GetComponent<ShipScript>().xOffset;
            float yOff = ship.GetComponent<ShipScript>().yoffset;
            if (rotationAmount > 0)
            {
                xOff = ship.GetComponent<ShipScript>().yoffset;
                yOff = ship.GetComponent<ShipScript>().xOffset;
            }
            Debug.Log(tile[middleTile]);
            Vector3 tilePos = allTiles[tile[middleTile]].transform.position;
            ship.transform.position = new Vector3(tilePos.x + xOff, tilePos.y + yOff, tilePos.z);

            ship.transform.localEulerAngles = new Vector3(0, 0, rotationAmount);
            //Debug.Log(tile.Length +  "<-->" + MathF.Abs(tile[0] - tile[1]) + "<-->" + gameManager.allTiles[tile[0]].transform.position);
            shipIndex++;
        }
    }
}
