using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.Scripts.TreeSharedData
{
    [CreateAssetMenu(menuName = "BehaviourTree/SharedData")]
    public class SharedData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeReference]
        private List<SharedVariableBase> variables;

        [NonSerialized]
        private Dictionary<string, SharedVariableBase> variableDictionary;

        public List<SharedVariableBase> Variables
        {
            get => variables;
            set => variables = value;
        }

        private void OnEnable()
        {
            variables ??= new List<SharedVariableBase>();
            InitializeDictionary();
        }

        private void InitializeDictionary()
        {
            variableDictionary = new Dictionary<string, SharedVariableBase>();
            foreach (var variable in variables)
            {
                if (variable != null)
                {
                    variableDictionary[variable.variableName] = variable;
                }
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            InitializeDictionary();
        }

        public void AddVariable(SharedVariableBase variable)
        {
            variables.Add(variable);
            variableDictionary[variable.variableName] = variable;
        }

        public void RemoveVariable(string variableName)
        {
            if (variableDictionary.TryGetValue(variableName, out var variable))
            {
                variables.Remove(variable);
                variableDictionary.Remove(variableName);
            }
        }

        public SharedVariableBase GetVariable(string variableName)
        {
            variableDictionary.TryGetValue(variableName, out var variable);
            return variable;
        }

        public SharedData Clone()
        {
            var clone = Instantiate(this);
            clone.Variables = new List<SharedVariableBase>(variables);
            clone.InitializeDictionary();
            return clone;
        }
    }
}
