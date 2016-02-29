using System.Collections.Generic;

namespace Prover.Tree
{
    sealed class Variable : Node
    {
        public char Letter { get; }
        public bool IsFixed { get; }

        public Variable(char letter, bool isFixed)
        {
            Letter = letter;
            IsFixed = isFixed;
        }
        
        public override void RegisterVariables(HashSet<char> variables)
        {
            variables.Add(Letter);
        }

        public override bool Evaluate(Dictionary<char, bool> variableValues)
        {
            return variableValues[Letter];
        }

        public override bool Match(Node other, Dictionary<char, Node> variableMappings)
        {
            if (IsFixed)
                return false;

            if (variableMappings.ContainsKey(Letter))
                return variableMappings[Letter].Equals(other);
            else
            {
                variableMappings.Add(Letter, other);
                return true;
            }
        }

        public override Node Map(Dictionary<char, Node> variableMappings)
        {
            if (!variableMappings.ContainsKey(Letter))
                return this;
            else
                return variableMappings[Letter];
        }

        public override int GetHashCode()
        {
            return Letter.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Variable other = obj as Variable;

            return other != null && Letter == other.Letter;
        }

        public override string ToString()
        {
            return Letter.ToString();
        }
    }
}
