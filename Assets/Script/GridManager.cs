using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public GameOverUI gameOverUI; // Drag via Inspector
    private int totalScore = 0; // Atur skor sesuai logic kamu
    public int width = 12;
    public int height = 12;
    public GameObject[] blockPrefabs;

    private List<(int x1, int y1, int x2, int y2, int color)> solutionPairs;

    public float spacing = 0.65f;
    private bool isLoadingGame = false;



    private Block[,] grid;
    public AudioClip popSound; // Drag clip ini di inspector
    public AudioClip reshuffleClip; // audio clip untuk reshuffle
    public AudioClip acakBlock; // audio clip untuk acak block
    public AudioClip kalahClip; // audio clip untuk kalah
    private AudioSource audioSource;

    public float soundVolume = 0.5f; // Optional, atur volume

    private bool isGameOver = false;

    public bool IsGameOver
    {
        get { return isGameOver; }
        set { isGameOver = value; }
    }
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // PlayerPrefs.DeleteKey("SavedScore");
        // PlayerPrefs.DeleteKey("GridWidth");
        // PlayerPrefs.DeleteKey("GridHeight");
        // PlayerPrefs.DeleteKey("GridSpacing");
        audioSource = GetComponent<AudioSource>();
        grid = new Block[width, height];

        // Set isGameOver ke false agar game baru bisa dimulai
        isGameOver = false;

        // Reset data sebelumnya, tidak perlu lagi menghapus PlayerPrefs karena sudah dihapus di GameOver()
        if (PlayerPrefs.HasKey("SavedScore"))
        {
            LoadGame();  // Memuat game jika ada data sebelumnya
        }
        else
        {
            GenerateGrid();  // Jika tidak ada data, generate grid baru
        }
    }


    void OnApplicationQuit()
    {
        // Debug.Log("📦 Auto-save sebelum aplikasi ditutup.");

        // Simpan hanya jika game belum berakhir
        if (GridManager.Instance != null && !GridManager.Instance.IsGameOver)
        {
            GridManager.Instance.SaveGame();
        }

        Time.timeScale = 1f; // Pastikan timeScale dikembalikan ke normal saat keluar aplikasi
    }

    void GenerateGrid()
    {
        // 🔁 Clear grid sebelumnya
        if (isLoadingGame) return;

        if (grid != null)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        grid = new Block[width, height];
        solutionPairs = new List<(int x1, int y1, int x2, int y2, int color)>();

        List<Vector2Int> allPositions = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                allPositions.Add(new Vector2Int(x, y));
            }
        }

        Shuffle(allPositions);

        int totalBlocks = width * height;
        int pairCount = totalBlocks / 2;

        if (allPositions.Count < pairCount * 2)
        {
            // Debug.LogError("Not enough positions for pairs!");
            return;
        }

        for (int i = 0; i < pairCount; i++)
        {
            Vector2Int pos1 = allPositions[0];
            Vector2Int pos2 = allPositions[1];

            allPositions.RemoveAt(0);
            allPositions.RemoveAt(0);

            int color = Random.Range(0, blockPrefabs.Length);
            solutionPairs.Add((pos1.x, pos1.y, pos2.x, pos2.y, color));
        }

        foreach (var pair in solutionPairs)
        {
            Vector2 posA = new Vector2(pair.x1 * spacing, pair.y1 * spacing);
            GameObject goA = Instantiate(blockPrefabs[pair.color], posA, Quaternion.identity, transform);
            Block bA = goA.AddComponent<Block>();
            bA.Init(pair.color, pair.x1, pair.y1);
            grid[pair.x1, pair.y1] = bA;

            Vector2 posB = new Vector2(pair.x2 * spacing, pair.y2 * spacing);
            GameObject goB = Instantiate(blockPrefabs[pair.color], posB, Quaternion.identity, transform);
            Block bB = goB.AddComponent<Block>();
            bB.Init(pair.color, pair.x2, pair.y2);
            grid[pair.x2, pair.y2] = bB;
        }
        if (reshuffleClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(reshuffleClip);
        }
    }


    public void CheckMatch(Block block)
    {
        List<Block> matched = new List<Block>();
        FindConnectedBlocks(block.x, block.y, block.colorIndex, matched, new bool[width, height]);

        if (matched.Count >= 2)
        {
            // Hitung score berdasarkan jumlah block
            totalScore += (matched.Count - 1) * 10;
            // Debug.Log("Score: " + totalScore);

            ScoreManager.Instance.AddScore(totalScore); // Tambahkan score sekali

            AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position, soundVolume);

            foreach (Block b in matched)
            {
                Animator anim = b.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("pop");
                }

                Destroy(b.gameObject, 0.3f);
                grid[b.x, b.y] = null;
            }

            Invoke(nameof(FallDown), 0.2f);
        }
    }

    private void CheckGameOver()
    {
        List<Block> remainingBlocks = new List<Block>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    remainingBlocks.Add(grid[x, y]);
                }
            }
        }

        int remainingCount = remainingBlocks.Count;

        // Jika tersisa 2 - 4 blok
        if (remainingCount >= 2 && remainingCount <= 4)
        {
            // Ambil semua warna unik
            HashSet<int> uniqueColors = new HashSet<int>();
            foreach (var block in remainingBlocks)
            {
                uniqueColors.Add(block.colorIndex);
            }

            // Jika semua warna berbeda (jumlah warna unik == jumlah blok)
            if (uniqueColors.Count == remainingCount)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        // Set game over flag di PlayerPrefs agar tidak tersimpan ketika keluar game
        PlayerPrefs.SetInt("IsGameOver", 1);
        PlayerPrefs.Save();

        // Pemutaran audio game over dan tampilan UI
        if (kalahClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(kalahClip);
        }

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(totalScore);
        }

        Time.timeScale = 0f; // Game pause saat game over
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("SavedScore", totalScore);
        PlayerPrefs.SetInt("GridWidth", width);
        PlayerPrefs.SetInt("GridHeight", height);
        PlayerPrefs.SetFloat("GridSpacing", spacing);

        // Simpan semua blok
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    string key = $"Block_{x}_{y}_Exists";
                    PlayerPrefs.SetInt(key, 1); // Menandai bahwa ada blok di posisi ini

                    string colorKey = $"Block_{x}_{y}_Color";
                    PlayerPrefs.SetInt(colorKey, grid[x, y].colorIndex);
                }
                else
                {
                    // Tandai blok kosong
                    PlayerPrefs.DeleteKey($"Block_{x}_{y}_Exists");
                    PlayerPrefs.DeleteKey($"Block_{x}_{y}_Color");
                }
            }
        }

        PlayerPrefs.SetInt("IsGameOver", isGameOver ? 1 : 0);
        PlayerPrefs.Save();
        // Debug.Log("💾 Game berhasil disimpan!");
    }


    public void LoadGame()
    {
        // Tidak ada data game sebelumnya karena PlayerPrefs sudah dihapus
        if (!PlayerPrefs.HasKey("SavedScore"))
        {
            return;  // Tidak perlu melanjutkan memuat game, grid akan kosong
        }

        // Memuat score dan mengupdate ScoreManager
        totalScore = PlayerPrefs.GetInt("SavedScore", 0);
        ScoreManager.Instance.AddScore(totalScore);  // Update score di ScoreManager

        width = PlayerPrefs.GetInt("GridWidth", width);
        height = PlayerPrefs.GetInt("GridHeight", height);
        spacing = PlayerPrefs.GetFloat("GridSpacing", spacing);

        // Bersihkan grid lama dulu
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        grid = new Block[width, height];

        // Buat ulang blok dari data yang disimpan
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string existsKey = $"Block_{x}_{y}_Exists";

                if (PlayerPrefs.HasKey(existsKey))
                {
                    string colorKey = $"Block_{x}_{y}_Color";
                    int colorIndex = PlayerPrefs.GetInt(colorKey, 0);

                    CreateBlockAtPosition(x, y, colorIndex);  // Buat blok berdasarkan data yang disimpan
                }
            }
        }
    }

    IEnumerator DelayedGameOver()
    {
        yield return null; // Tunggu 1 frame
        GameOver(); // Panggil GameOver setelah 1 frame
    }



    public void CreateBlockAtPosition(int x, int y, int colorIndex)
    {
        GameObject prefab = blockPrefabs[colorIndex];

        if (prefab == null)
        {
            // Debug.LogError($"❌ Prefab untuk colorIndex {colorIndex} tidak ditemukan!");
            return;
        }

        Vector2 spawnPos = new Vector2(x * spacing, y * spacing); // ✅ PAKAI spacing
        GameObject newBlock = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        Block blockScript = newBlock.GetComponent<Block>();

        if (blockScript == null)
        {
            // Debug.LogError($"❌ Prefab di index {colorIndex} tidak memiliki komponen Block!");
            return;
        }

        blockScript.Init(colorIndex, x, y); // Tetap simpan posisi logika
        grid[x, y] = blockScript;
    }


    bool HasAnyAvailableMatch()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block current = grid[x, y];
                if (current == null) continue;

                int color = current.colorIndex;
                if (IsNeighborSameColor(x + 1, y, color) ||
                    IsNeighborSameColor(x - 1, y, color) ||
                    IsNeighborSameColor(x, y + 1, color) ||
                    IsNeighborSameColor(x, y - 1, color))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsNeighborSameColor(int x, int y, int color)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;

        Block neighbor = grid[x, y];
        return neighbor != null && neighbor.colorIndex == color;
    }

    void RandomizeRemainingBlocks()
    {
        List<Block> remainingBlocks = new List<Block>();
        if (acakBlock != null && audioSource != null)
        {
            audioSource.PlayOneShot(acakBlock);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    remainingBlocks.Add(grid[x, y]);
                    grid[x, y].gameObject.SetActive(false);
                    grid[x, y] = null;
                }
            }
        }

        if (remainingBlocks.Count == 0)
        {

            // Debug.Log("✅ All blocks cleared — starting new grid.");
            StartCoroutine(GenerateGridSmooth());
            if (reshuffleClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(reshuffleClip);
            }

            return;
        }

        Dictionary<int, List<Block>> colorGroups = new Dictionary<int, List<Block>>();
        foreach (var block in remainingBlocks)
        {
            if (!colorGroups.ContainsKey(block.colorIndex))
            {
                colorGroups[block.colorIndex] = new List<Block>();
            }
            colorGroups[block.colorIndex].Add(block);
        }

        List<Block> reordered = new List<Block>();
        while (colorGroups.Count > 0)
        {
            List<int> availableColors = new List<int>(colorGroups.Keys);
            Shuffle(availableColors);

            foreach (int color in availableColors)
            {
                var blocks = colorGroups[color];
                int takeCount = Mathf.Min(2, blocks.Count);

                for (int i = 0; i < takeCount; i++)
                {
                    reordered.Add(blocks[0]);
                    blocks.RemoveAt(0);
                }

                if (blocks.Count == 0)
                {
                    colorGroups.Remove(color);
                }
            }
        }

        int totalBlocks = reordered.Count;
        int columns = Mathf.Min(width, totalBlocks);
        int rows = Mathf.CeilToInt((float)totalBlocks / columns);

        int startX = (width - columns) / 2;
        int startY = 0;

        for (int i = 0; i < reordered.Count; i++)
        {
            int localX = i % columns;
            int localY = i / columns;

            int x = startX + localX;
            int y = startY + localY;

            if (x < width && y < height)
            {
                Block block = reordered[i];
                block.x = x;
                block.y = y;
                block.transform.position = new Vector2(x * spacing, y * spacing);
                block.gameObject.SetActive(true);
                grid[x, y] = block;
            }
        }

        if (!HasAnyAvailableMatch())
        {
            // Debug.Log("🔁 Randomized grid has no matches, regenerating.");
            StartCoroutine(GenerateGridSmooth());
            if (reshuffleClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(reshuffleClip);
            }

        }

    }

    void FallDown()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyY = -1;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null && emptyY == -1)
                {
                    emptyY = y;
                }
                else if (grid[x, y] != null && emptyY != -1)
                {
                    Block b = grid[x, y];
                    grid[x, emptyY] = b;
                    b.y = emptyY;
                    b.transform.position = new Vector2(x * spacing, emptyY * spacing);
                    grid[x, y] = null;

                    emptyY++;
                }
            }
        }

        Invoke(nameof(ShiftToCenter), 0.2f);
        CheckGameOver();
    }

    void ShiftToCenter()
    {
        List<int> filledColumns = new List<int>();
        for (int x = 0; x < width; x++)
        {
            bool hasBlock = false;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    hasBlock = true;
                    break;
                }
            }

            if (hasBlock)
                filledColumns.Add(x);
        }

        int targetStartX = (width - filledColumns.Count) / 2;
        Block[,] newGrid = new Block[width, height];

        for (int i = 0; i < filledColumns.Count; i++)
        {
            int oldX = filledColumns[i];
            int newX = targetStartX + i;

            for (int y = 0; y < height; y++)
            {
                if (grid[oldX, y] != null)
                {
                    Block b = grid[oldX, y];
                    b.x = newX;
                    b.transform.position = new Vector2(newX * spacing, y * spacing);
                    newGrid[newX, y] = b;
                }
            }
        }

        grid = newGrid;
        Invoke(nameof(CheckAfterShift), 0.2f);
    }

    void CheckAfterShift()
    {
        if (IsGridCompletelyEmpty())
        {
            // Debug.Log("🎉 All blocks cleared! Generating new grid.");
            StartCoroutine(GenerateGridSmooth());
            if (reshuffleClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(reshuffleClip);
            }
            return;
        }

        if (!HasAnyAvailableMatch())
        {
            // Debug.Log("❌ No more available matches. Randomizing remaining blocks.");
            RandomizeRemainingBlocks();
        }

        CheckGameOver();
    }

    public IEnumerator GenerateGridSmooth(float delay = 0.05f, float moveDuration = 0.25f)
    {
        float halfWidth = width / 2f;
        List<Coroutine> coroutines = new List<Coroutine>();

        // Kiri ke tengah DAN kanan ke tengah dalam satu loop
        for (int y = 0; y < height; y++)
        {
            for (int i = 0; i < halfWidth; i++)
            {
                int leftX = i;
                int rightX = width - 1 - i;

                // Kiri
                if (grid[leftX, y] == null)
                {
                    int color = Random.Range(0, 4);
                    CreateBlockAtPosition(leftX, y, color);
                    Vector3 target = new Vector3(leftX * spacing, y * spacing, 0);
                    Vector3 start = new Vector3(-width * spacing, y * spacing, 0);
                    Block block = grid[leftX, y];
                    block.transform.position = start;
                    coroutines.Add(StartCoroutine(MoveBlockToPosition(block, start, target, moveDuration)));
                }

                // Kanan
                if (rightX != leftX && grid[rightX, y] == null) // Jangan double di tengah jika lebar genap
                {
                    int color = Random.Range(0, 4);
                    CreateBlockAtPosition(rightX, y, color);
                    Vector3 target = new Vector3(rightX * spacing, y * spacing, 0);
                    Vector3 start = new Vector3((width + 1) * spacing, y * spacing, 0);
                    Block block = grid[rightX, y];
                    block.transform.position = start;
                    coroutines.Add(StartCoroutine(MoveBlockToPosition(block, start, target, moveDuration)));
                }
            }

            // Delay antar baris (bukan antar blok)
            yield return new WaitForSeconds(delay);
        }

        // Tunggu semua selesai (bisa juga pakai total delay jika mau)
        yield return new WaitForSeconds(moveDuration + 0.2f);

        CheckAfterShift();
    }
    private IEnumerator MoveBlockToPosition(Block block, Vector3 startPos, Vector3 targetPos, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            block.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        block.transform.position = targetPos;
    }




    bool IsGridCompletelyEmpty()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                    return false;
            }
        }
        return true;
    }

    void FindConnectedBlocks(int x, int y, int color, List<Block> matched, bool[,] visited)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        if (visited[x, y])
            return;

        Block current = grid[x, y];
        if (current == null || current.colorIndex != color)
            return;

        visited[x, y] = true;
        matched.Add(current);

        FindConnectedBlocks(x + 1, y, color, matched, visited);
        FindConnectedBlocks(x - 1, y, color, matched, visited);
        FindConnectedBlocks(x, y + 1, color, matched, visited);
        FindConnectedBlocks(x, y - 1, color, matched, visited);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
}
