using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public int width = 10;
    public int height = 10;

    void Start()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 spawnPos = new Vector3(x, 0, z);
                GameObject cell = Instantiate(cellPrefab, spawnPos, Quaternion.identity, transform);

                // Nombrar cada celda por coordenadas
                cell.name = $"Cell_{x}_{z}";

                //marcar celdas como "Path" o "Wall"
                if (x == 5) //toda la fila 5 es el camino
                {
                    cell.GetComponent<Renderer>().material.color = Color.yellow;
                    cell.tag = "Path";
                }
                else
                {
                    cell.GetComponent<Renderer>().material.color = Color.gray;
                    cell.tag = "Wall";
                }
            }
        }
    }
}
