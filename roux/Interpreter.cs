using System;
using System.Collections.Generic;

namespace Roux
{
    internal class InterpreterException : Exception 
    {
        public readonly Token Token;

        public InterpreterException(Token op, string message)
            : base(message)
        {
            Token = op;
        }
    }

    internal class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private Environment _environment = new Environment();

        private readonly IErrorReporter _errorReporter;

        public Interpreter(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (InterpreterException e)
            {
                _errorReporter.RuntimeError(e.Token, e.Message);
            }
            catch (EnvironmentException e)
            {
                _errorReporter.RuntimeError(e.Token, e.Message);
            }
        }

        public object InterpretExpression(Expr expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
                return value;
            }
            catch (InterpreterException e)
            {
                _errorReporter.RuntimeError(e.Token, e.Message);
            }

            return null;
        }

        #region Statement Visiting

        public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.Expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            // Note: roux does not require an initalizer, default initialization to null
            object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }
            return null;
        }

        #endregion

        #region Expression Visiting

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.Value);
            _environment.Assign(expr.Name, value);
            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            switch (expr.Operator.TokenType)
            {
                // Comma
                case TokenType.Comma:
                    return right;

                // Comparison
                case TokenType.Greater:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GreaterEqual:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.Less:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LessEqual:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;

                case TokenType.BangEqual:
                    return !IsEqual(left, right);
                case TokenType.EqualEqual:
                    return IsEqual(left, right);

                // Arithmetic
                case TokenType.Minus:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.Slash:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.Star:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.Percent:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left % (double)right;

                // Special cases
                case TokenType.Plus:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    if (left is string && right is string)
                    {
                        return (double)left + (double)right;
                    }

                    // Try to ToString whatever the value is
                    if (left is string && !(right is string) && right != null)
                    {
                        return (string)left + right.ToString();
                    }
                    else if (!(left is string) && left != null && right is string)
                    {
                        return left.ToString() + (string)right;
                    }
                    break;
            }

            // Unreachable
            throw new InterpreterException(null, null);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.Left);

            if (expr.Operator.TokenType == TokenType.Or) 
            {
                if (IsTruthy(left)) return left;
            } 
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitSubscriptExpr(Expr.Subscript expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            object left = Evaluate(expr.Left);
            object middle = Evaluate(expr.Middle);
            object right = Evaluate(expr.Right);

            return IsTruthy(left) ? middle : right;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.Right);

            switch (expr.Operator.TokenType)
            {
                case TokenType.Bang: return !IsTruthy(right);
                case TokenType.Minus:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            // Unreachable
            throw new InterpreterException(null, null);
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return _environment.Get(expr.Name);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Send the statement back through the interpreter's visitor implementation
        /// </summary>
        private object Execute(Stmt stmt)
        {
            return stmt.Accept(this);
        }

        /// <summary>
        /// Send a list of statements back through the interpreter's visitor implementation
        /// </summary>
        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = _environment;
            try
            {
                _environment = environment;
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        /// <summary>
        /// Send the expression back through the interpreter's visitor implementation
        /// </summary>
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        /// <summary>
        /// false/null false, all others true
        /// </summary>
        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        /// <summary>
        /// Null equals null, lhs cannot be null
        /// </summary>
        private bool IsEqual(object a, object b)
        {           
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Checks if the operand is a number
        /// </summary>
        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;

            throw new InterpreterException(op, "Operand must be a number.");
        }

        /// <summary>
        /// Checks if the operands are numbers
        /// </summary>
        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;

            throw new InterpreterException(op, "Operand must be a number.");
        }


        private string Stringify(object obj)
        {
            if (obj == null) return "null";

            // Hack. Work around Java adding ".0" to integer-valued doubles.
            if (obj is double)
            {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        #endregion
    }
}
