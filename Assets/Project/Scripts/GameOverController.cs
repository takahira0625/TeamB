using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    private bool isGameOver = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        StartCoroutine(GameOverRoutine());
    }

    // Update is called once per frame
    private IEnumerator GameOverRoutine()
    {
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1;
        SceneManager.LoadScene("GameOverScene");
    }
}
