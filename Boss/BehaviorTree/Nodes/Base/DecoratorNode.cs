using UnityEngine;

namespace BirdCase
{
    public abstract class DecoratorNode : Node
    {
        [HideInInspector] public Node child;

        public override Node Clone()
        {
            if (child == null)
            {
                return null;
            }

            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            node.name = node.name.Replace("(Clone)", "");
            return node;
        }
    }
}