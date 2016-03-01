using System.IO;
using Prover.Tree;

namespace Prover.ProofSteps
{
    sealed class StepFromPremise : ProofStep
    {
        public StepFromPremise(Node expression) : base(expression)
        {

        }

        public override int SetOrdering(int num)
        {
            Number = num;
            return num + 1;
        }

        public override void Print(TextWriter output)
        {
            output.WriteLine($"{StepNumber, -4}{ExpressionProven}");
            output.WriteLine("      from premise");
        }
    }
}
