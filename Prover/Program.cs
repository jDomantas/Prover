using System;
using Prover.Tree;

namespace Prover
{
    class Program
    {
        static void Main(string[] args)
        {
            CLI();
        }

        static void CLI()
        {
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                Process(input);
            }
        }

        public static void Process(string input)
        {
            Node expression;

            try
            {
                expression = Parser.ParseString(input);
            }
            catch (Parser.ParseException e)
            {
                Console.WriteLine(input.Replace('\t', ' '));
                for (int i = 0; i < e.Position; i++) Console.Write(' ');
                Console.WriteLine("^");
                for (int i = 0; i < e.Position; i++) Console.Write(' ');
                Console.WriteLine($"Error: {e.Message}");
                return;
            }

            var invalid = Validator.FindInvalidInterpretation(expression);
            if (invalid != null)
            {
                Console.WriteLine("Expression is false with interpretation:");
                foreach (var pair in invalid)
                    Console.Write($"{pair.Key} = {pair.Value}, ");
                Console.WriteLine();
                return;
            }

            Hilbert.HilbertProver.Prove(new Sequence(expression), Console.Out);
        }
    }
}
