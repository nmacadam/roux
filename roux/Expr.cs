using System.Collections.Generic;

namespace Roux
{
	internal abstract class Expr
	{
		public interface Visitor<T>
		{
			T VisitAssignExpr(Assign expr);
			T VisitBinaryExpr(Binary expr);
			// T VisitCallExpr(Call expr);
			// T VisitGetExpr(Get expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitLogicalExpr(Logical expr);
			// T VisitSetExpr(Set expr);
			T VisitSubscriptExpr(Subscript expr);
			// T VisitSuperExpr(Super expr);
			T VisitTernaryExpr(Ternary expr);
			// T VisitThisExpr(This expr);
			T VisitUnaryExpr(Unary expr);
			T VisitVariableExpr(Variable expr);
		}

		public abstract T Accept<T>(Visitor<T> visitor);

		public class Assign : Expr
		{
			public readonly Token Name;
			public readonly Expr Value;

			public Assign(Token name, Expr value)
			{
				Name = name;
				Value = value;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitAssignExpr(this);
			}
		}

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

		internal class Subscript : Expr
		{
			public readonly Expr Expression;

			public Subscript(Expr expression)
			{
				Expression = expression;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitSubscriptExpr(this);
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

		public class Logical : Expr
		{
			public readonly Expr Left;
			public readonly Token Operator;
			public readonly Expr Right;

			public Logical(Expr left, Token op, Expr right)
			{
				Left = left;
				Operator = op;
				Right = right;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitLogicalExpr(this);
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

		internal class Variable : Expr
		{
			public readonly Token Name;

			public Variable(Token name)
			{
				Name = name;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitVariableExpr(this);
			}

		}
	}
}
