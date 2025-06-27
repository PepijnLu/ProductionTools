using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public float lineThickness = 0.05f;
    public Material lineMaterial;

    void Start()
    {
        for (int x = 0; x <= width; x++)
        {
            CreateLine(
                new Vector3(x * cellSize, 0, 0),
                new Vector3(x * cellSize, height * cellSize, 0)
            );
        }

        for (int y = 0; y <= height; y++)
        {
            CreateLine(
                new Vector3(0, y * cellSize, 0),
                new Vector3(width * cellSize, y * cellSize, 0)
            );
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.parent = transform;

        var lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = lineMaterial;
        lr.startWidth = lr.endWidth = lineThickness;
        lr.useWorldSpace = false;
        lr.sortingOrder = 100; // Make sure it's rendered on top
    }
}
