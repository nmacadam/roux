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

        /* Statement Grammar:
         *  program        → declaratation* EOF ;
         *  declaratation  → varDecl | statement ;
         *  varDecl        → "var" Identifier ( "=" expression )? ";" ;
         *  statement      → exprStatement | printStmt ;
         */

        /* Expression Grammar:
         *  expression     → separator ;
         *  separator      → assignment ( ( "," ) assignment )* ;
         *  assignment     → Identifier ( ( "=" ) ternary )* ;
         *  
         *  ternary        → comparison ( ( ( "?" ) comparison )* ( ":" ) comparison )* ;
         *  comparison     → equality ( ( ">" | ">=" | "<" | "<=" ) equality )* ;
         *  equality       → bitwise-or ( ( "!=" | "==" ) bitwise-or )* ;
         *  bitwise-or     → bitwise-xor ( ( "|" ) bitwise-xor )* ;
         *  bitwise-xor    → bitwise-and ( ( "^" ) bitwise-and )* ;
         *  bitwise-and    → term ( ( "&" ) term )* ;
         *  term           → factor ( ( "-" | "+" ) factor )* ;
         *  factor         → unary ( ( "/" | "*" ) unary )* ;
         *  unary          → ( "!" | "-" ) unary | primary ;
         *  primary        → Number | String | "true" | "false" | "null" | "(" expression ")" | Identifier;
         */

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();

            try
            {
                while (!IsAtEnd())
                {
                    statements.Add(Declaration());
                }
            }
            catch (ParseException e)
            {
                return null;
            }

            return statements;
        }

        public Expr ParseExpression()
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

        #region Statements

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.Class))
                {
                    return ClassDeclaration();
                }
                if (Match(TokenType.Fun))
                {
                    return Function("function");
                }
                if (Match(TokenType.Var))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseException e)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "Expect class name.");
            Expr superclass = null;
            //if (Match(TokenType.Less))
            //{
            //    Consume(TokenType.Identifier, "Expect superclass name.");
            //    superclass = new Expr.Variable(Previous());
            //}
            Consume(TokenType.LeftBrace, "Expect '{' before class body.");

            List<Stmt.Function> methods = new List<Stmt.Function>();
            List<Stmt.Function> staticMethods = new List<Stmt.Function>();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                // handle vars
                if (Match(TokenType.Static))
                {
                    staticMethods.Add(Function("method"));
                }
                else
                {
                    methods.Add(Function("method"));
                }

            }
            Consume(TokenType.RightBrace, "Expect '}' after class body.");
            return new Stmt.Class(name, superclass, methods, staticMethods);
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(TokenType.Identifier, $"Expect {kind} name.");
            Consume(TokenType.LeftParenthesis, $"Expect '(' after {kind} name.");
            List<Token> parameters = new List<Token>();
            if (!Check(TokenType.RightParenthesis))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }

                    parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expect '{' before " + kind + " body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        //private Stmt.Function AnonymousFunction()
        //{
        //    Consume(TokenType.LeftParenthesis, $"Expect '(' after 'fun'");
        //    List<Token> parameters = new List<Token>();
        //    if (!Check(TokenType.RightParenthesis))
        //    {
        //        do
        //        {
        //            if (parameters.Count >= 255)
        //            {
        //                Error(Peek(), "Cannot have more than 255 parameters.");
        //            }

        //            parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
        //        } while (Match(TokenType.Comma));
        //    }
        //    Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
        //    Consume(TokenType.LeftBrace, "Expect '{' before anonymous function body.");
        //    List<Stmt> body = Block();
        //    return new Stmt.Function(null, parameters, body);
        //}

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "Expect variable name.");

            Expr initializer = null;

            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.Break)) return BreakStatement();
            if (Match(TokenType.Continue)) return ContinueStatement();
            if (Match(TokenType.For)) return ForStatement();
            if (Match(TokenType.If)) return IfStatement();
            if (Match(TokenType.Print)) return PrintStatement();
            if (Match(TokenType.Return)) return ReturnStatement();
            if (Match(TokenType.While)) return WhileStatement();
            if (Match(TokenType.LeftBrace)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt BreakStatement()
        {
            if (_loopDepth == 0)
            {
                Error(Previous(), "Cannot use 'break' outside of a loop.");
            }

            Consume(TokenType.Semicolon, "Expect ';' after 'break'.");
            return new Stmt.Break();
        }

        private Stmt ContinueStatement()
        {
            if (_loopDepth == 0)
            {
                Error(Previous(), "Cannot use 'continue' outside of a loop.");
            }

            Consume(TokenType.Semicolon, "Expect ';' after 'continue'.");
            return new Stmt.Continue();
        }

        private Stmt ForStatement()
        {
            _loopDepth++;

            try
            {
                Consume(TokenType.LeftParenthesis, "Expect '(' after 'for'.");

                Stmt initializer;
                if (Match(TokenType.Semicolon))
                {
                    initializer = null;
                }
                else if (Match(TokenType.Var))
                {
                    initializer = VarDeclaration();
                }
                else
                {
                    initializer = ExpressionStatement();
                }

                Expr condition = null;
                if (!Check(TokenType.Semicolon))
                {
                    condition = Expression();
                }
                Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

                Expr increment = null;
                if (!Check(TokenType.RightParenthesis))
                {
                    increment = Expression();
                }
                Consume(TokenType.RightParenthesis, "Expect ')' after for clauses.");

                Stmt body = Statement();

                if (increment != null)
                {
                    body = new Stmt.Block(new List<Stmt>() { body, new Stmt.ExpressionStmt(increment) });
                }

                if (condition == null) condition = new Expr.Literal(true);
                body = new Stmt.While(condition, body);

                if (initializer != null)
                {
                    body = new Stmt.Block(new List<Stmt>() { initializer, body });
                }

                return body;
            }
            finally
            {
                _loopDepth--;
            }
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LeftParenthesis, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RightParenthesis, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();

            // todo: going to cause issue
            Consume(TokenType.Semicolon, "Expect ; or newline after value");
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            // todo: going to cause issue
            Consume(TokenType.Semicolon, "Expect ; after return value");
            return new Stmt.Return(keyword, value);
        }

        private Stmt WhileStatement()
        {
            _loopDepth++;

            try
            {
                Consume(TokenType.LeftParenthesis, "Expect '(' after 'while'.");
                Expr condition = Expression();
                Consume(TokenType.RightParenthesis, "Expect ')' after if condition.");
                Stmt body = Statement();

                return new Stmt.While(condition, body);
            }
            finally
            {
                _loopDepth--;
            }
            
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RightBrace, "Expect } after block");
            return statements;
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();

            // todo: going to cause issue
            Consume(TokenType.Semicolon, "Expect ; or newline after expression");
            return new Stmt.ExpressionStmt(expr);
        }

        #endregion

        #region Expressions

        /// <summary>
        /// Entry-point for grammar
        /// </summary>
        private Expr Expression()
        {
            return Separator();
            //return Assignment();
        }

        /// <summary>
        /// Generates an expr for the expression seperator (checks the operator ,)
        /// </summary>
        private Expr Separator()
        {
            return BinaryExpression(Lambda, TokenType.Comma);
        }

        private Expr Lambda()
        {
            // might need to be one lower
            //Expr expr = Assignment();

            if (Match(TokenType.Fun))
            {
                Consume(TokenType.LeftParenthesis, $"Expect '(' after 'fun'");
                List<Token> parameters = new List<Token>();
                if (!Check(TokenType.RightParenthesis))
                {
                    do
                    {
                        if (parameters.Count >= 255)
                        {
                            Error(Peek(), "Cannot have more than 255 parameters.");
                        }

                        parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
                    } while (Match(TokenType.Comma));
                }
                Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
                Consume(TokenType.LeftBrace, "Expect '{' before lambda function body.");
                List<Stmt> body = Block();
                return new Expr.Lambda(parameters, body);
            }

            //return expr;
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Ternary();

            bool compound = Match(
                TokenType.MinusEqual, 
                TokenType.PlusEqual, 
                TokenType.StarEqual, 
                TokenType.SlashEqual, 
                TokenType.PercentEqual
            );

            if (Match(TokenType.Equal) || compound)
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Get get)
                {
                    return new Expr.Set(get.Object, get.Name, value);
                }
                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).Name;
                    switch (equals.TokenType)
                    {
                        case TokenType.MinusEqual: return new Expr.Assign(name, new Expr.Binary(expr, equals.Copy(TokenType.Minus), value));
                        case TokenType.PlusEqual: return new Expr.Assign(name, new Expr.Binary(expr, equals.Copy(TokenType.Plus), value));
                        case TokenType.StarEqual: return new Expr.Assign(name, new Expr.Binary(expr, equals.Copy(TokenType.Star), value));
                        case TokenType.SlashEqual: return new Expr.Assign(name, new Expr.Binary(expr, equals.Copy(TokenType.Slash), value));
                        case TokenType.PercentEqual: return new Expr.Assign(name, new Expr.Binary(expr, equals.Copy(TokenType.Percent), value));
                        case TokenType.Equal: return new Expr.Assign(name, value);
                    }
                }

                // todo: handle grouping

                Error(equals, "Invalid assignment target");
            }

            return expr;
        }

        /// <summary>
        /// Generates an expr for ternary expressions (checks ternary operator ?:)
        /// </summary>
        private Expr Ternary()
        {
            Expr expr = Or();

            if (Match(TokenType.QuestionMark))
            {
                Expr middle = Or();
                Consume(TokenType.Colon, "Expected : in ternary expression");
                Expr right = Or();
                expr = new Expr.Ternary(expr, middle, right);
            }

            return expr;
        }

        private Expr Or()
        {
            return LogicalExpression(And, TokenType.Or);
        }

        private Expr And()
        {
            return LogicalExpression(Equality, TokenType.And);
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
            return BinaryExpression(BitwiseOr, TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual);
        }

        /// <summary>
        /// Generates an expr for bitwise OR (checks the operator |)
        /// </summary>
        private Expr BitwiseOr()
        {
            return BinaryExpression(BitwiseXor, TokenType.Bar);
        }

        /// <summary>
        /// Generates an expr for bitwise XOR (checks the operator ^)
        /// </summary>
        private Expr BitwiseXor()
        {
            return BinaryExpression(BitwiseAnd, TokenType.Caret);
        }

        /// <summary>
        /// Generates an expr for bitwise AND (checks the operator &)
        /// </summary>
        private Expr BitwiseAnd()
        {
            return BinaryExpression(Term, TokenType.Ampersand);
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
            return BinaryExpression(Unary, TokenType.Slash, TokenType.Star, TokenType.Percent);
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

            return Prefix();
        }

        private Expr Prefix()
        {
            if (Match(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                Token op = Previous();
                Expr right = Prefix();
                Expr value = new Expr.Literal(1.0);

                if (right is Expr.Variable)
                {
                    Token name = ((Expr.Variable)right).Name;

                    switch (op.TokenType)
                    {
                        case TokenType.MinusMinus: return new Expr.Assign(name, new Expr.Binary(right, op.Copy(TokenType.Minus), value));
                        case TokenType.PlusPlus: return new Expr.Assign(name, new Expr.Binary(right, op.Copy(TokenType.Plus), value));
                    }
                }
            }

            return Suffix();
        }

        private Expr Suffix()
        {
            Expr expr = Call();

            if (Match(TokenType.MinusMinus, TokenType.PlusPlus) && expr is Expr.Variable)
            {
                Token op = Previous();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).Name;
                    Expr value = new Expr.Literal(1.0);

                    return new Expr.Suffix(name, op, expr);
                }

                Error(op, "Invalid assignment target");
            }

            // todo: handle grouping

            return expr;
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LeftParenthesis))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.Dot))
                {
                    Token name = Consume(TokenType.Identifier, "Expect property name after '.'.");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }
            return expr;
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

            if (Match(TokenType.This))
            {
                return new Expr.This(Previous());
            }

            if (Match(TokenType.Identifier))
            {
                return new Expr.Variable(Previous());
            }

            if (Peek().TokenType == TokenType.Fun)
            {
                return Lambda();
            }

            if (Match(TokenType.LeftParenthesis))
            {
                return Grouping();
            }

            // Todo: make sure this works as expected, might need to have left and inner (right?) expressions for accessing array or whatever
            if (Match(TokenType.LeftBracket))
            {
                return Susbcript();
            }


            var peeked = Peek();
            throw Error(Peek(), "Expect expression");
        }

        private Expr Susbcript()
        {
            Expr expr = Expression();
            Consume(TokenType.RightBracket, "Expect ']' after expression.");
            return new Expr.Subscript(expr);
        }

        private Expr Grouping()
        {
            // todo: handle function calls, multiple return values/tuples

            Expr expr = Expression();
            Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        #endregion

        #region Helpers

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!Check(TokenType.RightParenthesis))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    }
                    //arguments.Add(Expression());
                    arguments.Add(Assignment());
                } while (Match(TokenType.Comma));
            }

            Token paren = Consume(TokenType.RightParenthesis, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
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
        /// Initiates the parser's recursive descent at a given step for the given tokens to match
        /// </summary>
        /// <param name="descentStep">The step to begin at</param>
        /// <param name="match">The token(s) to match against</param>
        /// <returns>The resulting Expr</returns>
        private Expr LogicalExpression(System.Func<Expr> descentStep, params TokenType[] match)
        {
            Expr expr = descentStep.Invoke();

            // Build out the equality operation(s) as necessary
            while (Match(match))
            {
                Token op = Previous();
                Expr right = descentStep.Invoke();
                expr = new Expr.Logical(expr, op, right);
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

        //private bool LookAhead(TokenType lookFor, TokenType stopOn, out Token foundToken)
        //{
        //    foundToken = default;
        //    int itr = _current;
        //    while (itr < _tokens.Count && _tokens[itr].TokenType != stopOn)
        //    {
        //        if (_tokens[itr].TokenType == lookFor)
        //        {
        //            foundToken = _tokens[itr];
        //            return true;
        //        }

        //        itr++;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Returns the previous token
        /// </summary>
        /// <returns>The previous token</returns>
        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        #endregion
    }
}
