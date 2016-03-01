using System.Collections.Generic;

namespace Prover.Tree
{
    sealed class Sequence
    {
        public IEnumerable<Node> Premises { get; }
        public IEnumerable<Node> Outcomes { get; }

        public Sequence(IEnumerable<Node> premises, IEnumerable<Node> outcomes)
        {
            Premises = premises;
            Outcomes = outcomes;
        }

        public Sequence(Node formula) : this(new Node[0], new Node[1] { formula })
        {

        }
    }
}
