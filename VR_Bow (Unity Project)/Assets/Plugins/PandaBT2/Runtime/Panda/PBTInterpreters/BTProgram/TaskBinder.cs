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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using PandaBT.Compilation;

namespace PandaBT.Runtime
{
	public class TaskBinder : BTBinder
	{
		override public string name => memberInfo.Name;

		// The type of each task parameter
		System.Type[] _parameterTypes;
		public System.Type[] methodParameterTypes
		{
			get
			{
				if (_parameterTypes == null)
				{
					var list = new List<System.Type>();
					if (methodInfo != null)
					{
						var pars = methodParameters;
						list.AddRange(pars.Select(p => p.ParameterType));
					}
					_parameterTypes = list.ToArray();
				}
				return _parameterTypes;
			}
		}

		private bool _methodInfoInitialized = false;
		private MethodInfo _methodInfo;
		private ParameterInfo[] _methodParameterInfos;
		public MethodInfo methodInfo
		{
			get
			{
				InitMethodInfo();
				return _methodInfo;
			}
		}

		private void InitMethodInfo()
		{
			if (!_methodInfoInitialized)
			{
				_methodInfo = memberInfo as MethodInfo;
				_methodParameterInfos = _methodInfo?.GetParameters();
				_methodInfoInitialized = true;
			}
		}

		public ParameterInfo[] methodParameters
		{
			get
			{
				InitMethodInfo();
				return _methodParameterInfos;
			}
		}

		bool _hasParamArray = false;
		bool _hasParamArrayInitialized = false;
		public bool hasParamArray
		{
			get
			{
				if (!_hasParamArrayInitialized)
				{
					_hasParamArray = false;
					if (methodInfo != null)
					{
						var lastParameter = methodParameters.LastOrDefault();
						if (lastParameter != null)
							_hasParamArray = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
					}
					_hasParamArrayInitialized = true;
				}
				return _hasParamArray;
			}
		}

		bool _lastParameterDefined = false;
		ParameterInfo _lastParameter = null;
		public ParameterInfo lastParameter
		{
			get
			{
				if( !_lastParameterDefined)
				{
					if( methodInfo != null)
						_lastParameter = methodParameters.LastOrDefault();
					_lastParameterDefined = true;
				}
				return _lastParameter;
			}
		}

		// Whether the task match this task implementation
		public bool IsMatch(BTTask task, VariableBinder[] variableBinders)
		{
			if (task.name != memberInfo.Name)
				return false;

			bool itIs = true;

			var method = memberInfo as MethodInfo;
			var field = memberInfo as FieldInfo;
			var property = memberInfo as PropertyInfo;

			if (method != null)
			{
				itIs = IsMethodMatch(task, variableBinders);
			}
			else if (field != null)
			{
				if (field.FieldType != typeof(bool))
					itIs = false;
			}
			else
			{
				if (field != null && property.PropertyType != typeof(bool))
				{
					if (property.PropertyType != typeof(bool))
						itIs = false;
				}
			}

			return itIs;
		}

		// Return the tasks implemented om the given implementers
		public static TaskBinder[] Get(object[] targets)
		{
			var taskBinders = new List<TaskBinder>();
			foreach(var target in targets)
			{
				var binders = Get(target);
				if (binders != null)
					taskBinders.AddRange(binders);
			}

			return taskBinders.ToArray();
		}

		public static IEnumerable<TaskBinder> Get(object target)
		{
			var pandaTasks = GetPandaTasks(target);

			IEnumerable<TaskBinder> binders = null;
			if (pandaTasks.Length > 0)
			{
				binders = pandaTasks.Select(t =>
				{
					var binder = new TaskBinder();
					binder.target = target;
					binder.memberInfo = t.memberInfo;
					return binder;
				});
			}

			return binders;
		}

		private static PandaBTMemberInfo[] GetPandaTasks(object target)
		{
			var type = target.GetType();
			var pandaTasks = PandaBTReflectionDatabase.GetTaskMemberInfos(type);
			return pandaTasks;
		}

