using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prover.Tree
{
    sealed class Sequence
    {
        public IList<Node> Premises { get; }
        public IList<Node> Outcomes { get; }

        public Sequence(IEnumerable<Node> premises, IEnumerable<Node> outcomes)
        {
            Premises = premises.ToList();
            Outcomes = outcomes.ToList();
        }

        public Sequence(Node formula) : this(new Node[0], new Node[1] { formula })
        {

        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            if (Premises.Count() > 0)
            {
                strBuilder.Append(Premises.First().ToString());
                foreach (var premise in Premises.Skip(1))
                {
                    strBuilder.Append(", ");
                    strBuilder.Append(premise.ToString());
                }

                strBuilder.Append(" ");
            }

            strBuilder.Append("=>");

            if (Outcomes.Count() > 0)
            {
                strBuilder.Append(" ");
                strBuilder.Append(Outcomes.First().ToString());
                foreach (var outcome in Outcomes.Skip(1))
                {
                    strBuilder.Append(", ");
                    strBuilder.Append(outcome);
                }
            }

            return strBuilder.ToString();
        }
    }
}
