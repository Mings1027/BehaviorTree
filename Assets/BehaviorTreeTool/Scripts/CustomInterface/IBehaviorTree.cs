namespace BehaviorTreeTool.Scripts.CustomInterface
{
    public interface IObject
    {
        bool UseGetComponent { get; set; }
    }

    public interface IBehaviorTree
    {
        void TreeUpdate();
    }
}