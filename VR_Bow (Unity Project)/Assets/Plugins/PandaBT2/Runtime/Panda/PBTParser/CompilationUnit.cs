/*
Copyright (c) 2015 Eric Begue (ericbeg@gmail.com)

This source file is part of the Panda BT package, which is licensed under
the Unity's standard Unity Asset Store End User License Agreement ("Unity-EULA").

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Linq;
using System.Collections.Generic;

namespace PandaBT.Compilation
{
	[Serializable]
	public class CompilationUnit 
	{
		public string sourceHash;
		public Token[] tokens;
		public Node[] nodes;
		public Node[] trees;

		public PandaScriptExceptionData[] exceptions;
		public int exceptionsCount => exceptions != null ? exceptions.Length : 0;
		
		[NonSerialized] internal Dictionary<int, Node> _tokenToNode;
		
		public Token GetToken( Node node)
		{
			return tokens[node.tokenIndex];
		}

		public Node GetNode(Token token)
		{
			if (nodes == null)
				return null;

			Node foundNode = null;

			if(_tokenToNode == null)
			{
				_tokenToNode = new Dictionary<int, Node>();
				foreach(var node in nodes)
					_tokenToNode[ tokens[node.tokenIndex].substring_start] = node;
			}

			while( token.type == TokenType.ExpressionParenthesis_Open)
			{
				int i = GetTokenIndex(token);
				if( i != -1 && i + 1 < tokens.Length )
					token = tokens[i + 1];
			}

			while (token.type == TokenType.ExpressionParenthesis_Closed)
			{
				int i = GetTokenIndex(token);
				if (i != -1 && i - 1 >= 0)
					token = tokens[i - 1];
			}

			if ( _tokenToNode.ContainsKey(token.substring_start) )
				foundNode = _tokenToNode[token.substring_start];

			return foundNode;
		}

		public int GetTokenIndex(Token token)
		{
			int index = -1;
			for (int i = 0; i < tokens.Length; i++)
			{
				if (tokens[i].substring_start == token.substring_start)
				{
					index = i;
					break;
				}
			}
			return index;
		}

		private Dictionary<Node, Node[]> _children = new Dictionary<Node, Node[]>();
		public Node[] GetChildren( Node node)
		{
			Node[] children = null;

			if( _children.ContainsKey(node))
			{
				children = _children[node];
			}
			else
			{
				children = _children[node] = node.children.Select(c => nodes[c]).ToArray();
			}

			return children;
		}

		
		 
	}
}

