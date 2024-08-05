using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class SharedData : ScriptableObject
    {
        [SerializeReference]
        private List<SharedVariableBase> variables;

        public List<SharedVariableBase> Variables
        {
            get => variables;
            private set => variables = value;
        }

        private void OnEnable()
        {
            variables ??= new List<SharedVariableBase>();
        }

        public SharedData Clone()
        {
            var clone = Instantiate(this);
            clone.Variables = new List<SharedVariableBase>();
            for (int i = 0; i < variables.Count; i++)
            {
                SharedVariableBase variable = variables[i];
                clone.Variables.Add(variable.Clone());
            }

            return clone;
        }
    }
}