using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Dimensiones")]
    public int width = 10;
    public int height = 10;

    [Header("Start / End")]
    public Vector2Int startCell;
    public Vector2Int endCell;

    [Header("Prefabs")]
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject roadPrefab;
    public GameObject buildingSpotPrefab; 
    public GameObject sidewalkPrefab;     
    public GameObject buildingPrefab;

    [Header("Limite de banquetas de construccion")]
    public int maxSpots = 6;

    [Header("Grid + Parent")]
    public Grid grid;
    public Transform mapParent;

    private bool[,] occupied;
    private List<Vector2Int> pathCells;

    void Start()
    {
        GenerateMap();
    }

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        ClearMap();
        occupied = new bool[width, height];
        BuildPath();
        InstantiateTiles();
    }

    void ClearMap()
    {
        for (int i = mapParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(mapParent.GetChild(i).gameObject);
    }

    void BuildPath()
    {
        var dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var q = new Queue<Vector2Int>();
        prev[startCell] = startCell;
        q.Enqueue(startCell);

        while (q.Count > 0 && !prev.ContainsKey(endCell))
        {
            var cell = q.Dequeue();
            // barajar direcciones
            for (int i = 0; i < dirs.Length; i++)
            {
                int r = Random.Range(i, dirs.Length);
                (dirs[i], dirs[r]) = (dirs[r], dirs[i]);
            }
            foreach (var d in dirs)
            {
                var nxt = cell + d;
                if (nxt.x < 0 || nxt.x >= width || nxt.y < 0 || nxt.y >= height) continue;
                if (prev.ContainsKey(nxt)) continue;
                prev[nxt] = cell;
                q.Enqueue(nxt);
                if (nxt == endCell) break;
            }
        }

        pathCells = new List<Vector2Int>();
        if (!prev.ContainsKey(endCell))
        {
            Debug.LogError("No se encontro ruta start?end");
            return;
        }
        var cur = endCell;
        while (true)
        {
            pathCells.Add(cur);
            if (cur == startCell) break;
            cur = prev[cur];
        }
        pathCells.Reverse();
    }

    void InstantiateTiles()
    {
        // Helper para instanciar y marcar ocupado
        void Place(Vector2Int cell, GameObject prefab)
        {
            var worldPos = grid.CellToWorld(new Vector3Int(cell.x, 0, cell.y));
            Instantiate(prefab, worldPos, Quaternion.identity, mapParent);
            occupied[cell.x, cell.y] = true;
        }

        // 1 Start / End
        Place(startCell, startPrefab);
        Place(endCell, endPrefab);

        // 2 Camino
        foreach (var c in pathCells)
        {
            if (c == startCell || c == endCell) continue;
            Place(c, roadPrefab);
        }

        // 3 Recolectar celdas adyacentes al camino
        var adjacent = new List<Vector2Int>();
        for (int i = 0; i < pathCells.Count; i++)
        {
            var c = pathCells[i];
            Vector2Int dir = (i < pathCells.Count - 1)
                ? pathCells[i + 1] - c
                : c - pathCells[i - 1];
            var left = new Vector2Int(-dir.y, dir.x);
            var right = new Vector2Int(dir.y, -dir.x);

            var lpos = c + left;
            var rpos = c + right;
            if (IsValid(lpos)) adjacent.Add(lpos);
            if (IsValid(rpos)) adjacent.Add(rpos);
        }
        var uniqueAdj = new List<Vector2Int>(new HashSet<Vector2Int>(adjacent));

        // 4 maxSpots para buildingSpot
        var chosenSpots = new HashSet<Vector2Int>();
        var candidates = new List<Vector2Int>(uniqueAdj);
        while (chosenSpots.Count < maxSpots && candidates.Count > 0)
        {
            int idx = Random.Range(0, candidates.Count);
            chosenSpots.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }

        // 5 Colocar buildingSpot y sidewalk en adyacentes
        foreach (var pos in uniqueAdj)
        {
            if (chosenSpots.Contains(pos))
                Place(pos, buildingSpotPrefab);
            else
                Place(pos, sidewalkPrefab);
        }

        // 6 Rellenar el resto con edificios
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!occupied[x, y])
                {
                    Place(new Vector2Int(x, y), buildingPrefab);
                }
            }
        }
    }

    bool IsValid(Vector2Int c)
    {
        return c.x >= 0 && c.x < width
            && c.y >= 0 && c.y < height
            && !occupied[c.x, c.y];
    }
}
