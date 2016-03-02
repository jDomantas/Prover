using System;
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

            // for rendering
            public int ColumnWidth;
            public int TextStartX;
            public int TextEndX;
            public string CachedString { get; }

            public ProofNode(Sequence sequence)
            {
                Sequence = sequence;
                Operation = OP.Unproved;
                Childs = new List<ProofNode>();

                CachedString = Sequence.ToString();
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

            public string OperationToString()
            {
                switch (Operation)
                {
                    case OP.Axiom: return "";
                    case OP.Unproved: return "";
                    case OP.LeftAnd: return "(& =>)";
                    case OP.LeftImplication: return "(-> =>)";
                    case OP.LeftNot: return "(~ =>)";
                    case OP.LeftOr: return "(V =>)";
                    case OP.RightAnd: return "(=> &)";
                    case OP.RightImplication: return "(=> ->)";
                    case OP.RightNot: return "(=> ~)";
                    case OP.RightOr: return "(=> V)";
                    default: throw new Exception($"invalid proof step operation: {Operation}");
                }
            }
        }
        
        public static void Prove(Sequence sequence, TextWriter output)
        {
            ProofNode sourceNode = new ProofNode(sequence);
            bool provable = Prove(sourceNode);
            if (provable)
                output.WriteLine("Provable");
            else
                output.WriteLine("Unprovable");

            CalculateColumns(sourceNode);
            PlaceText(sourceNode, 0);

            int depth = ProofDepth(sourceNode);

            for (int i = depth - 1; i >= 0; i--)
            {
                PrintAtDepth(sourceNode, output, i, 0);
                output.WriteLine();
                PrintSepparatorsAtDepth(sourceNode, output, i - 1, 0);
                output.WriteLine();
            }
        }

        private static int PrintAtDepth(ProofNode node, TextWriter output, int depth, int cursorX)
        {
            if (depth == 0)
            {
                while (cursorX < node.TextStartX)
                {
                    cursorX++;
                    output.Write(' ');
                }
                output.Write(node.CachedString);
                return cursorX + node.CachedString.Length;
            }
            else
            {
                for (int i = 0; i < node.Childs.Count; i++)
                    cursorX = PrintAtDepth(node.Childs[i], output, depth - 1, cursorX);

                return cursorX;
            }
        }

        private static int PrintSepparatorsAtDepth(ProofNode node, TextWriter output, int depth, int cursorX)
        {
            if (depth == 0 && node.Childs.Count > 0)
            {
                int startX = Math.Min(node.TextStartX, node.Childs.First().TextStartX);
                int endX = Math.Max(node.TextEndX, node.Childs.Last().TextEndX);
                while (cursorX < startX)
                {
                    cursorX++;
                    output.Write(' ');
                }
                while (cursorX <= endX)
                {
                    cursorX++;
                    output.Write('\u2014');
                }
                return cursorX;
            }
            else if (depth > 0)
            {
                for (int i = 0; i < node.Childs.Count; i++)
                    cursorX = PrintSepparatorsAtDepth(node.Childs[i], output, depth - 1, cursorX);

                return cursorX;
            }
            else
                return cursorX;
        }

        private static int ProofDepth(ProofNode node)
        {
            if (node.Childs.Count == 0)
                return 1;
            else
                return node.Childs.Max(child => ProofDepth(child)) + 1;
        }

        private static void CalculateColumns(ProofNode node)
        {
            foreach (var child in node.Childs)
                CalculateColumns(child);

            node.ColumnWidth = Math.Max(
                node.CachedString.Length,
                (node.Childs.Count - 1) * 5 // (n - 1) sepparating columns
                    + node.Childs.Sum(child => child.ColumnWidth)); // and each childs column
        }

        private static void PlaceText(ProofNode node, int startX)
        {
            int childWidths = node.Childs.Sum(child => child.ColumnWidth);
            int gapSpace = node.ColumnWidth - childWidths;
            
            node.TextStartX = startX + (node.ColumnWidth - node.CachedString.Length) / 2;
            node.TextEndX = node.TextStartX + node.CachedString.Length - 1;

            int currentWidthSum = 0;
            int divideGroups = node.Childs.Count < 2 ? 2 : node.Childs.Count - 1;
            int groupNumber = node.Childs.Count < 2 ? 1 : 0;

            for (int i = 0; i < node.Childs.Count; i++)
            {
                int x = (int)Math.Round((double)(gapSpace * groupNumber) / divideGroups);
                PlaceText(node.Childs[i], startX + x + currentWidthSum);
                currentWidthSum += node.Childs[i].ColumnWidth;
                groupNumber++;
            }
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
                    {
                        // destroy trees of other childs
                        foreach (var c in node.Childs)
                            if (c != child)
                                c.Childs.Clear();
                        return false;
                    }

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
