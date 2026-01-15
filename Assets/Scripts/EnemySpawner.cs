using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnSettings
{
    public GameObject enemyPrefab; // Префаб врага
    public int minCount = 1;       // Минимальное количество при спавне
    public int maxCount = 3;       // Максимальное количество при спавне
    public float minRadius = 2f;   // Минимальная дистанция от центра
    public float maxRadius = 5f;   // Максимальная дистанция от центра
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<EnemySpawnSettings> enemies; // Список врагов для спавна

    [Header("General Settings")]
    public float spacing = 1f; // Минимальное расстояние между врагами

    [SerializeField] private GameObject Anomaly;

    private void Start()
    {
        SpawnEnemies();
        Anomaly.transform.position = GetRandomPosition(enemies[0].minRadius, enemies[0].maxRadius);
    }

    // Метод спавна всех врагов
    public void SpawnEnemies()
    {
        foreach (var setting in enemies)
        {
            int count = Random.Range(setting.minCount, setting.maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetRandomPosition(setting.minRadius, setting.maxRadius);
                // Проверка на расстояние между уже спавненными объектами
                int attempts = 0;
                while (!IsPositionValid(spawnPos) && attempts < 10)
                {
                    spawnPos = GetRandomPosition(setting.minRadius, setting.maxRadius);
                    attempts++;
                }

                GameObject g = Instantiate(setting.enemyPrefab, spawnPos, Quaternion.identity, transform);
                print(g.name);
            }
        }
    }

    // Генерация случайной позиции вокруг центра спавнера
    private Vector3 GetRandomPosition(float minRadius, float maxRadius)
    {
        Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
        Vector3 pos = transform.position + new Vector3(circle.x, 0f, circle.y);
        return pos;
    }

    // Проверка, чтобы враги не спавнились слишком близко друг к другу
    private bool IsPositionValid(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, spacing);
        return hits.Length == 0;
    }

    // В редакторе можно вызвать для проверки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        if (enemies != null)
        {
            foreach (var setting in enemies)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawWireSphere(transform.position, setting.minRadius);
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(transform.position, setting.maxRadius);
            }
        }
    }
}