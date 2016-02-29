using System.Collections.Generic;

namespace Prover.Tree
{
    abstract class Node
    {
        public abstract bool Evaluate(Dictionary<char, bool> variableValues);

        public abstract void RegisterVariables(HashSet<char> variables);

        public abstract bool Match(Node other, Dictionary<char, Node> variableMappings);

        public abstract Node Map(Dictionary<char, Node> variableMappings);

        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException("bool Equals(object obj) must be overriden in child class");
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException("int GetHashCode() must be overriden in child class");
        }

        public static bool operator ==(Node a, Node b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Node a, Node b)
        {
            return !(a == b);
        }
    }
}
