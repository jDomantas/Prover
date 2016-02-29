using Prover.Tree;

namespace Prover.ProofSteps
{
    abstract class ProofStep
    {
        public Node ExpressionProven { get; }
        public int Number { get; protected set; }
        protected string StepNumber { get { return Number.ToString() + "."; } }

        protected ProofStep(Node expressionProven)
        {
            ExpressionProven = expressionProven;
        }

        public abstract int SetOrdering(int num);

        public abstract void Print();
    }
}
