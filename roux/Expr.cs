using System.Collections.Generic;

namespace Roux
{
	internal abstract class Expr
	{
		public interface Visitor<T>
		{
			T VisitAssignExpr(Assign expr);
			T VisitBinaryExpr(Binary expr);
			T VisitCallExpr(Call expr);
			// T VisitGetExpr(Get expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLambdaExpr(Lambda expr);
			T VisitLiteralExpr(Literal expr);
			T VisitLogicalExpr(Logical expr);
			// T VisitSetExpr(Set expr);
			T VisitSubscriptExpr(Subscript expr);
			T VisitSuffixExpr(Suffix expr);
			// T VisitSuperExpr(Super expr);
			T VisitTernaryExpr(Ternary expr);
			// T VisitThisExpr(This expr);
			T VisitUnaryExpr(Unary expr);
			T VisitVariableExpr(Variable expr);
		}

		public abstract T Accept<T>(Visitor<T> visitor);

		internal class Assign : Expr
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

		internal class Call : Expr
		{
			public readonly Expr Callee;
			public readonly Token Parenthesis;
			public readonly List<Expr> Arguments;

			public Call(Expr callee, Token parenthesis, List<Expr> arguments)
			{
				Callee = callee;
				Parenthesis = parenthesis;
				Arguments = arguments;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitCallExpr(this);
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

		public class Suffix : Expr
		{
			public readonly Token Name;
			public readonly Token Operator;
			public readonly Expr Value;

			public Suffix(Token name, Token op, Expr value)
			{
				Name = name;
				Operator = op;
				Value = value;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitSuffixExpr(this);
			}
		}

		internal class Lambda : Expr
		{
			public readonly List<Token> Parameters;
			public readonly List<Stmt> Body;

			public Lambda(List<Token> parameters, List<Stmt> body)
			{
				Parameters = parameters;
				Body = body;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitLambdaExpr(this);
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
