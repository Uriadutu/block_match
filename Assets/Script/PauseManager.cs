using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseUI; // Assign canvas panel UI Pause di Inspector
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Sembunyikan UI pause saat game mulai
        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    void Update()
    {
        // Cegah input ketika game dijeda
        if (isPaused) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseUI.SetActive(true);

        // Nonaktifkan semua input agar tidak terjadi aksi di belakang layar
        // Misalnya jika ada script yang menerima input, kita bisa menonaktifkan script tersebut
        // Contoh:
        GridManager.Instance.enabled = false; // Pastikan input untuk menghancurkan blok tidak bekerja
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseUI.SetActive(false);

        // Aktifkan kembali script atau input yang sebelumnya dinonaktifkan
        GridManager.Instance.enabled = true; // Aktifkan input lagi untuk menghancurkan blok
    }

    public void RestartGame()
    {
        // Reset semua data PlayerPrefs sebelum merestart
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.DeleteKey("GridWidth");
        PlayerPrefs.DeleteKey("GridHeight");
        PlayerPrefs.DeleteKey("GridSpacing");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Muat ulang scene yang sama
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        // Simpan data sebelum keluar
        if (GridManager.Instance != null)
        {
            GridManager.Instance.SaveGame();
        }

        SceneManager.LoadScene("MainMenu"); // Pastikan kamu punya scene bernama "MainMenu"
    }
}
