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

        public SharedData Clone()
        {
            var clone = Instantiate(this);
            clone.variables = new List<SharedVariableBase>();
            for (int i = 0; i < variables.Count; i++)
            {
                clone.variables.Add(variables[i].Clone());
            }

            return clone;
        }
    }
}