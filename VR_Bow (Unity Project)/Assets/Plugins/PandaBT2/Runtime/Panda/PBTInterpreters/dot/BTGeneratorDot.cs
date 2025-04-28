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

using System.Collections;
using System.Collections.Generic;

namespace PandaBT.Compilation
{
	public class BTGeneratorDot 
	{
	
		
		public static string PrintDotGraph (Node tree, CompilationUnit cu)
		{
			string strDot = "digraph G {\n";
			// Get all nodes
			var stack = new Stack<Node> ();
			var nodes = new List<Node>();
			
			stack.Push (tree);
			while (stack.Count > 0) 
			{
				var node = stack.Pop ();
				nodes.Add( node );
				var children = cu.GetChildren(node);
				for (int c = children.Length - 1; c >= 0; --c) 
				{
					var child = children [c];
					stack.Push (child);
				}
			}
			
			var nodeID = new Dictionary<Node, int>();
			for(int i=0; i < nodes.Count;++i)
			{
				nodeID[nodes[i]] = i;
			}
			
			
			// Define node label
			string strlabels = "";
			foreach( var node in nodes)
			{
				string name = nodeID[node].ToString();
				string label = cu.GetToken(node).ToString().Replace("\"", "").Replace("[", "").Replace("]", "");
				string style = DotStyle(node, cu);
				strlabels += string.Format("{0} [label=\"{1}\"{2}]\n", name, label, style);
			}
			strDot += strlabels;
			
			
			//Dot graph tree
			stack.Push (tree);
			string strParenting = "";
			while (stack.Count > 0) 
			{
				var node = stack.Pop ();
				var children = cu.GetChildren(node);
				foreach (var child in children) 
				{
					strParenting +=string.Format( "{0} -> {1}\n",
					                             nodeID[node] , nodeID[child]
					                             );
					
					stack.Push (child);
				}
			}
			strDot += strParenting;
			strDot += "\n}";
			return strDot;
		}
		
		static string DotStyle(Node node, CompilationUnit cu )
		{
			string style = "";
	
			switch( cu.GetToken(node).type  )
			{
			case TokenType.If: style = ",shape=diamond,style=filled,fillcolor=aliceblue"; break;
			case TokenType.IfElse: style = ",shape=diamond,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Before: style = ",shape=diamond,style=filled,fillcolor=aliceblue"; break;
			case TokenType.ParallelOperator:
			case TokenType.Parallel: style = ",color=red,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Tree: style = ",shape=doublecircle,style=filled,fillcolor=aliceblue"; break;
			case TokenType.TreeIdentifier: style = ",shape=doublecircle,style=filled,fillcolor=aliceblue"; break;
			case TokenType.FallbackOperator:
			case TokenType.Fallback: style = ",shape=parallelogram,style=filled,fillcolor=aliceblue"; break;
			case TokenType.SequenceOperator:
			case TokenType.Sequence: style = ",color=square,style=filled,fillcolor=aliceblue"; break;
			case TokenType.RaceOperator:
			case TokenType.Race: style = ",color=square,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Break: style = ",color=square,style=filled,fillcolor=aliceblue"; break;
			case TokenType.Word:
				style = ",shape=plaintext"; break;
			}
			
			return style;
		}
		
	}
}