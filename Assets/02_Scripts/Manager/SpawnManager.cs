using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] PlayerController _player;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

    [Header("Current Stage")]
    [SerializeField] private StageData _currentStageData;

    private void Awake()
    {
        if (_player == null) _player = FindFirstObjectByType<PlayerController>();
    }

    private void Start()
    {
        if (_currentStageData != null)
        {
            SpawnStageMonsters(_currentStageData);
        }
    }

    public void SpawnStageMonsters(StageData data)
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogWarning("스폰 포인트 설정 X");
            return;
        }
        
        foreach (var info in data.MonsterList)
        {
            int finalSpawnCount = Random.Range(info.MinSpawnCount, info.MaxSpawnCount + 1);

            for (int i = 0; i < finalSpawnCount; i++)
            {
                int randomIndex = Random.Range(0, _spawnPoints.Count);
                Vector3 spawnPos = _spawnPoints[randomIndex].position;

                Vector2 randomCircle = Random.insideUnitCircle * 2.0f;

                Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y);

                GameObject monster = Instantiate(info.MonsterPrefab, spawnPos + offset, Quaternion.identity);

                if (monster.TryGetComponent<MonsterBase>(out MonsterBase monsterBase))
                {
                    if (_player != null)
                    {
                        monsterBase.SetTarget(_player.gameObject);
                    }
                }
            }
        }
    }
}
