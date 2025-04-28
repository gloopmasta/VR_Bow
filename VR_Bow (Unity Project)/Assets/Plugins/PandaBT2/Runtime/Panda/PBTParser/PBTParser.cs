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
	
	public class PBTParser
	{
	
		public static Node[] ParseTokens(CompilationUnit cu)
		{

			var exceptions = new List<PandaScriptException>();
			try
			{
				cu.nodes = ParseTokensToNodes(cu);

			} catch( PandaScriptException e)
			{
				exceptions.Add(e);
			}

			try
			{
				if( cu.nodes != null)
				{
					cu.trees = ResolveNodeParenting(cu);
					foreach (var r in cu.trees)
						CheckTree(r, cu);

					// Check orphan nodes. Each nodes must have a parent excepted the root nodes.
					var children = cu.nodes.Where(n => !cu.trees.Contains(n));
					foreach( var child in children)
					{
						if( child.parent == -1)
						{
							var token = cu.GetToken(child);
							var msg = $"Could not resolve parent of node `{token.substring}`.";
							throw new PandaScriptException(msg, PBTTokenizer.filepath, token.line);
						}
					}
				}

			} catch ( PandaScriptException e)
			{
				exceptions.Add(e);
			}

			cu.exceptions = exceptions.Select(e =>
			{
				var data = new PandaScriptExceptionData();
				data.message = e.Message;
				data.filePath = e.filePath;
				data.lineNumber = e.lineNumber;
				return data;

			}).ToArray();

			return cu.trees;
		}

		public static Node[] ParseTokensToNodes(CompilationUnit cu )
		{
			CheckParenthesis(cu);
			var indent = 0;
			if( cu.tokens== null || cu.tokens.Length == 0)
			{
				string msg = string.Format("Invalid panda script.");
				throw new PandaScriptException(msg, null, -1);
			}

			var nodes = new List<Node>();
			Node lastNode = null;
			bool param_parenthesis_opened = false;
			int expression_parenthesis_count = 0;
			for ( int i=0; i < cu.tokens.Length; ++i)
			{
				var t = cu.tokens[i];
				var node = new Node();
				node.cu = cu;
				node.tokenIndex = i;
				node.indent = indent;
				switch( t.type )
				{
				// Indentation control
				case TokenType.Indent:
					if( expression_parenthesis_count == 0 ) // (indentation is ignored inside expressions)
						++indent;
					break;
				case TokenType.EOL:
					indent=0;
					lastNode = null;
					break;

				case TokenType.VariableByValue:
				case TokenType.VariableByRef:
				case TokenType.Value:
						//lastNode.parameters.Add(t);
				   break;
				
				case TokenType.ParamParenthesis_Open:
					if( !param_parenthesis_opened )
					{
						param_parenthesis_opened = true;
					}else
					{
						string msg = "Unexpected open parenthesis.";
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;
					
				case TokenType.ParamParenthesis_Closed:
					if( param_parenthesis_opened )
					{
						param_parenthesis_opened = false;
					}else
					{
						string msg = "Unexpected closed parenthesis.";
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					var lastNodeToken = cu.GetToken( lastNode);
					lastNode.parseLength = t.substring_start - lastNodeToken.substring_start + t.substring_length;
					break;
					// Tree node
				case TokenType.Tree:
						if (indent == 0 )
						t.type = TokenType.Tree;
					else
						t.type = TokenType.TreeReference;
					break;
				case TokenType.TreeIdentifier:
					if (indent == 0)
						t.type = TokenType.TreeDefinition;
					else
						t.type = TokenType.TreeCall;
					break;
				case TokenType.ExpressionParenthesis_Open:
					expression_parenthesis_count++;
					break;
				case TokenType.ExpressionParenthesis_Closed:
					expression_parenthesis_count--;
					break;

				}// switch


				// Skip blanks
				if ( t.type == TokenType.EOL || t.type == TokenType.Indent)
					continue;
					
				// Ignore comments
				if( t.type == TokenType.Comment )
					continue;
				
				// Skip parameter parenthesis and coma
				if ( t.type == TokenType.ParamParenthesis_Open || t.type == TokenType.ParamParenthesis_Closed || t.type == TokenType.Coma )
					continue;

				// Skip expression parenthesis and coma
				if (t.type == TokenType.ExpressionParenthesis_Open || t.type == TokenType.ExpressionParenthesis_Closed )
					continue;

				if ( param_parenthesis_opened )
				{
					if(lastNode != null)
					{
						lastNode.parameters.Add( t );
					}	
				}
				else
				{

					if (t.type == TokenType.Value || t.type == TokenType.VariableByValue || t.type == TokenType.VariableByRef)
					{
						if (lastNode != null)
						{
							lastNode.parameters.Add(t);
							var lastNodeToken = cu.GetToken(lastNode);
							lastNode.parseLength = t.substring_start - lastNodeToken.substring_start + t.substring_length;
							continue;
						}
						else
						{
							string msg = "Unexpected parameter value.";
							throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
						}
					}
					
					lastNode = node;
					nodes.Add(node);
				}
			}
			
			for(int i = 0; i < nodes.Count; i++)
				nodes[i].index = i;

			return nodes.ToArray();
		}

		public static Node[] ResolveNodeParenting(CompilationUnit cu)
		{

			var expressions = ExpressionParser.ParseToken(cu.tokens);
			foreach(var expression in expressions)
				ExpressionParser.ParseExpression(expression, cu);


			// Excepted for the roots, each node must have a parent.

			// parenting rules:
			// for each node:
			//  - If there are non full parents on the same line, its parent is the first non full parent to the left
			//  - owtherwise, the first non full parent starting from the right up the indentation
			//  
			// In other words:
			// For a given node, its parent is either on the same line to its left or on first line with a lower indentation up above.
			// If there are multiple structural nodes on the same line, the parent is the first non full one starting from the right.

			List<Node> roots = new List<Node>();

			var topExpressionNodes = expressions
				.Select( e => {
					Node firstNode = null;
					foreach(var token in e.tokens)
					{
						firstNode = cu.GetNode(token);
						if(firstNode != null)
							break;
					}

					if( firstNode == null)
					{
						throw new PandaScriptException("Empty expression", PBTTokenizer.filepath, e.startToken.line);
					}

					var topParent = ExpressionParser.GetTopParentOrNull(firstNode, cu);
					return topParent != null ? topParent : firstNode;
				})
				.ToArray();

			var nodes = cu != null && cu.nodes != null ? cu.nodes.Where(n => !IsPartOfExpression(n, expressions, cu)).ToList(): new List<Node>();
			nodes.AddRange( topExpressionNodes );
			nodes.Sort((a,b) => cu.GetToken(a).substring_start.CompareTo(cu.GetToken(b).substring_start) );

			// Group node by line number
			var lines = new SortedDictionary<int, List<Node> >();
			foreach( var node in nodes)
			{
				int lineNumber = cu.GetToken(node).line;
				if (!lines.ContainsKey(lineNumber))
					lines[lineNumber] = new List<Node>();
				lines[lineNumber].Add(node);
			}

			foreach(var node in nodes)
			{
				var l = cu.GetToken(node).line;
				var i = node.indent;
				var lineOrder = lines[l].FindIndex(n => n == node);

				// no indentation and first on the line => root node, no parent
				if( i == 0 && lineOrder == 0)
				{
					roots.Add( node );
					continue;
				}

				Node parent = null;
				var parentLine = l; // line where the parent is

				List<Node> lineNodes;
				bool sameLineHasParent = false;

				lineNodes = lines[parentLine];
				sameLineHasParent = false;
				if ( cu.GetToken(node).line == parentLine && lineNodes[0] != node)
				{
					foreach (var n in lineNodes)
					{
						if (n != node && MaxChildrenCount( cu.GetToken(n).type) > 0)
						{
							sameLineHasParent = true;
							break;
						}
					}
				}

				if (!sameLineHasParent)
				{
					// find the line having a nodes with lower indent;
					do
					{
						// decrease line number to a valid one
						do
						{
							parentLine--;
						} while (parentLine >= 0 && !lines.ContainsKey(parentLine));

						if (parentLine >= 0)
							lineNodes = lines[parentLine];
					} while (parentLine >= 0 && lineNodes.Count > 0 && lineNodes[0].indent >= i);
				}


				if( parentLine >=0)
				{
					lineNodes = lines[parentLine];
					if (lineNodes.Count > 0)
					{
						parent = lines[parentLine][0];
						// Get the first non-full parent to the left
						bool isSameLine = cu.GetToken(node).line == parentLine;

						int p = lineNodes.Count - 1;
						if ( isSameLine) // start from the left of the node
							p = lineNodes.FindIndex( n => n == node) - 1;

						for(; p >= 0;  p--)
						{
							var candidate = lineNodes[p];
							var maxChildCount = MaxChildrenCount( cu.GetToken(candidate).type);
							var isStructural = maxChildCount > 0;
							var isSelf = candidate == node;
							var canAcceptChild  = candidate.children.Count < maxChildCount;

							if (isStructural && !isSelf && canAcceptChild)
							{
								parent = candidate;
								break;
							}
						}
					}
				}

				if( parent != null)
				{
					parent.AddChild(node);
				}
			}
			return roots.ToArray();
		}

		public static Node[] GetTreeReferences(Node tree, CompilationUnit cu, CompilationUnit[] cunits)
		{
			// Get all references
			var stack = new Stack<Node>();
			var treeRefs = new List<Node>();
			stack.Push(tree);
			while (stack.Count > 0)
			{
				var node = stack.Pop();
				if (node == null)
					continue;

				if( cu.GetToken(node).type == TokenType.TreeReference)
					treeRefs.Add(node);

				var children = cu.GetChildren(node);
				for (int c = children.Length - 1; c >= 0; --c)
				{
					var child = children[c];
					stack.Push(child);
				}
			}
			return treeRefs.ToArray();
		}

		public static Node[] GetTreeReferences(CompilationUnit cu, CompilationUnit[] cus)
		{
			var nodes = new List<Node>();
			if (cu.trees != null)
			{
				foreach (var b in cu.trees)
					nodes.AddRange(GetTreeReferences(b, cu, cus));
			}
			return nodes.ToArray();
		}
		

		public static void CheckTreeReferences(CompilationUnit cu, CompilationUnit[] cunits )
		{
			CheckTreeReferenceDefinitions(cu, cunits);
			CheckCircularDefinition(cu, cunits);
		}

		public static void CheckTreeNames(CompilationUnit cu, CompilationUnit[] cus)
		{
			if (cu == null || cus == null)
				return;

		   for (int r = 0; r < cu.trees.Length; ++r )
			{
				var tree = cu.trees[cu.trees.Length - r - 1];

				if (tree.parameters.Count > 0 && tree.parsedParameters[0].GetType().IsAssignableFrom(typeof(IVariable)))
				{
					string msg = string.Format("Invalid tree name. Tree can not be defined with a variable name.");
					throw new PandaScriptException(msg, PBTTokenizer.filepath, cu.GetToken(tree).line);
				}

				var treeName = GetTreeName(tree);
				if( string.IsNullOrEmpty(treeName))
				{
					string msg = string.Format("Invalid tree name", treeName);
					throw new PandaScriptException(msg, PBTTokenizer.filepath, cu.GetToken(tree).line);
				}

				for (int k = 0; k < cus.Length; ++k)
				{
					var i = cus.Length - k - 1;
					if (cus[i] == null || cus[i].trees == null)
						continue;

					for (int l = 0; l < cus[i].trees.Length; ++l)
					{
						var j = cus[i].trees.Length - l - 1;

						var other = cus[i].trees[j];
						if (tree == other)
							continue;
						var otherName = GetTreeName(other);
						if (otherName == treeName)
						{
							string msg = string.Format("Tree \"{0}\" is already defined.", treeName);
							throw new PandaScriptException(msg, PBTTokenizer.filepath, cu.GetToken(tree).line);
						}
					}
				}
			}
		}
		public static void CheckMains(CompilationUnit cu, CompilationUnit[] cus)
		{
			if (cu.trees == null || cus == null)
				return;

			for (int r = 0; r < cu.trees.Length; ++r)
			{
				var tree = cu.trees[cu.trees.Length - r - 1];
				var treeName = GetTreeName(tree);

				if (treeName.ToLower() != "root")
					continue;

				for (int k = 0; k < cus.Length; ++k)
				{
					var i = cus.Length - k - 1;
					if (cus[i].trees == null)
						continue;

					for (int l = 0; l < cus[i].trees.Length; ++l)
					{
						var j = cus[i].trees.Length - l - 1;

						var other = cus[i].trees[j];
						if (tree == other)
							continue;

						var otherName = GetTreeName(other);
						if (otherName.ToLower() == "root")
						{
							string msg = string.Format("Tree \"{0}\" is already defined.", treeName);
							throw new PandaScriptException(msg, PBTTokenizer.filepath, cu.GetToken(tree).line);
						}
					}
				}
			}
		}

		private static Node ResolveTreeReference(string refName, CompilationUnit[] cus)
		{
			Node treeRef = null;
			foreach(var cu in cus)
			{
				if( cu != null && cu.trees != null )
				{
					foreach(var bh in cu.trees)
					{
						if(bh != null )
						{
							string name = GetTreeName(bh);
							if(name == refName)
							{
								treeRef = bh;
								break;
							}
						}
					}
				}

				if (treeRef != null)
					break;
			}

			return treeRef;
		}


		private static void CheckTreeReferenceDefinitions(CompilationUnit cu, CompilationUnit[] cunits)
		{
			// Check whether all tree references are defined
			var treeRefs = PBTParser.GetTreeReferences(cu, cunits);
			foreach (var treeRef in treeRefs)
			{
				// skip variable tree name
				bool isVariable = treeRef.parsedParameters.Length > 0 && treeRef.parsedParameters[0].GetType().IsAssignableFrom(typeof(IVariable));
				if (isVariable)
					continue;

				bool isDefined = false;
				string refName = GetTreeName(treeRef);

				Node resolved = ResolveTreeReference(refName, cunits);
				isDefined = resolved != null;

				if (!isDefined)
				{
					string msg = string.Format("Tree \"{0}\" is not defined.", refName);
					throw new PandaScriptException(msg, PBTTokenizer.filepath, cu.GetToken( treeRef).line);
				}
			}
		}

		private static void CheckCircularDefinition(CompilationUnit cu, CompilationUnit[] cunits)
		{
			
			if (cu.trees == null)
				return;

			for (int i = 0; i < cu.trees.Length; ++i)
			{
				var root = cu.trees[i];
				CheckCircularDefinition(root, cu, cunits);
			}

		}

		private static void CheckCircularDefinition(Node root, CompilationUnit cu, CompilationUnit[] cus)
		{
			var stack = new Stack<Node>();
			var depths = new Stack<int>();
			var path = new Stack<Node>();

			if (root != null)
			{
				stack.Push(root);
				depths.Push(0);
			}

			while (stack.Count > 0)
			{
				var tree = stack.Pop();
				var depth = depths.Pop();

				while( path.Count > depth )
					path.Pop();

				path.Push(tree);
				

				var treeRefs = GetTreeReferences(tree, cu, cus );
				foreach (var treeRef in treeRefs)
				{
					// resolve
					var refName = treeRef.parameters[0].ToString().Trim();
					Node targetTree = ResolveTreeReference(refName, cus);

					if (!path.Contains(targetTree))
					{
						stack.Push(targetTree);
						depths.Push(depth + 1);
					}
					else
					{

						path.Push(targetTree);

						var treeName = GetTreeName(targetTree);
						string msg = string.Format("Tree \"{0}\" is circularly defined. Circular tree definition is invalid.\n", treeName);

						string callPath = "";
						var pathArray = path.ToArray();
						for (int i = 0; i < pathArray.Length; ++i)
						{
							var j = pathArray.Length - i - 1;

							var n = pathArray[j];

							var name = GetTreeName(n);
							callPath += string.Format("/{0}", name);
						}
						msg += string.Format("call path: \"{0}\" ", callPath);

						var cuSubtree = cus.Where(c => c.trees.Where(t => t == targetTree).FirstOrDefault() != null).FirstOrDefault();
						throw new PandaScriptException(msg, PBTTokenizer.filepath, cuSubtree.GetToken(targetTree).line);
					}
				}
			}

		}

		private static string GetTreeName(Node tree)
		{
			return PBTTokenizer.ParseParameter(tree.parameters[0]).ToString();
		}
		
		static void CheckTree(Node root, CompilationUnit cu)
		{
			var flattenChildren = new List<Node>();
			var fifo = new Queue<Node>();
			fifo.Enqueue(root);
			while(fifo.Count > 0)
			{
				var node = fifo.Dequeue();
				var children = cu.GetChildren(node);
				flattenChildren.AddRange( children );
				foreach (var child in children)
					fifo.Enqueue(child);
			}

			var nodes = flattenChildren;
			foreach( var n in nodes)
			{
				var t = cu.GetToken(n);
				switch( t.type )
				{
				case TokenType.Word:
					// Action has no child
					if (n.children.Count != 0)
					{
						string msg = string.Format("Task node has {0} children. None is expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;
				
				case TokenType.While:
					// While node must have 2 nodes
					if (n.children.Count != 2)
					{
						string msg = string.Format("While node has {0} children. Two are expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;
	
				case TokenType.If:
					// This node must have one child.
					if (n.children.Count != 2)
					{
						string msg = string.Format("If-node has {0} children. Two are expected", n.children.Count );
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}

					break;

				case TokenType.IfElse:
					// This node must have 3 children.
					if (n.children.Count != 3)
					{
						string msg = string.Format("IfElse-node has {0} children. Three are expected", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}

					break;

				case TokenType.Before:
					// This node must have one child.
					if (n.children.Count != 2)
					{
						string msg = string.Format("Before-node has {0} children. Two are expected", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}

					break;

					case TokenType.Parallel:
				// Parallel node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Parallel node has no child. One or more is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}			
					break;
					
				case TokenType.Tree:

					if (n.parameters.Count != 1)
					{
						string msg = string.Format("Tree naming error. Tree name is expected as parameter of type string.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}

					// Root node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Tree node has no child. One is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					
					// Root node must have one child and it must be a task.
					if( n.children.Count > 1 )
					{
						string msg = string.Format("Tree node has too many children. Only one is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;
				
				case TokenType.TreeReference:

					if (n.parameters.Count != 1)
					{
						string msg = string.Format("Tree naming error.The Tree name is expected as parameter of type string.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}

					// Root node must have one child and it must be a task.
					if (n.children.Count > 0)
					{
						string msg = string.Format("Tree reference has children. None is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}

				break;
					
				case TokenType.Fallback:
					// Fallback node must have at least one child
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Fallback node has no child. One or more is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}			
					break;
					
				case TokenType.Sequence:
					// Sequence node must have one child and it must be a task.
					if( n.children.Count == 0 )
					{
						string msg = string.Format("Sequence node has no child. One or more is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;

				case TokenType.Race:
					// Race node must have one child and it must be a task.
					if (n.children.Count == 0)
					{
						string msg = string.Format("Race node has no child. One or more is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;

				case TokenType.Random:
					// Random node must have one child and it must be a task.
					if (n.children.Count == 0)
					{
						string msg = string.Format("Random node has no child. One or more is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;

				case TokenType.Shuffle:
					// Shuffle node must have one child and it must be a task.
					if (n.children.Count == 0)
					{
						string msg = string.Format("Shuffle node has no child. One or more is expected.");
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;


					case TokenType.Repeat:
					// Repeat node must have one child and it must be a task.
					if (n.children.Count != 1)
					{
						string msg = string.Format("Repeat node has {0} children. One is expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;

				case TokenType.Retry:
					// Retry node must have one child and it must be a task.
					if (n.children.Count != 1)
					{
						string msg = string.Format("Retry node has {0} children. One is expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;

					case TokenType.Not:
					// Not node must have one child.
					if (n.children.Count != 1)
					{
						string msg = string.Format("Not node has {0} children. One is expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;

				case TokenType.Mute:
					// Mute node must have one child.
					if (n.children.Count != 1)
					{
						string msg = string.Format("Mute node has {0} children. One is expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;
				case TokenType.Break:
					// Break has no child.
					if (n.children.Count > 0)
					{
						string msg = string.Format("Break node has {0} children. None is expected.", n.children.Count);
						throw new PandaScriptException(msg, PBTTokenizer.filepath, t.line);
					}
					break;
				}
			}
			
		}

		private static int MaxChildrenCount(TokenType structural)
		{
			int count = -1;
			switch(structural)
			{
				case TokenType.Fallback:
				case TokenType.Sequence:
				case TokenType.Parallel:
				case TokenType.Random:
				case TokenType.Shuffle:
				case TokenType.Race:
					count = int.MaxValue;
					break;
				case TokenType.IfElse:
					count = 3;
					break;
				case TokenType.While:
				case TokenType.If:
				case TokenType.Before:
					count = 2;
					break;
				case TokenType.Tree:
				case TokenType.TreeDefinition:
				case TokenType.Repeat:
				case TokenType.Retry:
				case TokenType.Mute:
				case TokenType.Not:
					count = 1;
					break;
				case TokenType.TreeIdentifier:
				case TokenType.TreeCall:
				case TokenType.Word:
				case TokenType.Break:
					count = 0;
					break;

			}
			return count;

		}

		private static bool IsPartOfExpression(Node node, IEnumerable<Expression> expression, CompilationUnit cu)
		{
			bool itIs = false;
			var token = cu.GetToken(node);
			foreach(Expression e in expression)
			{
				itIs = e.startToken.substring_start < token.substring_start && token.substring_start <= e.endToken.substring_start;
				if (itIs) break;
			}
			return itIs;
		}

		private static void CheckParenthesis(CompilationUnit cu)
		{
			int ppcount = 0; // parameter paretnthesis count
			int epcount = 0; // expression parenthesis count
			
			foreach(var t in cu.tokens)
			{
				if( t.type == TokenType.ParamParenthesis_Open)
				{
					ppcount++;
					continue;
				}

				if (t.type == TokenType.ParamParenthesis_Closed)
				{
					ppcount--;
					if (ppcount < 0)
					{
						throw new PandaScriptException("Unexpected closed parenthesis", PBTTokenizer.filepath, t.line);
					}
					continue;
				}

				if (t.type == TokenType.ExpressionParenthesis_Open)
				{
					epcount++;
					if( ppcount > 0)
					{
						throw new PandaScriptException("Unexpected open parenthesis", PBTTokenizer.filepath, t.line);
					}
					continue;
				}

				if (t.type == TokenType.ExpressionParenthesis_Closed)
				{
					epcount--;
					if( epcount < 0)
					{
						throw new PandaScriptException("Unexpected closed parenthesis", PBTTokenizer.filepath, t.line);
					}
					continue;
				}

				if( t.type == TokenType.Parenthesis_Open)
				{
					throw new PandaScriptException("Unexpected open parenthesis", PBTTokenizer.filepath, t.line);
				}

				if (t.type == TokenType.Parenthesis_Closed)
				{
					throw new PandaScriptException("Unexpected closed parenthesis", PBTTokenizer.filepath, t.line);
				}

				if( t.type == TokenType.EOL)
				{
					if( ppcount > 0 || epcount > 0 )
						throw new PandaScriptException("Closed parenthesis expected befored end-of-line", PBTTokenizer.filepath, t.line - 1);
				}

			}
		}
	}
}

