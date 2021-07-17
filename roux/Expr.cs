using System.Collections.Generic;

namespace Roux
{
	internal abstract class Expr
	{
		public interface Visitor<T>
		{
			T VisitTernaryExpr(Ternary expr);
			T VisitBinaryExpr(Binary expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitUnaryExpr(Unary expr);
		}

		public abstract T Accept<T>(Visitor<T> visitor);

		internal class Ternary : Expr
		{
			public readonly Expr Left;
			public readonly Expr Middle;
			public readonly Expr Right;

			public Ternary(Expr left, Expr middle, Expr right)
			{
				Left = left;
				Middle = middle;
				Right = right;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitTernaryExpr(this);
			}
		}

		internal class Binary : Expr
		{
			public readonly Expr Left;
			public readonly Token Operator;
			public readonly Expr Right;

			public Binary(Expr left, Token op, Expr right)
			{
				Left = left;
				Operator = op;
				Right = right;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitBinaryExpr(this);
			}
		}

		internal class Grouping : Expr
		{
			public readonly Expr Expression;

			public Grouping(Expr expression)
			{
				Expression = expression;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitGroupingExpr(this);
			}
		}

		internal class Literal : Expr
		{
			public readonly object Value;

			public Literal(object value)
			{
				Value = value;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitLiteralExpr(this);
			}
		}

		internal class Unary : Expr
		{
			public readonly Token Operator;
			public readonly Expr Right;

			public Unary(Token op, Expr right)
			{
				Operator = op;
				Right = right;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitUnaryExpr(this);
			}
		}
	}
}
