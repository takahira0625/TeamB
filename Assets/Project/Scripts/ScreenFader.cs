using System;
using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private bool playFadeInOnStart = true;

    private void Start()
    {
        if (playFadeInOnStart)
        {
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / fadeTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeOut(Action onComplete)
    {
        StartCoroutine(FadeOutRoutine(onComplete));
    }

    private IEnumerator FadeOutRoutine(Action onComplete)
    {
        canvasGroup.blocksRaycasts = true;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / fadeTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        onComplete?.Invoke();
    }
}