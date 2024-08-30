using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class SharedData : ScriptableObject
    {
        [SerializeReference] private List<SharedVariableBase> variables;
        public List<SharedVariableBase> Variables => variables;

        private void OnEnable()
        {
            variables ??= new List<SharedVariableBase>();
        }
    }
}