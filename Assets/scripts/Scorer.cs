using UnityEngine;
using TMPro;

public class Scorer : MonoBehaviour
{
    [SerializeField] TMP_Text hitsText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] int maxHits = 5;

    int hits = 0;

    void Start()
    {
        hitsText.text = "Hits: 0 / " + maxHits;
    }

    public void AddHit()
    {
        hits++;
        hitsText.text = "Hits: " + hits + " / " + maxHits;
        Debug.Log("You've bumped into a thing this many times: " + hits);

        if (hits >= maxHits)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}