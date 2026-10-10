using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] int maxHits = 5;
    [SerializeField] int continueBonusLives = 3;
    [SerializeField] int interstitialEveryNRestarts = 3;

    static int restartCounter = 0;
    int hits = 0;
    bool isGameOver = false;
    bool isWon = false;                      // added
    bool continueUsed = false;

    public int Hits => hits;
    public int MaxHits => maxHits;
    public int BonusLives => continueBonusLives;
    public bool IsGameOver => isGameOver;
    public bool IsWon => isWon;              // added
    public bool CanContinue => !continueUsed;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    public void RegisterHit()
    {
        if (isGameOver || isWon) return;     // added "|| isWon"
        hits++;
        Debug.Log("Hits: " + hits + " / " + maxHits);
        if (hits >= maxHits) EndGame();
    }

    public void Win()                        // added
    {
        if (isGameOver || isWon) return;
        isWon = true;
        Time.timeScale = 0f;
    }

    void EndGame()
    {
        isGameOver = true;
        Time.timeScale = 0f;
    }

    public void ContinueAfterReward()
    {
        continueUsed = true;
        hits = Mathf.Max(0, maxHits - continueBonusLives);
        isGameOver = false;
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        restartCounter++;
        bool showAd = restartCounter % interstitialEveryNRestarts == 0;
        if (showAd && AdManager.Instance != null)
            AdManager.Instance.ShowInterstitial(ReloadScene);
        else
            ReloadScene();
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
