using System;

namespace Tree
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeCategoryAttribute : Attribute
    {
        public string Category { get; }

        public NodeCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}