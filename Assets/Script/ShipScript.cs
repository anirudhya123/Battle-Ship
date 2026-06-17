using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ShipScript : MonoBehaviour
{
    [NonSerialized] public List<GameObject> touchedTiles = new List<GameObject>();
    public float xOffset = 0f;
    public float yoffset = 0f;
    float rotationAmount = 90f;
    GameObject clickedTile;
    int hitCount = 0;
    public int shipSize;
    bool occupied = false;
    [NonSerialized] public bool sanked = false;


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ship"))
        {
            //Debug.Log("Already Occupied");
            occupied = true;
        }
        else
        {
            occupied = false;
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {

       if(!occupied)
        {
            if (collision.gameObject.CompareTag("Tile"))
            {
                //Debug.Log(collision.gameObject);
                touchedTiles.Add(collision.gameObject);
            }
        
            touchedTiles = touchedTiles.Distinct().ToList();
        }
        else
        {
            ClearTileList();
        }

    }

    public void ClearTileList()
    {
        touchedTiles.Clear();
    }

    public Vector3 GetOffset(Vector3 tilePos)
    {
        occupied = false;
        return new Vector3(tilePos.x + xOffset, tilePos.y + yoffset, 0);
    }

    public void Rotate()
    {
        if (clickedTile == null) return;

        touchedTiles.Clear();
        transform.localEulerAngles += new Vector3(0, 0, rotationAmount);
        rotationAmount *= -1;
        float temp = xOffset;
        xOffset = yoffset;
        yoffset = temp;
        SetPosition(clickedTile.transform.position);
    }

    public void SetPosition(Vector3 vect)
    {
        ClearTileList();
        transform.localPosition = new Vector3(vect.x+xOffset,vect.y+yoffset, 0);
        occupied = false;
        //Debug.Log("Let's Begin the Play");
        
    }

    public void SetTile(GameObject tile)
    {
        clickedTile = tile;
    }

    public bool OnGameBoard()
    {
        //Debug.Log(touchedTiles.Count + "<- Touched Tiles | Ship Size ->" + shipSize);
        Debug.Log(touchedTiles.Count);
        return touchedTiles.Count == shipSize;
    }
    public bool Sanked()
    {
        hitCount++;
        sanked = shipSize <= hitCount;
        return sanked;
    }

    public void FlashColor(Color32 color) {
        GetComponent<SpriteRenderer>().color = color;
        Invoke("ResetColor", 0.5f);
    }

    private void ResetColor()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void Checking()
    {
        foreach (GameObject item in touchedTiles)
        {
            Debug.Log(Int32.Parse(Regex.Match(item.name, @"\d+").Value));
        }
    }

    public List<int> OccupiedTiles()
    {
        List<int> tiles = new();
        foreach (GameObject item in touchedTiles)
        {
            tiles.Add(Int32.Parse(Regex.Match(item.name, @"\d+").Value));
            //Debug.Log(Int32.Parse(Regex.Match(item.name, @"\d+").Value));
        }
        Debug.Log(tiles);
        return tiles;
    }
}
