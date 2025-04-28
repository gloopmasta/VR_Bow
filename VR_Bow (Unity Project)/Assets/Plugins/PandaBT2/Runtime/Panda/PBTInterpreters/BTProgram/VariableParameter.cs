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

using System.Linq;

using PandaBT;

namespace PandaBT.Compilation
{
	public class VariableParameter
	{
		public VariableParameter(string tokenSubstr)
		{
			var firstChar = tokenSubstr[0];
			_name = tokenSubstr.Substring(1, tokenSubstr.Length - 1);
			if (firstChar == '@')
				_isReference = false;
			else if (firstChar == '&')
				_isReference = true;
			else
				throw new System.Exception($"Unexpected character '{firstChar}'");
		}

		public string name => _name;
		public bool isReference => _isReference;
		string _name;
		bool _isReference = false;

		public IVariable ResolveVariable(Runtime.BTProgram program)
		{
			IVariable variable = null;
			
			if ( program.boundState == BoundState.Bound)
			{
				var vb = program.bindingInfo.variableBinders.Where(vb => vb.name == name).First();
				variable = vb?.variable;
				if (variable == null)
				{
					throw new System.Exception($"No variable named `{name}` bound to BT program.");
				}
			}

			return variable;
		}

		public string rootName
		{
			get
			{
				processName();
				return _rootName;
			}
		}

		public string[] path
		{
			get
			{
				processName();
				return _path;
			}
		}

		string _rootName;
		string[] _path;

		void processName()
		{
			if(_rootName == null)
			{
				if (name.Contains("."))
				{
					_path = name.Split('.');
					_rootName = _path[0];
				}
				else
				{
					_path = new string[] { name };
					_rootName = name;
				}
			}
		}
	}
}
