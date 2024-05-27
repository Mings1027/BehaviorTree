using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.Scripts.TreeSharedData
{
    [CreateAssetMenu(menuName = "BehaviourTree/SharedData")]
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

        public void AddVariable(SharedVariableBase variable)
        {
            variables.Add(variable);
        }

        public void RemoveVariable(string variableName)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].VariableName == variableName)
                {
                    variables.RemoveAt(i);
                    break;
                }
            }
        }

        public SharedData Clone()
        {
            var clone = Instantiate(this);
            clone.Variables = new List<SharedVariableBase>();
            foreach (var variable in variables)
            {
                clone.Variables.Add(variable.Clone());
            }

            return clone;
        }
    }
}