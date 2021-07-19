using System;
using System.Text;

namespace Roux
{
    internal class AstPrinter : Expr.Visitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitTernaryExpr(Expr.Ternary expr)
        {
            return Parenthesize("?:", expr.Left, expr.Middle, expr.Right);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitSubscriptExpr(Expr.Subscript expr)
        {
            return Parenthesize("[]", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "null";
            return expr.Value.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Right);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            // todo: dont think i can revisit the same thing here?
            return Parenthesize("=", expr, expr.Value);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return Parenthesize("var", expr);
        }

        public string VisitSuffixExpr(Expr.Suffix expr)
        {
            return Parenthesize("suffix " + expr.Operator.Lexeme, expr.Value);
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            return Parenthesize("call", expr.Callee);
        }

        public string VisitLambdaExpr(Expr.Lambda expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }
    }
}
