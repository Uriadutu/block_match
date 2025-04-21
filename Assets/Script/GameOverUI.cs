using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Text currentScoreText;
    public Text highScoreText;

    private const string HighScoreKey = "HighScore";

    public Button restartButton;
    public Button menuButton;

    private void Start()
    {
        // Sembunyikan UI saat awal
        gameOverPanel.SetActive(false);

        // Tambahkan listener tombol
        restartButton.onClick.AddListener(RestartGame);
        menuButton.onClick.AddListener(BackToMenu);
    }



    public void ShowGameOver(int finalScore)
    {
        gameObject.SetActive(true);

        // Tampilkan skor saat ini
        currentScoreText.text = finalScore.ToString();

        // Ambil skor tertinggi dari PlayerPrefs
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        // Update jika skor sekarang lebih tinggi
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        // Tampilkan skor tertinggi
        highScoreText.text = highScore.ToString();
    }
    public void RestartGame()
    {
        // Reset semua data PlayerPrefs sebelum merestart
        // PlayerPrefs.DeleteAll(); // Menghapus semua data yang disimpan di PlayerPrefs

        // Atau jika hanya ingin menghapus data tertentu, gunakan:
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.DeleteKey("GridWidth");
        PlayerPrefs.DeleteKey("GridHeight");
        PlayerPrefs.DeleteKey("GridSpacing");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Muat ulang scene yang sama
    }

    private void BackToMenu()
    {
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.DeleteKey("GridWidth");
        PlayerPrefs.DeleteKey("GridHeight");
        PlayerPrefs.DeleteKey("GridSpacing");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Muat ulang scene yang sama

    }
}
