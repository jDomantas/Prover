using System;
using System.Collections.Generic;
using Prover.Tree;

namespace Prover
{
    class Parser
    {
        /* Grammar, in EBNF (more or less accurate to what the code parses):
         *  
         *  Variable    = ? all letters except V (both "v" and "V") ?
         *  Not         = "!" | "~"
         *  And         = "&"
         *  Or          = "|" | "v" | "V"
         *  Implies     = "->"
         *  Unit        = (Not, Unit) | Variable | ("(", Node, ")")
         *  Binary      = Unit, (And | Or | Implies), Unit
         *  Node        = Unit | Binary
         *
         * The whole parser input must be a valid instance of Node
         */

        private enum TokenType { OpenParenth, CloseParenth, Variable, And, Or, Implies, Not, EndOfInput }
        
        [Serializable]
        public class ParseException : Exception
        {
            private readonly int position;
            public int Position { get { return position; } }

            public ParseException(string message, int position) : base(message)
            {
                this.position = position;
            }

            public ParseException() { }
            public ParseException(string message) : base(message) { }
            public ParseException(string message, Exception inner) : base(message, inner) { }
            protected ParseException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context)
            {
                position = info.GetInt32("position");
            }
        }

        private class Token
        {
            public TokenType Type { get; }
            public int Position { get; }
            public char Letter { get; }
            
            public Token(TokenType type, int position)
            {
                Type = type;
                Position = position;
                Letter = (char)0;
            }

            public Token(int position, char letter)
            {
                Type = TokenType.Variable;
                Position = position;
                Letter = letter;
            }
        }

        public static Node ParseString(string str)
        {
            Parser parser = new Parser(str);
            Node result = parser.ReadNode();
            if (parser.Peek().Type != TokenType.EndOfInput)
                throw ExceptionAtToken("expected input to be over", parser.Peek());

            return result;
        }
        
        private static ParseException ExceptionAtToken(string message, Token token)
        {
            return new ParseException(message, token.Position);
        }

        private string Input { get; }
        private List<Token> Tokens;
        private int ParsePosition;

        private Parser(string str)
        {
            Input = str;
            Tokens = new List<Token>();

            Tokenize();
            ParsePosition = 0;
        }

        private void Tokenize()
        {
            for (int i = 0; i < Input.Length; i++)
            {
                if (char.IsWhiteSpace(Input[i]))
                    continue;
                if (Input[i] == 'v' || Input[i] == 'V' || Input[i] == '|')
                    Tokens.Add(new Token(TokenType.Or, i));
                else if (Input[i] == '!' || Input[i] == '~')
                    Tokens.Add(new Token(TokenType.Not, i));
                else if (Input[i] == '&')
                    Tokens.Add(new Token(TokenType.And, i));
                else if (Input[i] == '(')
                    Tokens.Add(new Token(TokenType.OpenParenth, i));
                else if (Input[i] == ')')
                    Tokens.Add(new Token(TokenType.CloseParenth, i));
                else if (char.IsLetter(Input[i]))
                {
                    // this does throw on input like AvB (which is arguably valid)
                    // but I don't care enough to fix it (also because avb doesn't look valid)
                    if (i < Input.Length - 1 && char.IsLetter(Input[i + 1]))
                        throw new ParseException("only one letter names are supported", i + 1);
                    Tokens.Add(new Token(i, Input[i]));
                }
                else if (i < Input.Length - 1 && Input[i] == '-' && Input[i + 1] == '>')
                {
                    Tokens.Add(new Token(TokenType.Implies, i));
                    i++;
                }
                else
                {
                    throw new ParseException("unrecognized symbol", i);
                }
            }

            Tokens.Add(new Token(TokenType.EndOfInput, Input.Length));
        }

        private Token Peek()
        {
            if (ParsePosition == Tokens.Count)
                return null;
            else
                return Tokens[ParsePosition];
        }

        private Token Consume()
        {
            if (ParsePosition == Tokens.Count)
                return null;
            else
                return Tokens[ParsePosition++];
        }

        private Node ReadEnclosed()
        {
            if (Peek().Type != TokenType.OpenParenth)
                throw ExceptionAtToken("expected (", Peek());
            Consume();

            Node result = ReadNode();

            if (Peek().Type != TokenType.CloseParenth)
            {
                if (result is BinaryOperation)
                    throw ExceptionAtToken("expected )", Peek());
                else
                    throw ExceptionAtToken("expected ), |, &, or ->", Peek());
            }
            Consume();

            return result;
        }

        private Node ReadUnit()
        {
            if (Peek().Type == TokenType.Not)
            {
                Consume();
                return new UnaryOperation(ReadUnit(), UnaryOperation.OP.Not);
            }
            else if (Peek().Type == TokenType.Variable)
            {
                char letter = Consume().Letter;
                return new Variable(letter, true);
            }
            else if (Peek().Type == TokenType.OpenParenth)
            {
                return ReadEnclosed();
            }
            else
            {
                throw ExceptionAtToken("expected !, (, or variable", Peek());
            }
        }
        
        private Node ReadNode()
        {
            // whether input is binary node or unit is undecidable without reading unit first
            Node unit = ReadUnit();
            if (Peek().Type == TokenType.And || Peek().Type == TokenType.Or || Peek().Type == TokenType.Implies)
            {
                Token opToken = Consume();
                Node secondParam = ReadUnit();
                BinaryOperation.OP op;

                if (opToken.Type == TokenType.And)
                    op = BinaryOperation.OP.And;
                else if (opToken.Type == TokenType.Or)
                    op = BinaryOperation.OP.Or;
                else if (opToken.Type == TokenType.Implies)
                    op = BinaryOperation.OP.Implies;
                else
                    throw ExceptionAtToken("invalid binary operator", opToken); // this should never be thrown

                return new BinaryOperation(unit, secondParam, op);
            }
            else
                return unit;
        }
    }
}
