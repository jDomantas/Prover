using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prover.Tree;
using System.Diagnostics;

namespace Prover.Sequential
{
    class SequentialProver
    {
        private class ProofNode
        {
            public enum OP { Unproved, LeftAnd, RightAnd, LeftOr, RightOr, LeftImplication, RightImplication, LeftNot, RightNot, Axiom }

            public Sequence Sequence { get; }
            public List<ProofNode> Childs { get; }
            public OP Operation { get; private set; }

            public ProofNode(Sequence sequence)
            {
                Sequence = sequence;
                Operation = OP.Unproved;
                Childs = new List<ProofNode>();
            }

            public void Prove(OP operation, IEnumerable<ProofNode> childs = null)
            {
                Operation = operation;

                if (childs != null)
                    foreach (var child in childs)
                        Childs.Add(child);
            }

            public void Prove(OP operation, params ProofNode[] childs)
            {
                // force calling the other overload
                Prove(operation, (IEnumerable<ProofNode>)childs);
            }
        }
        
        public static void Prove(Sequence sequence, TextWriter output)
        {
            ProofNode sourceNode = new ProofNode(sequence);
            if (Prove(sourceNode))
                output.WriteLine($"Provable: {sequence.ToString()}");
            else
                output.WriteLine($"Unprovable: {sequence.ToString()}");
        }

        private static bool Prove(ProofNode node)
        {
            foreach (var premise in node.Sequence.Premises)
            {
                if (node.Sequence.Outcomes.Any(o => premise == o))
                {
                    node.Prove(ProofNode.OP.Axiom);
                    return true;
                }
            }

            if (ProveWithLeftOperation(node) || ProveWithRightOperation(node))
            {
                foreach (var child in node.Childs)
                    if (!Prove(child))
                        return false;

                return true;
            }
            
            return false;
        }

        private static bool ProveWithLeftOperation(ProofNode node)
        {
            foreach (var premise in node.Sequence.Premises)
            {
                bool flag = false;
                List<Node> filteredPremises = node.Sequence.Premises.Where(p => flag || (premise != p || !(flag = true))).ToList();
                if (premise is UnaryOperation)
                {
                    var newPremises = filteredPremises;
                    var newOutcomes = new List<Node>(node.Sequence.Outcomes);
                    newOutcomes.Add((premise as UnaryOperation).Inner);

                    node.Prove(ProofNode.OP.LeftNot, new ProofNode(new Sequence(newPremises, newOutcomes)));
                    return true;
                }
                else if (premise is BinaryOperation)
                {
                    BinaryOperation binary = premise as BinaryOperation;
                    
                    if (binary.Operation == BinaryOperation.OP.And)
                    {
                        // split A & B to A, B
                        var newPremises = filteredPremises;
                        var newOutcomes = node.Sequence.Outcomes;

                        newPremises.Add(binary.LHS);
                        newPremises.Add(binary.RHS);

                        node.Prove(ProofNode.OP.LeftAnd, new ProofNode(new Sequence(newPremises, newOutcomes)));
                    }
                    else if (binary.Operation == BinaryOperation.OP.Or)
                    {
                        // with A v B separate A and B to different childs
                        var newPremises1 = filteredPremises;
                        var newPremises2 = new List<Node>(filteredPremises);
                        var newOutcomes = node.Sequence.Outcomes;

                        newPremises1.Add(binary.LHS);
                        newPremises2.Add(binary.RHS);

                        node.Prove(ProofNode.OP.LeftAnd,
                            new ProofNode(new Sequence(newPremises1, newOutcomes)),
                            new ProofNode(new Sequence(newPremises2, newOutcomes)));
                    }
                    else if (binary.Operation == BinaryOperation.OP.Implies)
                    {
                        // with A -> B, move A to the right of the first and B to the left of the second child
                        var newPremises1 = filteredPremises;
                        var newPremises2 = new List<Node>(filteredPremises);
                        var newOutcomes1 = new List<Node>(node.Sequence.Outcomes);
                        var newOutcomes2 = node.Sequence.Outcomes;

                        newOutcomes1.Add(binary.LHS);
                        newPremises2.Add(binary.RHS);

                        node.Prove(ProofNode.OP.LeftImplication,
                            new ProofNode(new Sequence(newPremises1, newOutcomes1)),
                            new ProofNode(new Sequence(newPremises2, newOutcomes2)));
                    }
                    else
                        Debug.Assert(false, $"invalid binary operation: {binary.Operation}");

                    return true;
                }
            }

            return false;
        }

        private static bool ProveWithRightOperation(ProofNode node)
        {
            foreach (var outcome in node.Sequence.Outcomes)
            {
                bool flag = false;
                var filteredOutcomes = node.Sequence.Outcomes.Where(o => flag || (outcome != o || !(flag = true))).ToList();
                if (outcome is UnaryOperation)
                {
                    var newPremises = new List<Node>(node.Sequence.Premises);
                    var newOutcomes = filteredOutcomes;
                    newPremises.Add((outcome as UnaryOperation).Inner);

                    node.Prove(ProofNode.OP.RightNot, new ProofNode(new Sequence(newPremises, newOutcomes)));
                    return true;
                }
                else if (outcome is BinaryOperation)
                {
                    BinaryOperation binary = outcome as BinaryOperation;
                    
                    if (binary.Operation == BinaryOperation.OP.And)
                    {
                        // split A & B to different childs
                        var newPremises = node.Sequence.Premises;
                        var newOutcomes1 = filteredOutcomes;
                        var newOutcomes2 = new List<Node>(filteredOutcomes);

                        newOutcomes1.Add(binary.LHS);
                        newOutcomes2.Add(binary.RHS);

                        node.Prove(ProofNode.OP.RightAnd, 
                            new ProofNode(new Sequence(newPremises, newOutcomes1)),
                            new ProofNode(new Sequence(newPremises, newOutcomes2)));
                    }
                    else if (binary.Operation == BinaryOperation.OP.Or)
                    {
                        // split A v B to A, B
                        var newPremises = node.Sequence.Premises;
                        var newOutcomes = filteredOutcomes;

                        newOutcomes.Add(binary.LHS);
                        newOutcomes.Add(binary.RHS);

                        node.Prove(ProofNode.OP.RightOr,
                            new ProofNode(new Sequence(newPremises, newOutcomes)));
                    }
                    else if (binary.Operation == BinaryOperation.OP.Implies)
                    {
                        // change A -> B to A |- B (deduction theorem)
                        var newPremises = new List<Node>(node.Sequence.Premises);
                        var newOutcomes = filteredOutcomes;

                        newPremises.Add(binary.LHS);
                        newOutcomes.Add(binary.RHS);

                        node.Prove(ProofNode.OP.RightImplication,
                            new ProofNode(new Sequence(newPremises, newOutcomes)));
                    }
                    else
                        Debug.Assert(false, $"invalid binary operation: {binary.Operation}");

                    return true;
                }
            }

            return false;
        }
    }
}
