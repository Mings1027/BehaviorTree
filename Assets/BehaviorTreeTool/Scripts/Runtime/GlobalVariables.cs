using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class GlobalVariables : MonoBehaviour
    {
        private static GlobalVariables _instance;

        public static GlobalVariables Instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<GlobalVariables>();
                return _instance;
            }
            private set => _instance = value;
        }

        public List<SharedVariableBase> Variables
        {
            get => variables;
            set => variables = value;
        }

        [SerializeReference, HideInInspector] private List<SharedVariableBase> variables = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
        }

        public SharedVariableBase GetVariable(string variableName)
        {
            var curVariables = Instance.variables;

            for (var i = 0; i < curVariables.Count; i++)
            {
                if (curVariables[i].VariableName == variableName)
                    return curVariables[i];
            }

            return null;
        }

        public void SetVariable(string variableName, object value)
        {
            var variable = GetVariable(variableName);
            if (variable != null) variable.SetValue(value);
            else
            {
                throw new ArgumentException(
                    $"The variable with the name '{variableName}' does not exist in the global variables list.");
            }
        }
    }
}