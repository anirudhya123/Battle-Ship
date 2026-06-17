using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyMissileBrain : MonoBehaviour
{
    GameManager gameManager;
    EnemyScript enemy;
    public Vector2 targetTileLocation;
    private int targetTile = -1;
    bool shipped = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        enemy = FindObjectOfType<EnemyScript>();
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ship"))
    //    {
    //        gameManager.EnemyHits(targetTileLocation,targetTile,collision.gameObject);
    //    }
    //    else
    //    {
    //        enemy.PauseAndEnd(targetTile);
    //    }
    //    Destroy(gameObject);
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Enemy Missile Triggering on -> " + collision.gameObject.name );
        if (collision.gameObject.CompareTag("Ship"))
        {
            gameManager.EnemyHits(targetTileLocation, targetTile, collision.gameObject);
            shipped = true;
        }
        else if(!shipped)
        {
            enemy.PauseAndEnd(targetTile);
        }
        Destroy(gameObject);
    }

    public void SetTarget(int target)
    {
        targetTile = target;    
    }
}
