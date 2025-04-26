using UnityEngine;

[CreateAssetMenu(menuName = "RatDefence/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float speed = 2f;
    public int health = 1;
    public GameObject prefab;
}
