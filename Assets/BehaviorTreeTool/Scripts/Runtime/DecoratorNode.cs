using UnityEngine;

public abstract class DecoratorNode : Node
{
    public Node Child
    {
        get => child;
        set => child = value;
    }

    [HideInInspector, SerializeField] protected Node child;

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}