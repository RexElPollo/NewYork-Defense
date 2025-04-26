using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    float speed;
    List<Transform> waypoints;
    int index;
    EnemyManager manager;

    public void Init(EnemyData data, Transform pathParent)
    {
        speed = data.speed;
        manager = FindObjectOfType<EnemyManager>();
        waypoints = new List<Transform>(pathParent.childCount);
        foreach (Transform t in pathParent) waypoints.Add(t);
        waypoints.Sort((a, b) => a.name.CompareTo(b.name));
        index = 0;
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (index >= waypoints.Count) return;
        transform.position = Vector3.MoveTowards(
            transform.position,
            waypoints[index].position,
            speed * Time.deltaTime
        );
        if (transform.position == waypoints[index].position) index++;
        if (index >= waypoints.Count)
        {
            manager.DespawnEnemy(gameObject);
            // Aquí podrías restar vidas o contar “escape”
        }
    }
}
