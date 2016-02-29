using System;
using System.Collections.Generic;
using Prover.Tree;

namespace Prover
{
    class Validator
    {
        public static Dictionary<char, bool> FindInvalidInterpretation(Node expression)
        {
            HashSet<char> variableSet = new HashSet<char>();
            expression.RegisterVariables(variableSet);

            if (variableSet.Count > 10)
                throw new Exception($"expression has too many variables: {variableSet.Count} (max 10 supported)");

            List<char> variables = new List<char>(variableSet);
            Dictionary<char, bool> values = new Dictionary<char, bool>();
            foreach (var v in variables)
                values.Add(v, false);

            for (int i = (1 << variables.Count) - 1; i >= 0; i--)
            {
                for (int j = 0; j < variables.Count; j++)
                    values[variables[j]] = ((i >> j) & 1) == 1;

                bool result = expression.Evaluate(values);
                if (!result)
                    return values;
            }

            return null;
        }
    }
}
