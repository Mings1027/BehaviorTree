using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject monster;
    [SerializeField] private int monsterCount;
    [SerializeField] private GameObject unit;
    [SerializeField] private int unitCount;

    [SerializeField] private int spawnRange;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < monsterCount; i++)
        {
            var ranPos = Random.insideUnitSphere * spawnRange;
            if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
            {
                var obj = Instantiate(monster, hit.position, Quaternion.identity);
                obj.name += $"({i})";
            }
        }
        for (int i = 0; i < unitCount; i++)
        {
            var ranPos = Random.insideUnitSphere * spawnRange;
            if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
            {
                var obj = Instantiate(unit, hit.position, Quaternion.identity);
                obj.name += $"({i})";
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRange);
    }
}