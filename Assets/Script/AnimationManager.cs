using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

public class AnimationManager : MonoBehaviour
{

    public void FadeIn(GameObject obj, float duration)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning($"FadeIn: No CanvasGroup found on {obj.name}");
            return;
        }

        obj.SetActive(true); // Activate the object before fading in
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, duration);
    }

    public void FadeOut(GameObject obj, float duration)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning($"FadeOut: No CanvasGroup found on {obj.name}");
            return;
        }

        canvasGroup.DOFade(0, duration).OnComplete(() => obj.SetActive(false));
    }

    public void FadeInAndOut(GameObject obj, float fadeDuration, float waitTime, Action onComplete = null)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning($"FadeInAndOut: No CanvasGroup found on {obj.name}");
            return;
        }

        obj.SetActive(true);
        canvasGroup.alpha = 0;

        // Fade in
        canvasGroup.DOFade(1, fadeDuration).OnComplete(() =>
        {
            // Wait for a few seconds, then fade out
            StartCoroutine(WaitAndFadeOut(obj, canvasGroup, fadeDuration, waitTime, onComplete));
        });
    }

    private IEnumerator WaitAndFadeOut(GameObject obj, CanvasGroup canvasGroup, float fadeDuration, float waitTime, Action onComplete)
    {
        yield return new WaitForSeconds(waitTime);
        canvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            obj.SetActive(false);
            onComplete?.Invoke(); // Invoke the callback if it's provided
        });
    }


}
