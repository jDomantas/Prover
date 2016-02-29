using System.Collections.Generic;
using System.Diagnostics;
using Prover.Tree;

namespace Prover
{
    class Axioms
    {
        public static IEnumerable<Node> AxiomList { get; } = CreateAxioms();
        
        private static IEnumerable<Node> CreateAxioms()
        {
            var axiomList = new List<Node>();
            axiomList.Add(CreateAxiom11());
            axiomList.Add(CreateAxiom12());
            axiomList.Add(CreateAxiom21());
            axiomList.Add(CreateAxiom22());
            axiomList.Add(CreateAxiom23());
            axiomList.Add(CreateAxiom31());
            axiomList.Add(CreateAxiom32());
            axiomList.Add(CreateAxiom33());
            axiomList.Add(CreateAxiom41());
            axiomList.Add(CreateAxiom42());
            axiomList.Add(CreateAxiom43());

            foreach (var axiom in axiomList)
                Debug.Assert(Validator.FindInvalidInterpretation(axiom) == null, $"axiom ({axiom}) is invalid");

            return axiomList;
        }

        #region AxiomBuilders

        /// <summary>
        /// A -> (B -> A)
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom11()
        {
            return new BinaryOperation(
                new Variable('A', false),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new Variable('B', false),
                    BinaryOperation.OP.Implies,
                    new Variable('A', false)));
        }

        /// <summary>
        /// (A -> (B -> C)) -> ((A -> B) -> (A -> C))
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom12()
        {
            return new BinaryOperation(
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.Implies,
                    new BinaryOperation(
                        new Variable('B', false),
                        BinaryOperation.OP.Implies,
                        new Variable('C', false)
                    )),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new BinaryOperation(
                        new Variable('A', false),
                        BinaryOperation.OP.Implies,
                        new Variable('B', false)),
                    BinaryOperation.OP.Implies,
                    new BinaryOperation(
                        new Variable('A', false),
                        BinaryOperation.OP.Implies,
                        new Variable('C', false))));
        }

        /// <summary>
        /// (A & B) -> A
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom21()
        {
            return new BinaryOperation(
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.And,
                    new Variable('B', false)),
                BinaryOperation.OP.Implies,
                new Variable('A', false));
        }

        /// <summary>
        /// (A & B) -> B
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom22()
        {
            return new BinaryOperation(
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.And,
                    new Variable('B', false)),
                BinaryOperation.OP.Implies,
                new Variable('B', false));
        }

        /// <summary>
        /// (A -> B) -> ((A -> C) -> (A -> (B & C)))
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom23()
        {
            return new BinaryOperation(
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.Implies,
                    new Variable('B', false)),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new BinaryOperation(
                        new Variable('A', false),
                        BinaryOperation.OP.Implies,
                        new Variable('C', false)),
                    BinaryOperation.OP.Implies,
                    new BinaryOperation(
                        new Variable('A', false),
                        BinaryOperation.OP.Implies,
                        new BinaryOperation(
                            new Variable('B', false),
                            BinaryOperation.OP.And,
                            new Variable('C', false)))));
        }

        /// <summary>
        /// A -> (A | B)
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom31()
        {
            return new BinaryOperation(
                new Variable('A', false),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.Or,
                    new Variable('B', false)));
        }

        /// <summary>
        /// B -> (A | B)
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom32()
        {
            return new BinaryOperation(
                new Variable('B', false),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.Or,
                    new Variable('B', false)));
        }

        /// <summary>
        /// (A -> C) -> ((B -> C) -> ((A | B) -> C))
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom33()
        {
            return new BinaryOperation(
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.Implies,
                    new Variable('C', false)),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new BinaryOperation(
                        new Variable('B', false),
                        BinaryOperation.OP.Implies,
                        new Variable('C', false)),
                    BinaryOperation.OP.Implies,
                    new BinaryOperation(
                        new BinaryOperation(
                            new Variable('A', false),
                            BinaryOperation.OP.Or,
                            new Variable('B', false)),
                        BinaryOperation.OP.Implies,
                        new Variable('C', false))));
        }

        /// <summary>
        /// (A -> B) -> (!B -> !A)
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom41()
        {
            return new BinaryOperation(
                new BinaryOperation(
                    new Variable('A', false),
                    BinaryOperation.OP.Implies,
                    new Variable('B', false)),
                BinaryOperation.OP.Implies,
                new BinaryOperation(
                    new UnaryOperation(new Variable('B', false), UnaryOperation.OP.Not),
                    BinaryOperation.OP.Implies,
                    new UnaryOperation(new Variable('A', false), UnaryOperation.OP.Not)));
        }

        /// <summary>
        /// A -> !!A
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom42()
        {
            return new BinaryOperation(
                new Variable('A', false),
                BinaryOperation.OP.Implies,
                new UnaryOperation(new UnaryOperation(new Variable('A', false), UnaryOperation.OP.Not), UnaryOperation.OP.Not));
        }

        /// <summary>
        /// !!A -> A
        /// </summary>
        /// <returns></returns>
        private static Node CreateAxiom43()
        {
            return new BinaryOperation(
                new UnaryOperation(new UnaryOperation(new Variable('A', false), UnaryOperation.OP.Not), UnaryOperation.OP.Not),
                BinaryOperation.OP.Implies,
                new Variable('A', false));
        }

        #endregion
    }
}
