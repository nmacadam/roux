using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{

    internal class ResolverException : Exception
    {
        public readonly Token Token;

        public ResolverException(Token op, string message)
            : base(message)
        {
            Token = op;
        }
    }

    internal class Resolver : Expr.Visitor<Empty>, Stmt.Visitor<Empty>
    {
        private enum VariableState { Declared, Defined, Read }
        private enum FunctionType { None, Function, Constructor, Lambda, Method }
        private enum ClassType { None, Class }

        private class Variable
        {
            public readonly Token Name;
            public VariableState State;
            public Variable(Token name, VariableState state)
            {
                Name = name;
                State = state;
            }
        }

        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, Variable>> _scopes = new Stack<Dictionary<string, Variable>>();
        private FunctionType _currentFunction = FunctionType.None;
        private ClassType _currentClass = ClassType.None;

        private readonly IErrorReporter _errorReporter;

        public Resolver(Interpreter interpreter, IErrorReporter errorReporter)
        {
            _interpreter = interpreter;
            _errorReporter = errorReporter;
        }

        public void Resolve(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Resolve(statement);
                }
            }
            catch (ResolverException e)
            {
                _errorReporter.Error(e.Token, e.Message);
            }
        }

        #region Statements

        public Empty VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return default;
        }

        public Empty VisitBreakStmt(Stmt.Break stmt)
        {
            return default;
        }

        public Empty VisitClassStmt(Stmt.Class stmt)
        {
            // keep track of whether we are in a class or not to check class-specific keyword validity
            ClassType enclosingClass = _currentClass;
            _currentClass = ClassType.Class;

            Declare(stmt.Name);
            Define(stmt.Name);

            // Push a new scope and add 'this' as a variable to the class
            BeginScope();
            _scopes.Peek().Add("this", new Variable(null, VariableState.Read));
            
            // resolve the class's methods
            foreach (var method in stmt.Methods)
            {
                FunctionType declaration = FunctionType.Method;

                if (method.Name.Lexeme.Equals("construct"))
                {
                    declaration = FunctionType.Constructor;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();

            _currentClass = enclosingClass;

            return default;
        }

        public Empty VisitContinueStmt(Stmt.Continue stmt)
        {
            return default;
        }

        public Empty VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Resolve(stmt.Expression);
            return default;
        }

        public Empty VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt, FunctionType.Function);
            return default;
        }

        public Empty VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
            {
                Resolve(stmt.ElseBranch);
            }
            return default;
        }

        public Empty VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expression);
            return default;
        }

        public Empty VisitReturnStmt(Stmt.Return stmt)
        {
            if (_currentFunction == FunctionType.None)
            {
                throw new ResolverException(stmt.Keyword, "Can't return from top-level code.");
            }
            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.Constructor)
                {
                    throw new ResolverException(stmt.Keyword, "Can't return a value from a constructor.");
                }

                Resolve(stmt.Value);
            }
            return default;
        }

        public Empty VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }
            Define(stmt.Name);

            return default;
        }

        public Empty VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return default;
        }

        #endregion

        #region Expressions

        public Empty VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return default;
        }

        public Empty VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return default;
        }

        public Empty VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);
            foreach (Expr arg in expr.Arguments)
            {
                Resolve(arg);
            }
            return default;
        }

        public Empty VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Object);
            return default;
        }

        public Empty VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return default;
        }

        public Empty VisitLambdaExpr(Expr.Lambda expr)
        {
            BeginScope();
            foreach (var param in expr.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(expr.Body);
            EndScope();
            return default;
        }

        public Empty VisitLiteralExpr(Expr.Literal expr)
        {
            return default;
        }

        public Empty VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return default;
        }

        public Empty VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
            return default;
        }

        public Empty VisitSubscriptExpr(Expr.Subscript expr)
        {
            return default;
        }

        public Empty VisitSuffixExpr(Expr.Suffix expr)
        {
            Resolve(expr.Value);
            return default;
        }

        public Empty VisitTernaryExpr(Expr.Ternary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Middle);
            Resolve(expr.Right);
            return default;
        }

        public Empty VisitThisExpr(Expr.This expr)
        {
            if (_currentClass == ClassType.None)
            {
                throw new ResolverException(expr.Keyword, "Can't use 'this' outside of a class.");
            }

            // 'this' gets resolved like any other variable, it is
            // effectively a property that is automatically bound to all class instances (see VisitClassStmt())
            ResolveLocal(expr, expr.Keyword);
            return default;
        }

        public Empty VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return default;
        }

        public Empty VisitVariableExpr(Expr.Variable expr)
        {
            if (_scopes.Count != 0 && _scopes.Peek().ContainsKey(expr.Name.Lexeme) && _scopes.Peek()[expr.Name.Lexeme].State == VariableState.Declared)
            {
                // todo: should i throw an error here? 
                throw new ResolverException(expr.Name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);

            return default;
        }

        #endregion

        #region Helpers

        private void Declare(Token name)
        {
            if (_scopes.Count == 0) return;
            if (_scopes.Peek().ContainsKey(name.Lexeme))
            {
                throw new ResolverException(name, "Variable with this name is already declared in this scope.");
            }
            _scopes.Peek()[name.Lexeme] = new Variable(name, VariableState.Declared);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0) return;
            _scopes.Peek()[name.Lexeme].State = VariableState.Defined;
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = _currentFunction;
            _currentFunction = type;
            BeginScope();
            foreach (var param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);

            var scope = _scopes.Peek();
            for (int i = 0; i < function.Body.Count; i++)
            {
                if (i < function.Body.Count - 1 && _scopes.Peek() == scope && function.Body[i] is Stmt.Return returnStmt)
                {
                    _errorReporter.Warning(returnStmt.Keyword, "Unreachable code detected");
                    break;
                }
            }

            EndScope();
            _currentFunction = enclosingFunction;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            int depth = 0;
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    scope[name.Lexeme].State = VariableState.Read;
                    _interpreter.Resolve(expr, depth);
                    return;
                }
                depth++;
            }
            // not found, assume global
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, Variable>());
        }

        private void EndScope()
        {
            var scope = _scopes.Peek();
            foreach (var item in scope.Values)
            {
                if (item.State != VariableState.Read)
                {
                    _errorReporter.Warning(item.Name, "Local variable is never used.");
                }
            }
            _scopes.Pop();
        }

        #endregion
    }
}
