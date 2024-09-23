using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class GlobalVariables : MonoSingleton<GlobalVariables>
    {
#if UNITY_EDITOR
        public List<SharedVariableBase> Variables
        {
            get => variables;
            set => variables = value;
        }
#endif
        private static readonly Dictionary<string, SharedVariableBase> VariableTable = new();

        [SerializeReference, HideInInspector] private List<SharedVariableBase> variables = new();

        public static SharedVariableBase GetVariable(string variableName)
        {
            if (VariableTable.TryGetValue(variableName, out var variable))
            {
                return variable;
            }

            throw new KeyNotFoundException($"The variable {variableName} does not exist.");
        }

        public static SharedVariable<T> GetVariable<T>(string variableName)
        {
            var variable = GetVariable(variableName);
            if (variable is SharedVariable<T> typedVariable)
            {
                return typedVariable;
            }

            throw new InvalidCastException(
                $"The variable with the name '{variableName}' exists but is not of type '{typeof(T)}'.");
        }

        public static void SetVariable(string variableName, object value)
        {
            var variable = GetVariable(variableName);
            if (variable != null) variable.SetValue(value);
            else
            {
                throw new ArgumentException(
                    $"The variable with the name '{variableName}' does not exist in the global variables list.");
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitTable();
        }

        private static void InitTable()
        {
            VariableTable.Clear();
            var variableList = Instance.variables;
            for (int i = 0; i < variableList.Count; i++)
            {
                VariableTable[variableList[i].VariableName] = variableList[i];
            }
        }
    }
}