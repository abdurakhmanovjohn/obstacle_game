using UnityEngine;
using TMPro;

// TextMeshPro version of GameUI: drives the Canvas objects
// (HitsText, GameOverPanel, WatchAdButton, WinPanel) instead of OnGUI.
public class GameUI : MonoBehaviour
{
    [SerializeField] TMP_Text hitsText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject watchAdButton;
    [SerializeField] GameObject winPanel;

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        hitsText.text = "Hits: " + gm.Hits + " / " + gm.MaxHits;
        gameOverPanel.SetActive(gm.IsGameOver);
        winPanel.SetActive(gm.IsWon);

        AdManager ads = AdManager.Instance;
        watchAdButton.SetActive(gm.CanContinue && ads != null && ads.IsRewardedReady());
    }

    public void OnWatchAdPressed()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.ShowRewarded(GameManager.Instance.ContinueAfterReward);
    }

    public void OnPlayAgainPressed()
    {
        GameManager.Instance.Restart();
    }
}
