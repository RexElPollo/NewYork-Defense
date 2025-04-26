// EnemyManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Configuración de Enemigos")]
    public EnemyData[] enemyTypes;      // Tus 3 ScriptableObjects (RataPequeña, RataNormal, RataGrande)
    public int poolSizePerType = 10;    // Número de instancias por tipo en el pool

    [Header("Ruta de Movimiento")]
    public Transform pathParent;        // GameObject PathWaypoints que contiene los waypoints

    [Header("Spawn")]
    public float spawnInterval = 0.5f;  // Segundos entre cada spawn

    // Diccionario que asocia cada EnemyData con su cola de GameObjects (pool)
    private Dictionary<EnemyData, Queue<GameObject>> pools;

    void Awake()
    {
        // Inicializa el pool para cada tipo de rata
        pools = new Dictionary<EnemyData, Queue<GameObject>>();
        foreach (var data in enemyTypes)
        {
            var q = new Queue<GameObject>();
            for (int i = 0; i < poolSizePerType; i++)
            {
                GameObject go = Instantiate(data.prefab);
                go.SetActive(false);
                q.Enqueue(go);
            }
            pools[data] = q;
        }
    }

    void Start()
    {
        // Inicia la corrida que spawnea una rata tras otra hasta agotar todos los pools
        StartCoroutine(SpawnAllEnemiesOneByOne());
    }

    private IEnumerator SpawnAllEnemiesOneByOne()
    {
        // Mientras queden enemigos en cualquiera de los pools
        bool anyLeft;
        do
        {
            anyLeft = false;
            // Recorre cada tipo y, si tiene instancias disponibles, spawnea una
            foreach (var data in enemyTypes)
            {
                var q = pools[data];
                if (q.Count > 0)
                {
                    SpawnEnemy(data);
                    anyLeft = true;
                    yield return new WaitForSeconds(spawnInterval);
                }
            }
        } while (anyLeft);
    }

    // Saca del pool la instancia, la activa e inicializa su movimiento
    public void SpawnEnemy(EnemyData data)
    {
        var q = pools[data];
        if (q.Count == 0) return;

        GameObject go = q.Dequeue();
        go.SetActive(true);
        go.GetComponent<EnemyMovement>().Init(data, pathParent);
    }

    // Devuelve al pool la instancia desactivada
    public void DespawnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        string prefabName = enemy.name.Replace("(Clone)", "");
        foreach (var kv in pools)
        {
            if (kv.Key.prefab.name == prefabName)
            {
                kv.Value.Enqueue(enemy);
                return;
            }
        }
    }
}
