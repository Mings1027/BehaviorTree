using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.Scripts.TreeSharedData
{
    [CreateAssetMenu]
    public class TestSO : ScriptableObject
    {
        [SerializeField] private List<Transform> testList;
    }
}
