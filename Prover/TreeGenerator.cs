using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prover.Tree;

namespace Prover
{
    class TreeGenerator
    {
        public static IEnumerable<Node> GenerateTrees(IEnumerable<char> variables)
        {
            Debug.Assert(variables.Count() > 0);
            // for now only guess temporaries with one or two variables
            //Debug.Assert(variables.Count() <= 2);

            return Trees(3 - variables.Count(), variables);
        }

        private static HashSet<Node> Trees(int depth, IEnumerable<char> variables)
        {
            if (depth <= 0)
            {
                var trees = new HashSet<Node>();
                foreach (var v in variables)
                    trees.Add(new Variable(v, false));
                return trees;
            } 
            else
            {
                var smaller = Trees(depth - 1, variables);
                HashSet<Node> trees = new HashSet<Node>(smaller);
                foreach (var n in smaller)
                {
                    trees.Add(new UnaryOperation(n, UnaryOperation.OP.Not));
                    if (variables.Count() > 1)
                    {
                        foreach (var n2 in smaller)
                        {
                            if (!n.Equals(n2))
                            {
                                trees.Add(new BinaryOperation(n, BinaryOperation.OP.And, n2));
                                trees.Add(new BinaryOperation(n, BinaryOperation.OP.Or, n2));
                                trees.Add(new BinaryOperation(n, BinaryOperation.OP.Implies, n2));
                            }
                        }
                    }
                }
                return trees;
            }
        }
    }
}
