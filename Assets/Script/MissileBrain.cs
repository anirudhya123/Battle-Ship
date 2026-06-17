using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileBrain : MonoBehaviour
{
    private GameManager gameManager;
    private MultiPlayerManager multiPlayerManager;
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        multiPlayerManager = FindObjectOfType<MultiPlayerManager>();
    }
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Debug.Log("Collided"+ collision.gameObject);
    //    gameManager.CheckHit(collision.gameObject);
    //    Destroy(gameObject);
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("My Triggered" + collision.gameObject);

        if (gameManager != null)
            gameManager.CheckHit(collision.gameObject);
        else if (multiPlayerManager != null)
            multiPlayerManager.CheckHit(collision.gameObject);
            
        
        Destroy(gameObject);
    }
}
