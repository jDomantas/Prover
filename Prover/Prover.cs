using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prover.ProofSteps;
using Prover.Tree;

namespace Prover
{
    class Prover
    {
        // that symbol (tee, turnstile, yields, whatever it is called): \u22A2
        // but it doesn't get printed to the console properly

        [Serializable]
        public class ProofException : Exception
        {
            public ProofException() { }
            public ProofException(string message) : base(message) { }
            public ProofException(string message, Exception inner) : base(message, inner) { }
            protected ProofException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context)
            { }
        }

        public static void Prove(Node expression)
        {
            const int stepLimit = 10;

            Prover prover = new Prover(expression);
            int ticks = 0;
            Stopwatch timer = Stopwatch.StartNew();
            while (!prover.IsProven(expression))
            {
                prover.ExpandTargets();
                ticks++;
                if (ticks > stepLimit)
                {
                    Console.WriteLine($"Failed to find the proof in {stepLimit} steps");
                    return;
                }
            }
            Console.WriteLine($"Time: {timer.ElapsedMilliseconds} ms, steps: {ticks}");

            prover.NodeProofs[expression].SetOrdering(1);

            var steps = prover.NodeProofs.Values
                .Where(step => step.Number != 0) // select only used steps
                .OrderBy(step => step.Number); // and sort them by their step number

            foreach (var step in steps)
            {
                step.Print();
                Console.WriteLine();
            }
        }

        private static void InsertTo(Node expression, Node into, Node whole)
        {
            Dictionary<char, Node> mapping = new Dictionary<char, Node>();
            if (into.Match(expression, mapping))
            {
                Console.WriteLine(whole.Map(mapping));
                Console.WriteLine($"from {whole}");
                Console.WriteLine("  where:");
                foreach (var map in mapping)
                    Console.WriteLine($"    {map.Key} = {map.Value}");
                Console.WriteLine();
            }

            BinaryOperation binary = into as BinaryOperation;
            if (binary != null && binary.Operation == BinaryOperation.OP.Implies)
                InsertTo(expression, binary.RHS, whole);
        }

        private Dictionary<Node, ProofStep> NodeProofs { get; }
        private Dictionary<Node, HashSet<Node>> Targets { get; }
        private Node MainTarget { get; }
        
        private Prover(Node expression)
        {
            MainTarget = expression;

            NodeProofs = new Dictionary<Node, ProofStep>();
            Targets = new Dictionary<Node, HashSet<Node>>();

            BinaryOperation placeholder = new BinaryOperation(expression, BinaryOperation.OP.Implies, expression);
            AddTarget(expression, placeholder);
        }

        private bool IsProven(Node expression)
        {
            return NodeProofs.ContainsKey(expression);
        }

        private void ExpandTargets()
        {
            List<Node> targets = new List<Node>(Targets.Keys);

            foreach (var target in targets)
            {
                foreach (var axiom in Axioms.AxiomList)
                    AddMiddleTargets(target, axiom);
            }
        }

        private void OnProvenFormula(Node expression, ProofStep step)
        {
            if (IsProven(expression))
                return;

            NodeProofs.Add(expression, step);
            if (Targets.ContainsKey(expression))
            {
                foreach (var usage in Targets[expression])
                    DoModusPonens(expression, usage);

                Targets[expression].RemoveWhere(e => IsProven(e));

                if (Targets[expression].Count == 0)
                    Targets.Remove(expression);
            }
        }

        private void AddTarget(Node expression, Node useIn)
        {
            if (IsProven(expression))
            {
                DoModusPonens(expression, useIn);
            }
            else if (!Targets.ContainsKey(expression))
            {
                HashSet<Node> usages = new HashSet<Node>();
                usages.Add(useIn);
                Targets.Add(expression, usages);
            }
            else
            {
                Targets[expression].Add(useIn);
            }
        }

        private void DoModusPonens(Node source, Node useIn)
        {
            if (!IsProven(source) || !IsProven(useIn))
                return;

            BinaryOperation binary = useIn as BinaryOperation;
            Debug.Assert(binary != null && binary.Operation == BinaryOperation.OP.Implies, $"invalid use of modus ponens");

            Node fromModusPonens = binary.RHS;
            if (IsProven(fromModusPonens))
                return;

            OnProvenFormula(fromModusPonens, new StepModusPonens(fromModusPonens, NodeProofs[source], NodeProofs[useIn]));
        }
        
        private void AddMiddleTargets(Node expression, Node axiomToUse)
        {
            if (IsProven(expression))
                return;

            if (axiomToUse.Match(expression, new Dictionary<char, Node>()))
            {
                OnProvenFormula(expression, new StepFromAxiom(expression, axiomToUse));
                return;
            }

            HashSet<char> variablesInAxiom = new HashSet<char>();
            axiomToUse.RegisterVariables(variablesInAxiom);
            int variableCount = variablesInAxiom.Count;

            HashSet<char> variablesInExpression = new HashSet<char>();
            expression.RegisterVariables(variablesInExpression);

            Node currentSubpart = axiomToUse;
            int steps = 0;
            while (true)
            {
                BinaryOperation binary = currentSubpart as BinaryOperation;
                if (binary == null || binary.Operation != BinaryOperation.OP.Implies)
                    break;

                steps++;

                // try matching expression to the right (result) side of implication
                // and set target for it if it matches

                var mapping = new Dictionary<char, Node>();
                if (binary.RHS.Match(expression, mapping))
                {
                    if (mapping.Count == variableCount - 1)
                    {
                        // have to guess one variable
                        char variable = variablesInAxiom.Where(v => !mapping.ContainsKey(v)).First();
                        mapping.Add(variable, null);
                        foreach (var tree in TreeGenerator.GenerateTrees(variablesInExpression))
                        {
                            // replace guessed variable with given tree and then do the same
                            mapping[variable] = tree;

                            var mappedAxiom = axiomToUse.Map(mapping);
                            OnProvenFormula(mappedAxiom, new StepFromAxiom(mappedAxiom, axiomToUse));
                            AddImplicationSteps(mappedAxiom, steps);
                        }
                    }
                    else if (mapping.Count < variableCount)
                    {
                        // have to guess at least two variables
                        break;
                    }
                    else
                    {
                        var mappedAxiom = axiomToUse.Map(mapping);
                        OnProvenFormula(mappedAxiom, new StepFromAxiom(mappedAxiom, axiomToUse));
                        AddImplicationSteps(mappedAxiom, steps);
                    }
                }

                currentSubpart = binary.RHS;
            }
        }

        private void AddImplicationSteps(Node start, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                BinaryOperation binary = (BinaryOperation)start;
                Debug.Assert(binary.Operation == BinaryOperation.OP.Implies);
                AddTarget(binary.LHS, binary);
                start = binary.RHS;
            }
        }
    }
}
