using System.Collections.Generic;

namespace Prover.Tree
{
    sealed class UnaryOperation : Node
    {
        public enum OP { Not }

        public Node Inner { get; }
        public OP Operation { get; }

        private readonly int _cachedHashCode;

        public UnaryOperation(Node inner, OP op)
        {
            Inner = inner;
            Operation = op;

            // expression tree is immutable, so we can cache hash code so
            // that subsequent calls would not require walking the whole tree
            int hash = 17;
            hash = hash * 31 + Inner.GetHashCode();
            hash = hash * 31 + Operation.GetHashCode();
            _cachedHashCode = hash;
        }

        public override void RegisterVariables(HashSet<char> variables)
        {
            Inner.RegisterVariables(variables);
        }

        public override bool Evaluate(Dictionary<char, bool> variableValues)
        {
            return !Inner.Evaluate(variableValues);
        }

        public override bool Match(Node other, Dictionary<char, Node> variableMappings)
        {
            UnaryOperation o = other as UnaryOperation;
            if (o == null || o.Operation != Operation)
                return false;

            return Inner.Match(o.Inner, variableMappings);
        }

        public override Node Map(Dictionary<char, Node> variableMappings)
        {
            return new UnaryOperation(Inner.Map(variableMappings), Operation);
        }

        public override int GetHashCode()
        {
            return _cachedHashCode;
        }

        public override bool Equals(object obj)
        {
            UnaryOperation other = obj as UnaryOperation;

            return other != null && other._cachedHashCode == _cachedHashCode && Inner.Equals(other.Inner) && Operation == other.Operation;
        }

        public override string ToString()
        {
            if (Inner is BinaryOperation)
                return $"!({Inner})";
            else
                return $"!{Inner}";
        }
    }
}
