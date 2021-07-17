using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
	internal sealed class Empty
    {
		private Empty()
        {
			throw new InvalidOperationException("Empty is not meant to be instantiated.");
        }
    }

	internal abstract class Stmt
	{
		public interface Visitor<T>
		{
			T VisitBlockStmt(Block stmt);
			T VisitExpressionStmt(ExpressionStmt stmt);
			T VisitPrintStmt(Print stmt);
			T VisitVarStmt(Var stmt);
		}

		public abstract T Accept<T>(Visitor<T> visitor);

		internal class Block : Stmt
		{
			public readonly List<Stmt> Statements;

			public Block(List<Stmt> statement)
			{
				Statements = statement;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitBlockStmt(this);
			}
		}

		internal class ExpressionStmt : Stmt
		{
			public readonly Expr Expression;

			public ExpressionStmt(Expr expression)
			{
				Expression = expression;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitExpressionStmt(this);
			}
		}

		internal class Print : Stmt
		{
			public readonly Expr Expression;

			public Print(Expr expression)
			{
				Expression = expression;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitPrintStmt(this);
			}
		}

		internal class Var : Stmt
		{
			public readonly Token Name;
			public readonly Expr Initializer;

			public Var(Token name, Expr initializer)
			{
				Name = name;
				Initializer = initializer;
			}

			public override T Accept<T>(Visitor<T> visitor)
			{
				return visitor.VisitVarStmt(this);
			}
		}
	}
}
