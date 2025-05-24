using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Referencia al Grid")]
    public Grid grid;                
    public Transform mapParent;      

    [Header("Tamaño")]
    public int width = 10, height = 10;

    [Header("Start / End")]
    public Vector2Int startCell, endCell;

    [Header("Prefabs")]
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject roadPrefab;
    public GameObject spotPrefab;
    public GameObject fillerPrefab;

    bool[,] occupied;
    List<Vector2Int> pathCells;

    void Start()
    {
        GenerateMap();
    }

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
        bool found = false;

        while (q.Count > 0 && !found)
        {
            var cell = q.Dequeue();
            // Barajar direcciones
            for (int i = 0; i < dirs.Length; i++)
            {
                int r = Random.Range(i, dirs.Length);
                var tmp = dirs[i]; dirs[i] = dirs[r]; dirs[r] = tmp;
            }
            foreach (var d in dirs)
            {
                var nxt = cell + d;
                if (nxt.x < 0 || nxt.x >= width || nxt.y < 0 || nxt.y >= height) continue;
                if (prev.ContainsKey(nxt)) continue;
                prev[nxt] = cell;
                if (nxt == endCell) { found = true; break; }
                q.Enqueue(nxt);
            }
        }

        pathCells = new List<Vector2Int>();
        if (!prev.ContainsKey(endCell))
        {
            Debug.LogError("No se pudo generar ruta.");
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
        //instanciar en celdas
        void Place(Vector2Int cell, GameObject prefab)
        {
            var pos = grid.CellToWorld(new Vector3Int(cell.x, 0, cell.y));
            var go = Instantiate(prefab, pos, Quaternion.identity, mapParent);
            occupied[cell.x, cell.y] = true;
        }

        // 1) Start / End
        Place(startCell, startPrefab);
        Place(endCell, endPrefab);

        // 2) Road segments
        foreach (var c in pathCells)
        {
            if (c == startCell || c == endCell) continue;
            Place(c, roadPrefab);
        }

        // 3) Spots
        for (int i = 0; i < pathCells.Count; i++)
        {
            var c = pathCells[i];
            Vector2Int dir = (i < pathCells.Count - 1)
                ? pathCells[i + 1] - c
                : c - pathCells[i - 1];
            var left = new Vector2Int(-dir.y, dir.x);
            var right = new Vector2Int(dir.y, -dir.x);
            TryPlace(c + left);
            TryPlace(c + right);
        }

        void TryPlace(Vector2Int c)
        {
            if (c.x < 0 || c.x >= width || c.y < 0 || c.y >= height) return;
            if (occupied[c.x, c.y]) return;
            Place(c, spotPrefab);
        }

        // 4) Fillers
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!occupied[x, y]) Place(new Vector2Int(x, y), fillerPrefab);
            }
        }
    }
}
