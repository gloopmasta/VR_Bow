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
using System.Text;
using System.Collections.Generic;

namespace PandaBT.Compilation
{
	[Serializable]
	public class Node
	{
		[NonSerialized] public CompilationUnit cu;
		public int index;
		public int tokenIndex;
		public List<int> children = new List<int>();
		public int parent = -1;
		public List<Token> parameters = new List<Token>();
		public int parseLength = 0;
		public int indent = 0;


		public void Reset(CompilationUnit cu)
		{
			this.cu = cu;
			_parsedParameters = null;
			_toString = null;

		}

		public object[] _parsedParameters;
		public object[] parsedParameters
		{
			get
			{
				if (PandaBT.Runtime.BTProgram.current.boundState == BoundState.Unbound)
					_parsedParameters = null;

				if ( _parsedParameters == null)
				{
					List<object> parameters = new List<object>();
					foreach (var p in this.parameters)
						parameters.Add(PBTTokenizer.ParseParameter(p));
					_parsedParameters = parameters.ToArray();
				}
				return _parsedParameters;
			}
		}

		public Node()
		{
		}

		public void AddChild(Node child)
		{
			if( !children.Contains(child.index) )
				children.Add(child.index);
			child.parent = this.index;
		}

		private string _toString = null;
		public override string ToString()
		{
			// return base.ToString();

			if( _toString != null )
				return _toString;

			var sb = new StringBuilder();
			sb.Append(cu.tokens[tokenIndex]);

			if( parameters.Count > 0)
			{
				sb.Append("(");
				for(int i = 0; i < parameters.Count; i++)
				{
					var p = parameters[i];
					sb.Append(p.ToString());
					if( i+1 < parameters.Count)
						sb = sb.Append(", ");
				}
				sb.Append(")");
			}

			_toString = sb.ToString();
			return _toString;
		}
		




	}
	
}
