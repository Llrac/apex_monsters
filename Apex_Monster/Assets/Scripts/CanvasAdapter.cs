using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasAdapter : MonoBehaviour
{
    public Vector2 startPosition;
    Vector2 originalPosition;
    Vector2 originalSize;
    Vector2 lastResolution;
    Vector2 newResolutionRatio;

    RectTransform rectTransform;

    private void Awake()
    {
        lastResolution = new Vector2(Screen.width, Screen.height);
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
    }

    public void UpdateThisObjectsResolution(Vector2 newResolution)
    {
        originalPosition = startPosition;
        originalSize = Vector2.one;
        newResolutionRatio.x = newResolution.x / lastResolution.x;
        newResolutionRatio.y = newResolution.y / lastResolution.y;
        originalSize.x *= newResolutionRatio.x;
        originalSize.y *= newResolutionRatio.y;
        originalPosition.x *= newResolutionRatio.x;
        originalPosition.y *= newResolutionRatio.y;

        rectTransform.position = new Vector2(originalPosition.x, originalPosition.y);
        rectTransform.localScale = new Vector2(originalSize.x, originalSize.y);
        Debug.Log("hello I've updated the new resolution!");
    }
}
