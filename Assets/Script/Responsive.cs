using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Responsive : MonoBehaviour
{
    [Header("Initial Detailing")]
    [SerializeField] int row;
    [SerializeField] int column;
    [SerializeField] float space;
    [SerializeField] float scale;
    [SerializeField] Vector3 firstVect;
    [SerializeField] Canvas canvas;
    [SerializeField] float referenceHeight;
    [SerializeField] float referenceWidth;

    float canvasHeight;
    float canvasWidth;

    private void Start()
    {
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;

        Debug.Log(canvasHeight + "<<->>" + canvasWidth);
        firstVect.x = ReCalculate(referenceWidth, canvasWidth, firstVect.x);
        firstVect.y = ReCalculate(referenceHeight, canvasHeight, firstVect.y);
        scale = ReCalculate(referenceWidth, canvasWidth, scale);
        space = ReCalculate(referenceWidth, canvasWidth, space);

        ReArrange();

    }

    void ReArrange()
    {
        for (int i = 0; i < row; i++)
        {
            for(int j = 0; j < column; j++)
            {
                Vector3 updatedPostion = new(firstVect.x + (space * j), firstVect.y + (space * i), firstVect.z);
                int childIndex = (i * 10) + j;
                Transform child = transform.GetChild(childIndex);
                //Debug.Log(child.name);
                child.localScale = new Vector3(scale, scale, scale);
                child.localPosition = updatedPostion;
            }
        }

    }


    float ReCalculate(float prev, float pres, float val)
    {
        float updatedVal = (val / prev) * pres;

        return updatedVal;
    }

}
