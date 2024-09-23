using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Tree
{
    public class Spawner : MonoBehaviour
    {
        private int _limitCount;
        [SerializeField] private float spawnDelay;
        [SerializeField] private GameObject monster;
        [SerializeField] private int monsterCount;
        [SerializeField] private GameObject unit;
        [SerializeField] private int unitCount;

        [SerializeField] private int spawnRange;
        [SerializeField] private bool autoSpawn;

        // Start is called before the first frame update
        private void Start()
        {
            for (var i = 0; i < monsterCount; i++)
            {
                var ranPos = Random.insideUnitSphere * spawnRange;
                if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                {
                    var obj = Instantiate(monster, hit.position, Quaternion.identity);
                    obj.name += "_" + i;
                    _limitCount++;
                }
            }

            for (var i = 0; i < unitCount; i++)
            {
                var ranPos = Random.insideUnitSphere * spawnRange;
                if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                {
                    var obj = Instantiate(unit, hit.position, Quaternion.identity);
                    obj.name += "_" + i;
                    _limitCount++;
                }
            }

            DelaySpawn().Forget();

            var a = GlobalVariables.GetVariable("IsPlaying");
            var b = GlobalVariables.GetVariable<Transform>("Global Target");
            GlobalVariables.SetVariable("IsPlaying", false);
        }

        private async UniTask DelaySpawn()
        {
            while (_limitCount < 10000)
            {
                if (!autoSpawn) return;
                await UniTask.Delay(TimeSpan.FromSeconds(spawnDelay), cancellationToken: destroyCancellationToken);
                for (var i = 0; i < monsterCount; i++)
                {
                    var ranPos = Random.insideUnitSphere * spawnRange;
                    if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                    {
                        var obj = Instantiate(monster, hit.position, Quaternion.identity);
                        obj.name += "_" + i;
                        _limitCount++;
                    }
                }

                for (var i = 0; i < unitCount; i++)
                {
                    var ranPos = Random.insideUnitSphere * spawnRange;
                    if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                    {
                        var obj = Instantiate(unit, hit.position, Quaternion.identity);
                        obj.name += "_" + i;
                        _limitCount++;
                    }
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