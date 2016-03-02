using System;
using System.Collections.Generic;

namespace Prover.Tree
{
    sealed class BinaryOperation : Node
    {
        public enum OP { Or, And, Implies }

        public Node LHS { get; }
        public Node RHS { get; }
        public OP Operation { get; }

        private readonly int _cachedHashCode;

        public BinaryOperation(Node lhs, Node rhs, OP op)
        {
            LHS = lhs;
            RHS = rhs;
            Operation = op;

            // expression tree is immutable, so we can cache hash code so
            // that subsequent calls would not require walking the whole tree
            int hash = 17;
            hash = hash * 31 + LHS.GetHashCode();
            hash = hash * 31 + Operation.GetHashCode();
            hash = hash * 31 + RHS.GetHashCode();
            _cachedHashCode = hash;
        }

        public BinaryOperation(Node lhs, OP op, Node rhs) : this(lhs, rhs, op)
        {

        }

        public override void RegisterVariables(HashSet<char> variables)
        {
            LHS.RegisterVariables(variables);
            RHS.RegisterVariables(variables);
        }

        public override bool Evaluate(Dictionary<char, bool> variableValues)
        {
            switch (Operation)
            {
                case OP.And: return LHS.Evaluate(variableValues) && RHS.Evaluate(variableValues);
                case OP.Or: return LHS.Evaluate(variableValues) || RHS.Evaluate(variableValues);
                case OP.Implies: return !LHS.Evaluate(variableValues) || RHS.Evaluate(variableValues);
                default: throw new Exception($"invalid binary operator: {Operation}");
            }
        }

        public override bool Match(Node other, Dictionary<char, Node> variableMappings)
        {
            BinaryOperation o = other as BinaryOperation;
            if (o == null || o.Operation != Operation)
                return false;

            return LHS.Match(o.LHS, variableMappings) && RHS.Match(o.RHS, variableMappings);
        }

        public override Node Map(Dictionary<char, Node> variableMappings)
        {
            return new BinaryOperation(LHS.Map(variableMappings), RHS.Map(variableMappings), Operation);
        }

        public override int GetHashCode()
        {
            return _cachedHashCode;
        }

        public override bool Equals(object obj)
        {
            BinaryOperation other = obj as BinaryOperation;

            return other != null && other._cachedHashCode == _cachedHashCode && LHS.Equals(other.LHS) && RHS.Equals(other.RHS) && Operation == other.Operation;
        }

        public override string ToString()
        {
            string lhs = (LHS is BinaryOperation ? $"({LHS})" : LHS.ToString());
            string rhs = (RHS is BinaryOperation ? $"({RHS})" : RHS.ToString());

            switch (Operation)
            {
                case OP.And:
                    return $"{lhs} & {rhs}";
                case OP.Implies:
                    return $"{lhs} -> {rhs}";
                case OP.Or:
                    return $"{lhs} V {rhs}";
                default:
                    return $"{lhs} ? {rhs}";
            }
        }
    }
}
