using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Colors and Frequency")]
    public Color[] fireColors;
    public float colorChangeSpeed = 1.0f;

    private int currentColorIndex = 0;
    private float t = 0.0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (fireColors.Length > 0)
        {
            spriteRenderer.color = fireColors[currentColorIndex];
        }
    }

    void Update()
    {
        if (fireColors.Length > 0)
        {
            t += Time.deltaTime * colorChangeSpeed;
            if (t >= 1.0f)
            {
                t = 0.0f;
                currentColorIndex = (currentColorIndex + 1) % fireColors.Length;
                spriteRenderer.color = fireColors[currentColorIndex];
            }
            else
            {
                int nextColorIndex = (currentColorIndex + 1) % fireColors.Length;
                spriteRenderer.color = Color.Lerp(fireColors[currentColorIndex], fireColors[nextColorIndex], t);
            }
        }
    }
}
