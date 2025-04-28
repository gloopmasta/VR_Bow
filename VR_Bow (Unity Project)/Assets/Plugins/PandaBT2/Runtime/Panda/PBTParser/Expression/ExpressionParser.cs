using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PandaBT.Compilation
{
	public class ExpressionParser
	{
		public static Expression[] ParseToken(Token[] token )
		{
			var expressions = new List<Expression>();

			int openParenthesisCount = 0;
			Expression currentExpression = null;
			List<Token> currentExpressionTokens = new List<Token>();
			foreach( Token t in token)
			{
				if( t.type == TokenType.ExpressionParenthesis_Open)
				{
					openParenthesisCount++;
					if( openParenthesisCount == 1)
					{
						currentExpression = new Expression();
						currentExpression.startToken = t;
						continue;
					}
				}

				if (t.type == TokenType.ExpressionParenthesis_Closed)
				{
					openParenthesisCount--;
					if( openParenthesisCount == 0)
					{
						currentExpression.tokens = currentExpressionTokens.ToArray();
						currentExpressionTokens.Clear();
						expressions.Add(currentExpression);
						currentExpression.endToken = t;
						currentExpression = null;
						continue;
					}
				}

				if( currentExpression != null && t.type != TokenType.Indent )
					currentExpressionTokens.Add( t );
			}

			foreach( Expression e in expressions)
				e.children = ParseToken(e.tokens);

			return expressions.ToArray();
		}

		public static void ParseExpression(Expression expression, CompilationUnit cu)
		{
			CheckTokenAfterOperator(expression, cu);

			var stack = new Stack<Expression>();

			var fifo = new Queue<Expression>();
			fifo.Enqueue(expression);
			stack.Push(expression);
			while(fifo.Count > 0)
			{
				var parent = fifo.Dequeue();
				foreach(var child in parent.children)
				{
					stack.Push(child);
					fifo.Enqueue(child);
				}
			}

			while(stack.Count > 0)
			{
				var next = stack.Pop();
				ParseSimpleExpression(next, cu);
			}
			
		}

		static void ParseSimpleExpression(Expression expression, CompilationUnit cu)
		{
			EvaluateOperatorOfType(expression, cu, TokenType.NotOperator, isBinary: false);
			EvaluateOperatorOfType(expression, cu, TokenType.SequenceOperator);
			EvaluateOperatorOfType(expression, cu, TokenType.FallbackOperator);
			EvaluateOperatorOfType(expression, cu, TokenType.ParallelOperator);
			EvaluateOperatorOfType(expression, cu, TokenType.RaceOperator);
		}

		static void EvaluateOperatorOfType(Expression expression, CompilationUnit cu, TokenType type, bool isBinary=true)
		{
			int expression_parenthesis_count = 0;
			foreach(var token in expression.tokens)
			{
				if( token.type == TokenType.ExpressionParenthesis_Open)
				{
					expression_parenthesis_count++;
					continue;
				}

				if (token.type == TokenType.ExpressionParenthesis_Closed)
				{
					expression_parenthesis_count--;
					continue;
				}

				if( expression_parenthesis_count > 0)
				{ // skip sub expressions
					continue;
				}


				if (token.type == type)
				{
					if( isBinary )
						ParentToBinaryOperator(expression, cu, token);
					else
						ParentToUnaryOperator(expression, cu, token);
				}
			}
		}

		private static void ParentToBinaryOperator(Expression expression, CompilationUnit cu, Token token)
		{
			var operatorNode = cu.GetNode(token);
			if (operatorNode.children.Count > 0)
				throw new Exception("Operator already parsed.(This should never happen)");

			var leftNode = GetLeftNode(expression, cu, operatorNode);
			var rightNode = GetRightNode(expression, cu, operatorNode);

			if( leftNode == null)
			{
				throw new Exception("Node expected to the left.");
			}

			if (rightNode == null)
			{
				throw new Exception("Node expected to the right.");
			}

			operatorNode.AddChild(leftNode);
			operatorNode.AddChild(rightNode);
		}

		private static void ParentToUnaryOperator(Expression expression, CompilationUnit cu, Token token)
		{
			var operatorNode = cu.GetNode(token);

			var rightNode = GetRightNode(expression, cu, operatorNode);

			if (rightNode == null)
			{
				throw new Exception("Node expected to the right.");
			}

			operatorNode.AddChild(rightNode);
		}

		public static Node GetTopParentOrNull(Node node, CompilationUnit cu)
		{
			Node top = null;
			var parent = top = GetParentOrNull(node, cu);
			var cur = top;
			while(cur != null)
			{
				cur = GetParentOrNull(cur, cu);

				if( cur == parent)
					throw new System.Exception("ERROR: circular parenting. (This should not happen) ");

				if (cur != null)
					top = cur;

			}
			return top;
		}

		public static Node GetParentOrNull(Node node, CompilationUnit cu)
		{
			Node parent = null;

			if( node.parent != -1)
				parent = cu.nodes[node.parent];
			return parent;
		}

		static Node GetLeftNode(Expression expression, CompilationUnit cu, Node node)
		{
			var index = node.index;

			var leftNode = index - 1 >= 0 ? cu.nodes[index - 1] : null;
			var leftNodeParent = leftNode != null ? GetTopParentOrNull(leftNode, cu) : null;

			return leftNodeParent != null ? leftNodeParent : leftNode;
		}

		static Node GetRightNode(Expression expression, CompilationUnit cu, Node node)
		{
			var index = node.index;

			var rightNode = index + 1 < cu.nodes.Length ? cu.nodes[index + 1] : null;
			var rigthNodeParent = rightNode != null ? GetTopParentOrNull(rightNode, cu) : null;

			return rigthNodeParent != null ? rigthNodeParent : rightNode;
		}

		static void CheckTokenAfterOperator(Expression expression, CompilationUnit cu)
		{
			Token previousToken = null;
			bool isPreviousOperator = false;
			foreach (var token in expression.tokens)
			{
				var type = token.type;
				var isOperator = IsOperator(type);
				var isTask = IsTask(type);
				var isOpenParenthesis = type == TokenType.ExpressionParenthesis_Open;
				if ( isTask || isOperator || isOpenParenthesis  ) {

					var isExpectedToken = !isPreviousOperator || isPreviousOperator && (isTask || isOpenParenthesis);
					if( !isExpectedToken )
					{
						throw new PandaScriptException($"Unexpected token after operator `{previousToken.substring}` ", PBTTokenizer.filepath, token.line);
					}

					previousToken = token;
					isPreviousOperator = isOperator;
				}
			}
		}


		static bool IsOperator(TokenType tokenType)
		{
			bool isOperator = false;
			switch (tokenType)
			{
				case TokenType.FallbackOperator:
				case TokenType.RaceOperator:
				case TokenType.SequenceOperator:
				case TokenType.ParallelOperator:
					isOperator = true;
					break;
				default:
					isOperator = false;
					break;
			}

			return isOperator;
		}

		static bool IsTask(TokenType tokenType)
		{
			var isTask = false;
			if( tokenType == TokenType.Word)
			{
				isTask = true;
			}
			return isTask;
		}




	}
}
