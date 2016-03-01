using System.IO;
using System.Collections.Generic;
using Prover.Tree;

namespace Prover.ProofSteps
{
    sealed class StepFromAxiom : ProofStep
    {
        public Node Axiom { get; }
        public IReadOnlyDictionary<char, Node> Substitutions { get; }

        public StepFromAxiom(Node expression, Node axiom) : base(expression)
        {
            Dictionary<char, Node> substitutions = new Dictionary<char, Node>();
            axiom.Match(expression, substitutions);

            Axiom = axiom;
            Substitutions = substitutions;
        }

        public override int SetOrdering(int num)
        {
            Number = num;
            return num + 1;
        }

        public override void Print(TextWriter output)
        {
            output.WriteLine($"{StepNumber, -4}{ExpressionProven}");
            output.WriteLine($"      from axiom {Axiom}");
            output.WriteLine("      where");
            foreach (var subst in Substitutions)
                output.WriteLine($"        {subst.Key} = {subst.Value}");
        }
    }
}
