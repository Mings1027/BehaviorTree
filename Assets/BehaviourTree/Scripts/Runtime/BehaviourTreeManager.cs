using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public class BehaviourTreeManager : MonoBehaviour
    {
        private static BehaviourTreeManager _instance;
        private List<BehaviourTreeRunner> _behaviourTreeRunners;

        private void Awake()
        {
            _instance = this;
            _behaviourTreeRunners = new List<BehaviourTreeRunner>();
        }

        private void Start()
        {
            for (int i = 0; i < _behaviourTreeRunners.Count; i++)
            {
                _behaviourTreeRunners[i].Init();
            }
        }

        private void Update()
        {
            for (int i = 0; i < _behaviourTreeRunners.Count; i++)
            {
                _behaviourTreeRunners[i].TreeUpdate();
            }
        }

        public static void AddTree(BehaviourTreeRunner behaviourTreeRunner)
        {
            if (_instance._behaviourTreeRunners.Contains(behaviourTreeRunner)) return;
            _instance._behaviourTreeRunners.Add(behaviourTreeRunner);
        }

        public static void RemoveTree(BehaviourTreeRunner behaviourTreeRunner)
        {
            if (_instance._behaviourTreeRunners.Contains(behaviourTreeRunner))
                _instance._behaviourTreeRunners.Remove(behaviourTreeRunner);
        }
    }
}