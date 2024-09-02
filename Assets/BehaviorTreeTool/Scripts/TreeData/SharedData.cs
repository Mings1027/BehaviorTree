using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class SharedData : ScriptableObject
    {
        [SerializeReference] private List<SharedVariableBase> variables;

        public List<SharedVariableBase> Variables
        {
            get => variables;
            set => variables = value;
        }

        private void OnEnable()
        {
            variables ??= new List<SharedVariableBase>();
        }
    }
}