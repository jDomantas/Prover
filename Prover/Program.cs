using System;
using System.IO;
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
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                Process(input, Console.Out, Sequential.SequentialProver.Prove);
            }
        }

        public static void Process(string input, TextWriter output, Action<Sequence, TextWriter> prover)
        {
            Sequence sequence;

            try
            {
                sequence = Parser.ParseSequence(input);
            }
            catch (Parser.ParseException e)
            {
                output.WriteLine(input.Replace('\t', ' '));
                for (int i = 0; i < e.Position; i++) Console.Write(' ');
                output.WriteLine("^");
                for (int i = 0; i < e.Position; i++) Console.Write(' ');
                output.WriteLine($"Error: {e.Message}");
                return;
            }

            /*var invalid = Validator.FindInvalidInterpretation(sequence);
            if (invalid != null)
            {
                output.WriteLine("Expression is false with interpretation:");
                foreach (var pair in invalid)
                    output.Write($"{pair.Key} = {pair.Value}, ");
                output.WriteLine();
                return;
            }*/

            prover(sequence, output);
        }
    }
}
