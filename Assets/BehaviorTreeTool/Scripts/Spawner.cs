using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Tree
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject monster;
        [SerializeField] private int monsterCount;
        [SerializeField] private float monsterSpawnDelay;

        [SerializeField] private GameObject unit;
        [SerializeField] private int unitCount;
        [SerializeField] private float unitSpawnDelay;

        [SerializeField] private int spawnRange;
        [SerializeField] private bool autoSpawn;

        private void Start()
        {
            SpawnObjects().Forget();
        }

        private async UniTask SpawnObjects()
        {
            // Spawn monsters
            for (var i = 0; i < monsterCount; i++)
            {
                var ranPos = Random.insideUnitSphere * spawnRange;
                if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                {
                    var obj = Instantiate(monster, hit.position, Quaternion.identity);
                    // await UniTask.Delay(TimeSpan.FromSeconds(monsterSpawnDelay),
                    //     cancellationToken: destroyCancellationToken);
                }
            }

            // Spawn units
            for (var i = 0; i < unitCount; i++)
            {
                var ranPos = Random.insideUnitSphere * spawnRange;
                if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                {
                    var obj = Instantiate(unit, hit.position, Quaternion.identity);
                    // await UniTask.Delay(TimeSpan.FromSeconds(unitSpawnDelay),
                    //     cancellationToken: destroyCancellationToken);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRange);
        }
    }
}