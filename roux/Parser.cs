using System;
using System.Collections.Generic;

namespace Roux
{
    internal class ParseException : Exception { }

    /// <summary>
    /// Uses a list of input tokens to build a sequence of expressions
    /// </summary>
    internal class Parser
    {
        private readonly List<Token> _tokens;
        private readonly IErrorReporter _errorReporter;

        private int _current = 0;
        private int _loopDepth = 0;

        public Parser(List<Token> tokens, IErrorReporter errorReporter)
        {
            _tokens = tokens;
            _errorReporter = errorReporter;
        }

        /* Recursive Descent Grammar:
         *  expression     → equality ;
         *  equality       → comparison ( ( "!=" | "==" ) comparison )* ;
         *  comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
         *  term           → factor ( ( "-" | "+" ) factor )* ;
         *  factor         → unary ( ( "/" | "*" ) unary )* ;
         *  unary          → ( "!" | "-" ) unary | primary ;
         *  primary        → Number | String | "true" | "false" | "null" | "(" expression ")" ;
         */

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseException e)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Separator();
        }

        /// <summary>
        /// Generates an expr for the expression seperator (checks the operator ,)
        /// </summary>
        private Expr Separator()
        {
            return BinaryExpression(Ternary, TokenType.Comma);
        }

        /// <summary>
        /// Generates an expr for ternary expressions (checks ternary operator ?:)
        /// </summary>
        private Expr Ternary()
        {
            Expr expr = Equality();

            if (Match(TokenType.QuestionMark))
            {
                Expr middle = Equality();
                Consume(TokenType.Colon, "Expected : in ternary expression");
                Expr right = Equality();
                expr = new Expr.Ternary(expr, middle, right);
            }

            return expr;
        }

        /// <summary>
        /// Generates an expr for equality check (checks operators == and !=)
        /// </summary>
        private Expr Equality()
        {
            return BinaryExpression(Comparison, TokenType.BangEqual, TokenType.EqualEqual);
        }

        /// <summary>
        /// Generates an expr for comparison (checks operators >, >=, <, <=)
        /// </summary>
        private Expr Comparison()
        {
            return BinaryExpression(Term, TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual);
        }

        /// <summary>
        /// Generates an expr for terms (checks operators + and -)
        /// </summary>
        private Expr Term()
        {
            return BinaryExpression(Factor, TokenType.Minus, TokenType.Plus);
        }

        /// <summary>
        /// Generates an expr for factors (checks operators / and *)
        /// </summary>
        private Expr Factor()
        {
            return BinaryExpression(Unary, TokenType.Slash, TokenType.Star);
        }

        /// <summary>
        /// Generates an expr for unary operators (checks operators ! and -)
        /// </summary>
        private Expr Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        /// <summary>
        /// Generates an expr for primary expression (highest precedence, check for identifiers and groupings)
        /// </summary>
        private Expr Primary()
        {
            if (Match(TokenType.False)) return new Expr.Literal(false);
            if (Match(TokenType.True)) return new Expr.Literal(true);
            if (Match(TokenType.Null)) return new Expr.Literal(null);

            if (Match(TokenType.Number, TokenType.String))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.LeftParenthesis))
            {
                Expr expr = Expression();
                Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }


            var peeked = Peek();
            throw Error(Peek(), "Expect expression");
        }

        /// <summary>
        /// Initiates the parser's recursive descent at a given step for the given tokens to match
        /// </summary>
        /// <param name="descentStep">The step to begin at</param>
        /// <param name="match">The token(s) to match against</param>
        /// <returns>The resulting Expr</returns>
        private Expr BinaryExpression(System.Func<Expr> descentStep, params TokenType[] match)
        {
            Expr expr = descentStep.Invoke();

            // Build out the equality operation(s) as necessary
            while (Match(match))
            {
                Token op = Previous();
                Expr right = descentStep.Invoke();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Discard tokens until the beginning of the next statement in the case that a statement fails
        /// </summary>
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().TokenType == TokenType.Semicolon) return;

                switch (Peek().TokenType)
                {
                    case TokenType.Class:
                    //case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }

                Advance();
            }
        }

        /// <summary>
        /// Checks if the next token is of the expected type, and advances
        /// </summary>
        /// <param name="type">The TokenType to check against</param>
        /// <param name="errorMessage">The message to use if the next token is not the right type</param>
        /// <returns>The next token</returns>
        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), errorMessage);
        }

        /// <summary>
        /// Generates a ParseException and sends error info to the error reporter
        /// </summary>
        /// <param name="token">The token that caused a problem</param>
        /// <param name="message">A message indicating the nature of the error</param>
        /// <returns>A parse exception to be thrown</returns>
        private ParseException Error(Token token, string message)
        {
            _errorReporter.Error(token, message);
            return new ParseException();
        }

        /// <summary>
        /// Checks if the current token(s) is/are a given type(s), and consumes it if true
        /// </summary>
        /// <param name="types">A TokenType or set of TokenTypes to check in the sequence</param>
        /// <returns>Whether all types match the sequence</returns>
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the current token is of the given type
        /// </summary>
        /// <param name="type">The type to check against</param>
        /// <returns>If the current token is of the given type</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().TokenType == type;
        }

        /// <summary>
        /// Consumes the current token and returns it
        /// </summary>
        /// <returns>The consumed token</returns>
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        /// <summary>
        /// Returns if the current token is the end-of-file
        /// </summary>
        /// <returns>If the current token is the end-of-file</returns>
        private bool IsAtEnd()
        {
            return Peek().TokenType == TokenType.Eof;
        }

        /// <summary>
        /// Returns the current token
        /// </summary>
        /// <returns>The current token</returns>
        private Token Peek()
        {
            return _tokens[_current];
        }

        /// <summary>
        /// Returns the previous token
        /// </summary>
        /// <returns>The previous token</returns>
        private Token Previous()
        {
            return _tokens[_current - 1];
        }
    }
}
