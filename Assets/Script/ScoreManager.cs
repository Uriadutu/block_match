using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int score = 0;

    public Text scoreText;  // Drag UI Text ke sini di Inspector

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        score = amount;
        UpdateScoreText();  // Update UI Text
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();  // Update UI Text
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}
