using UnityEngine;

public class TitleUIController : MonoBehaviour
{

    public GameObject difficultyPopup;
    public GameObject howToPlayPopup;

    // ゲーム開始時にすべてのポップアップを非表示にする
    void Start()
    {
        CloseAllPopups();
    }

    // メインゲームボタンが押されたら呼び出す関数
    public void OpenDifficultyPopup()
    {
        // 難易度選択を表示して、操作説明は非表示にする
        if (difficultyPopup != null) difficultyPopup.SetActive(true);
        if (howToPlayPopup != null) howToPlayPopup.SetActive(false);
    }

    // 操作説明ボタンが押されたら呼び出す関数
    public void OpenHowToPlayPopup()
    {
        // 操作説明を表示して、難易度選択は非表示にする
        if (howToPlayPopup != null) howToPlayPopup.SetActive(true);
        if (difficultyPopup != null) difficultyPopup.SetActive(false);
    }

    // 各「閉じる」ボタンが押されたら呼び出す関数
    public void CloseAllPopups()
    {
        // 両方のポップアップを非表示にする
        if (difficultyPopup != null) difficultyPopup.SetActive(false);
        if (howToPlayPopup != null) howToPlayPopup.SetActive(false);
    }
}