		// Whether this binder matches the task
		private bool IsMethodMatch(BTTask task, VariableBinder[] variableBinders)
		{
			var itIs = true;
			if ( task.parameterTypes.Length <= methodParameterTypes.Length || hasParamArray)
			{
				// Check if the defined parameters matches
				for (int i = 0; i < task.parameterTypes.Length; i++)
				{
					bool paramsMatches = IsMethodParamMatch(task, i, variableBinders);
					if (!paramsMatches)
					{
						itIs = false;
						break;
					}
				}

				if (task.parameterTypes.Length < methodParameterTypes.Length)
				{
					// Check if all method params after task params have default value
					if( methodParameters != null)
					{
						for (int i = task.parameterTypes.Length; i < methodParameters?.Length; i++)
						{
							var param = methodParameters[i];
							if (!param.HasDefaultValue)
							{
								itIs = false;
							}
						}
					}
					else
					{
						itIs = false;
					}
				}

			}
			else
			{
				itIs = false;
			}

			return itIs;
		}

		private bool IsMethodParamMatch(BTTask task, int i, VariableBinder[] variableBinders)
		{
			Type methodParamType = null;

			if( i + 1 < methodParameterTypes.Length || !hasParamArray && i + 1 == methodParameterTypes.Length)
			{
				methodParamType = methodParameterTypes[i];
			}
			else if (hasParamArray && i + 1 >= methodParameterTypes.Length )
			{
				methodParamType = lastParameter.ParameterType.GetElementType();
			}else
			{
				return false;
			}
			
			var taskParamType = task.parameterTypes[i];
			var underlyingType = System.Nullable.GetUnderlyingType(methodParamType);

			bool paramsMatches = true;
			var variableParameter = task.i_parameters[i] as VariableParameter;
			Type variableParameterType = variableParameter != null && variableParameter.isReference == false ? GetVariableParameterType(variableBinders, variableParameter) : null;
			if (methodParamType.IsEnum && (taskParamType == typeof(EnumParameter) 
				|| variableParameterType != null && variableParameterType == typeof(EnumParameter)) )
			{
				EnumParameter enumParameter = (EnumParameter)task.i_parameters[i];
				paramsMatches = enumParameter.Matches(methodParamType);
			}else
			if (variableParameter != null)
			{
				

				if(variableParameter.isReference)
				{
					paramsMatches = typeof(IVariable).IsAssignableFrom(methodParamType);
				}
				else
				{
					

					if (variableParameterType != null)
					{
						paramsMatches = CheckParameter(variableParameterType, methodParamType);
					}
					else
					{
						paramsMatches = false;
					}

				}
			}
			else
			if( taskParamType == null && (!methodParamType.IsValueType || underlyingType != null)  )
			{
				paramsMatches = true;
			}
			else
			{
				paramsMatches = CheckParameter(taskParamType, methodParamType);
			}

			return paramsMatches;
		}

		private static Type GetVariableParameterType(VariableBinder[] variableBinders, VariableParameter variableParameter)
		{
			var variable = variableBinders.Where(vb => vb.name == variableParameter.name).FirstOrDefault()?.variable;
			var variableParemeterType = variable != null ? variable.valueType : null;
			return variableParemeterType;
		}

		bool CheckParameter(Type taskParamType, Type methodParamType)
		{
			bool paramsMatches = false;

			var underlyingType = System.Nullable.GetUnderlyingType(methodParamType);
			paramsMatches = methodParamType == taskParamType || underlyingType == taskParamType;

			if (paramsMatches == false && methodParamType.IsValueType == false)
			{
				paramsMatches = methodParamType != null && methodParamType.IsAssignableFrom(taskParamType)
				|| underlyingType != null && underlyingType.IsAssignableFrom(taskParamType);
			}

			return paramsMatches;
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
				
				sb.AppendFormat("{0}", t.memberInfo.Name);
				if (t.methodParameterTypes.Length > 0)
					sb.Append("( ");
				for(int i=0; i <  t.methodParameterTypes.Length; i++)
				{
					var p = methodParameterTypes[i];
					if( i > 0)
						sb.Append(", ");
					if (p.IsEnum)
					{
						sb.AppendFormat("{0}", p.FullName.Replace("+", "."));
					}
					else
						sb.AppendFormat("{0}", p.Name);
				}
				if (t.methodParameterTypes.Length > 0)
					sb.Append(" )");

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


