using System;
using UnityEngine;
using UnityEngine.AI;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        private Material _material;
        private NavMeshAgent _navMeshAgent;
        

        private void Start()
        {
            _material = GetComponent<Renderer>().material;
        }
    }
    
}