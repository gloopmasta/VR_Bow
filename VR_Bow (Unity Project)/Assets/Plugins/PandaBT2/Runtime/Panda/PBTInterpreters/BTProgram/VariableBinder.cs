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
using System.Reflection;

using PandaBT.Compilation;

namespace PandaBT.Runtime
{
	public class VariableBinder : BTBinder
	{

		public override string name => variable.name;
		public IVariable variable;

		// Whether the variable match this task implementation
		public bool IsMatch(VariableParameter varParam)
		{
			bool itIs = false;

			if (varParam.rootName == this.variable.name)
				itIs = true;

			return itIs;
		}

		// Return the variables implemented om the given targets
		public static VariableBinder[] Get(object[] targets)
		{
			var returnedVariableBinders = new List<VariableBinder>();
			foreach(var target in targets)
			{
				IEnumerable<VariableBinder> variableBinders = Get(target);

				if (variableBinders != null)
					returnedVariableBinders.AddRange(variableBinders);
			}

			return returnedVariableBinders.ToArray();
		}

		public static IEnumerable<VariableBinder> Get(object target)
		{
			var btVariables = new List<IVariable>();

			var pandaVariables = GetPandaVariables(target)
				.Select(m => new BTVariable(m.memberInfo, target));

			if (pandaVariables.Count() > 0)
				btVariables.AddRange(pandaVariables);

			foreach (var variableFinderFn in _variablesFinders)
			{
				var foundVariables = variableFinderFn.Invoke(target);
				if (foundVariables?.Length > 0)
					btVariables.AddRange(foundVariables);
			}

			IEnumerable<VariableBinder> variableBinders = null;
			if (btVariables.Count > 0)
			{
				variableBinders = btVariables.Select(btVariable =>
				{
					var binder = new VariableBinder();
					binder.target = target;
					binder.variable = btVariable;
					binder.memberInfo = (btVariable as BTVariable)?.memberInfo;
					return binder;
				});
			}

			return variableBinders;
		}

		public static void RegisterVariableFinder( Func<object, IVariable[]> variableFinder )
		{
			if( !_variablesFinders.Contains(variableFinder))
				_variablesFinders.Add( variableFinder );
		}

		private static List<Func<object, IVariable[]>> _variablesFinders = new List<Func<object, IVariable[]>>();

		private static PandaBTMemberInfo[] GetPandaVariables(object target)
		{
			if (target == null)
				return null;
			var type = target.GetType();
			var variables = PandaBTReflectionDatabase.GetVariableMemberInfos(type);
			return variables;
		}

		public Type GetMemberType()
		{
			Type type = null;
			
			var propertyInfo = variable as PropertyInfo;
			var fieldInfo = variable as FieldInfo;
			
			if (propertyInfo != null)
				type = propertyInfo.PropertyType;
			
			if (fieldInfo != null)
				type = fieldInfo.FieldType;

			return type;
		}

		string _toString = null;
		public override string ToString()
		{
			if (_toString == null)
			{
				var typeName = this.target.GetType().Name;
				var t = this;
				var sb = new System.Text.StringBuilder();
				sb.Append(typeName);
				sb.Append("/");

				sb.AppendFormat("{0}", t.variable.name);

				_toString = sb.ToString();

				_toString = _toString.Replace("System.Single", "float");
				_toString = _toString.Replace("System.Int32", "int");
				_toString = _toString.Replace("System.Boolean", "bool");
				_toString = _toString.Replace("System.String", "string");

				_toString = _toString.Replace("Single", "float");
				_toString = _toString.Replace("Int32", "int");
				_toString = _toString.Replace("Boolean", "bool");
				_toString = _toString.Replace("String", "string");

			}
			return _toString;
		}

	}
}
