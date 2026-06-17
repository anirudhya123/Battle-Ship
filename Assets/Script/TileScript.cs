using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileScript : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    bool caught = false;
    Color32[] colors = new Color32[2];

    
    GameManager gameManager;
    MultiPlayerManager multiPlayerManager;
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        multiPlayerManager = FindObjectOfType<MultiPlayerManager>();
        colors[0] = gameObject.GetComponent<SpriteRenderer>().color;
        colors[1] = gameObject.GetComponent<SpriteRenderer>().color;
    }

    private void Update()
    {
        Vector3 inputPos = Input.mousePosition;

        //  Check if Input.mousePosition is valid
        if (float.IsNaN(inputPos.x) || float.IsNaN(inputPos.y) || float.IsInfinity(inputPos.x) || float.IsInfinity(inputPos.y))
        {
            Debug.LogWarning("Skipping ScreenToWorldPoint due to invalid mouse position: " + inputPos);
            return;  // Skip the rest of the code
        }




        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if(hit.collider != null)
        {
            if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == gameObject.name)
            {
                if (!caught)
                {
                    if (gameManager != null)
                        gameManager.TileTouched(hit.collider.gameObject);
                    else if (multiPlayerManager != null)
                        multiPlayerManager.TileTouched(hit.collider.gameObject);

                }

            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Missile") ){
            caught = true;
        }
        else if (collision.gameObject.CompareTag("EnemyMissile"))
        {
            colors[0] = new Color32(139, 0, 255, 161);
            GetComponent<SpriteRenderer>().color = colors[0]; 
        }
    }

    public void SetTileColor(int index, Color32 color)
    {
        colors[index] = color;
    }
    public void SwitchColors(int colorIndex)
    {
        GetComponent<SpriteRenderer>().color = colors[colorIndex];
    }
}
