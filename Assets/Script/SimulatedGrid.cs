using System;

public class SimulatedGrid
{
    private int width;
    private int height;
    private int[] colorDistribution;
    private int[,] grid;

    public SimulatedGrid(int width, int height, int[] colorDistribution)
    {
        this.width = width;
        this.height = height;
        this.colorDistribution = colorDistribution;

        if (colorDistribution.Length != 6)
        {
            throw new ArgumentException("Jumlah distribusi warna harus 6.");
        }

        grid = new int[width, height];
    }

    public void GenerateSolvableGrid()
    {
        int totalBlocks = width * height;
        int totalDistribution = 0;

        foreach (var color in colorDistribution)
        {
            totalDistribution += color;
        }

        if (totalDistribution != totalBlocks)
        {
            throw new ArgumentException("Total distribusi warna tidak cocok dengan ukuran grid.");
        }

        // Isi grid berdasarkan distribusi warna
        int currentColor = 0;
        int colorCount = colorDistribution[currentColor];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (colorCount > 0)
                {
                    grid[x, y] = currentColor;
                    colorCount--;
                }
                else
                {
                    currentColor++;
                    if (currentColor >= colorDistribution.Length) currentColor = 0;
                    colorCount = colorDistribution[currentColor] - 1;
                }
            }
        }
    }

    public bool CanClear()
    {
        // Logika untuk memeriksa apakah grid dapat dibersihkan atau tidak
        return true;
    }
}
