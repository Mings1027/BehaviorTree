using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Tree
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject monster;
        [SerializeField] private int monsterCount;
        [SerializeField] private GameObject unit;
        [SerializeField] private int unitCount;

        [SerializeField] private int spawnRange;

        // Start is called before the first frame update
        private void Start()
        {
            for (var i = 0; i < monsterCount; i++)
            {
                var ranPos = Random.insideUnitSphere * spawnRange;
                if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                {
                    Instantiate(monster, hit.position, Quaternion.identity);
                }
            }
            for (var i = 0; i < unitCount; i++)
            {
                var ranPos = Random.insideUnitSphere * spawnRange;
                if (NavMesh.SamplePosition(ranPos, out var hit, 50, NavMesh.AllAreas))
                {
                    Instantiate(unit, hit.position, Quaternion.identity);
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