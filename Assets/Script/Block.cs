using UnityEngine;

public class Block : MonoBehaviour
{
    public int colorIndex;
    public int x, y;

    public void Init(int color, int xPos, int yPos)
    {
        colorIndex = color;
        x = xPos;
        y = yPos;
    }

    private void OnMouseDown()
    {
        GridManager.Instance.CheckMatch(this);
    }
}
