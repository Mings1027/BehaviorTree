using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/External Behavior Tree")]
public class ExternalBehaviorTree : BehaviorTree
{
    // public ExternalBehaviorTree Clone(Transform transform, List<SharedVariableBase> variables)
    // {
    //     var tree = Instantiate(this);
    //     tree.rootNode = tree.rootNode.Clone();
    //     tree.nodes = new List<Node>();
    //     tree.sharedData = rootNode.SharedData.Clone();
    //     Traverse(tree.rootNode, n =>
    //     {
    //         tree.nodes.Add(n);
    //         n.SetData(transform, tree.sharedData);
    //     });
    //     AssignSharedVariables(tree.nodes);
    //     Traverse(tree.rootNode, n => n.Init());
    //
    //     return tree;
    // }
}