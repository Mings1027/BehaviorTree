using System;
using BehaviourTree.Scripts.TreeSharedData;
using Cysharp.Threading.Tasks;
using UnityEngine;

// using Utilities;

namespace BehaviourTree.Scripts
{
    public class Test : MonoBehaviour
    {
        private void Start()
        {
            RotateTest();
        }

        private async UniTaskVoid RotateTest()
        {
            var count = 0;
            while (count <= 10)
            {
                await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
                transform.Rotate(new Vector3(0, 60, 0));
                count++;
            }
        }

        private void InstanceTest()
        {
            var originalData = ScriptableObject.CreateInstance<SharedData>();
            var sharedInt = new SharedVariable<int>();
            sharedInt.VariableName = "TestInt";
            sharedInt.Value = 42;
            originalData.AddVariable(sharedInt);

            var clonedData = originalData.Clone();

            Debug.Log("Original and Clone are different instances: " + !ReferenceEquals(originalData, clonedData));
            Debug.Log("Original and Clone variables are different instances: " +
                      !ReferenceEquals(originalData.Variables, clonedData.Variables));

            foreach (var variable in originalData.Variables)
            {
                var clonedVariable = clonedData.Variables.Find(v => v.VariableName == variable.VariableName);
                if (clonedVariable != null)
                {
                    Debug.Log($"Variable '{variable.VariableName}' is deep copied: " +
                              !ReferenceEquals(variable, clonedVariable));
                }
            }
        }

        private void CopyTest()
        {
            var original = new SharedInt { Value = 43, VariableName = "OriginalInt" };
            var cloneData = (SharedInt)original.Clone();

            original.Value = 10;
            original.VariableName = "10넣음";
            Debug.Log($"original {original.Value}  {original.VariableName}");
            Debug.Log($"copy  {cloneData.Value}  {cloneData.VariableName}");
        }
    }
}