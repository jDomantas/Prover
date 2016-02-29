using System;
using Prover.Tree;

namespace Prover.ProofSteps
{
    sealed class StepModusPonens : ProofStep
    {
        public ProofStep StepA { get; }
        public ProofStep StepAimpliesB { get; }

        public StepModusPonens(Node expression, ProofStep stepA, ProofStep AimpliesB) : base(expression)
        {
            StepA = stepA;
            StepAimpliesB = AimpliesB;
        }

        public override int SetOrdering(int num)
        {
            num = StepA.SetOrdering(num);
            num = StepAimpliesB.SetOrdering(num);
            Number = num;
            return num + 1;
        }

        public override void Print()
        {
            Console.WriteLine($"{StepNumber, -4}{ExpressionProven}");
            Console.WriteLine($"      modus ponens, from steps {StepA.Number} and {StepAimpliesB.Number}");
        }
    }
}
