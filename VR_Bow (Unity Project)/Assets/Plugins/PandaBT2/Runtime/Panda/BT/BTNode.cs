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
using System.Reflection;
using System.Runtime.Serialization;

using PandaBT.Compilation;

namespace PandaBT.Runtime
{
	public abstract class BTNode: System.IDisposable
	{

		public event System.Action OnTick;
		public event System.Action OnAbort;

		public string debugInfo = null;

		internal int m_lastTick = -1;
		public int lastTick{get{ return m_lastTick;}}

		public static BTNode currentNode
		{
			get
			{
				return BTProgram.current.currentNode;
			}
		}
		internal object[] i_parameters = null;
		internal object[] m_parameters = null;
		private object[] m_parameterValues = null;

		public int parameterCount
		{
			get
			{
				var n = 0;
				if( i_parameters != null )
					n = i_parameters.Length;
				return n;
			}
		}


		IVariable[] _resolvedVariableParameters;
		public IVariable[] resolvedVariableParameters
		{
			get
			{
				if( _resolvedVariableParameters == null && BTProgram.current?.boundState == BoundState.Bound)
				{

					_resolvedVariableParameters = new IVariable[parameterCount];

					for (int i = 0; i < parameterCount; i++)
					{
						_resolvedVariableParameters[i] = null;
						var i_parameter = i_parameters[i];
						if (i_parameter == null)
							continue;

						var vp = typeof(VariableParameter).IsAssignableFrom(i_parameter.GetType()) ? (VariableParameter)i_parameter : null;
						if( vp == null)
							continue;

						var vb = BTProgram.current.bindingInfo.variableBinders.Where(vb => vb.name == vp.name).FirstOrDefault();
						_resolvedVariableParameters[i] = vb?.variable;
					}
				}
				return _resolvedVariableParameters;
			}
		}

		public object[] parameters{
			get
			{
				var task = this as BTTask;
				var methodInfo = task?.m_taskBinder?.methodInfo;
				var methodParameters = task?.m_taskBinder?.methodParameters;


				bool isFirstPass = m_parameters == null;
				if (isFirstPass)
				{
					int n = -1;

					if (methodInfo != null && !task.m_taskBinder.hasParamArray)
					{
						n = methodParameters.Length;
					}
					else
					{
						n = (i_parameters != null ? i_parameters.Length : 0);
					}
					m_parameters = new object[n];
				}

				for (int i = 0; i < m_parameters.Length; i++)
					m_parameters[i] = GetParamValue(i, updateVariablesOnly: isFirstPass == false);

				return m_parameters;
			}
		}

		private object GetParamValue(int i, bool updateVariablesOnly)
		{
			var task = this as BTTask;
			var resolvedVars = resolvedVariableParameters;

			IVariable variable = resolvedVars != null && i < resolvedVars.Length  ? resolvedVars[i] : null;
			if ( variable != null)
			{
				// Only variables are to be updated after the first pass.
				var variableParameter = i_parameters[i] as VariableParameter;
				return variableParameter.isReference ? variable : variable.value;
			}

			if (updateVariablesOnly)
			{
				return m_parameters[i];
			}

			var methodInfo = task?.m_taskBinder?.methodInfo;
			var methodParameters = task?.m_taskBinder?.methodParameters;

			object value = null;

			EnumParameter enumParameter = null;
			enumParameter = i < i_parameters?.Length ? i_parameters[i] as EnumParameter : null;

			if (enumParameter != null)
			{
				if (task.boundState == BoundState.Bound)
				{
					value = enumParameter.Parse(methodParameters[i].ParameterType);
				}
				else
				{
					value = null;
				}
				return value;
			}

			value = i < i_parameters?.Length ? i_parameters[i] : methodParameters?[i].DefaultValue;
			return value;
		}

		public object[] ParameterValues
		{
			get
			{
				var task = this as BTTask;
				bool hasParamArray = task?.m_taskBinder != null ? task.m_taskBinder.hasParamArray : false;
					
				if (hasParamArray)
				{
					var methodParamCount = task.m_taskBinder.methodParameters.Length;
					m_parameterValues = new object[methodParamCount];
					var lastIndex = methodParamCount - 1;
					for (int i=0; i < lastIndex ; i++)
					{
						m_parameterValues.SetValue(GetParamValue(i, updateVariablesOnly: false), i);
					}
					m_parameterValues.SetValue(GetParamArrayFrom(lastIndex), lastIndex);
				}
				else
				{
					m_parameterValues = parameters;
				}
				return m_parameterValues;
			}
		}

		private Array GetParamArrayFrom(int startIndex)
		{
			var task = this as BTTask;
			var paramValues = parameters;
			var elementType = task.m_taskBinder.lastParameter.ParameterType.GetElementType();
			var paramArray = Array.CreateInstance(elementType, paramValues.Length - startIndex);
			for (int i = startIndex; i < paramValues.Length; i++)
			{
				paramArray.SetValue(paramValues[i], i - startIndex);
			}
			return paramArray;
		}

		internal System.Type[] _parameterTypes;
		internal System.Type[] parameterTypes
		{
			get
			{
				if ( _parameterTypes == null )
				{
					_parameterTypes = new System.Type[i_parameters.Length];
					for (int i = 0; i < i_parameters.Length; ++i)
					{
						var variable = i_parameters[i] as IVariable;
						if( variable == null)
						{
							var variableParameter = i_parameters[i] as VariableParameter;
							if (variableParameter != null)
								variable = variableParameter.ResolveVariable(BTProgram.current);
						}

						if (variable != null)
						{
							_parameterTypes[i] = variable.valueType;
						}
						else
						{
							_parameterTypes[i] = i_parameters[i]?.GetType();
						}
					}

				}
				return _parameterTypes;
			}
		}

		internal BTNode m_parent;
		public BTNode parent
		{
			get
			{
				return m_parent;
			}
		}
		
		internal Status m_status = Status.Running; // Initial value has to be different to Status.Ready to Reset();
		internal Status m_previousStatus = Status.Ready;
		public Status status
		{
			get
			{
				return m_status;
			}
		}

		public Status previousStatus
		{
			get
			{
				return m_previousStatus;
			}
		}

		protected abstract Status DoTick();
		protected abstract void DoReset();
		
		public abstract void AddChild( BTNode child);
		
		public Status Tick()
		{
			if (m_status == Status.Ready || m_status == Status.Running) // isTickable
			{
				BTNode previous = BTProgram.current.currentNode;
				BTProgram.current.currentNode = this;

				m_previousStatus = m_status;
				m_lastTick = BTProgram.current.tickCount;


				if (m_status != Status.Ready && m_nextReturnStatus != m_status)
					m_status = m_nextReturnStatus;
				else
					m_status = DoTick();

				if (m_status == Status.Ready)
					m_status = Status.Running;

				TriggerOnTick();

				BTProgram.current.currentNode = previous;
			}
			return m_status;
		}

		public void TriggerOnTick()
		{
			if (OnTick != null)
			{
				var previous = BTProgram.current.currentNode;
				BTProgram.current.currentNode = this;
				OnTick();
				BTProgram.current.currentNode = previous;
			}
		}


		public void Reset()
		{
			if ( m_status != Status.Ready ) // Ensure the node is reset only once.
			{
				BTNode previous =  BTProgram.current.currentNode;
				BTProgram.current.currentNode = this;

				debugInfo = null;

				m_previousStatus = m_status;
				m_status = Status.Ready;
				m_nextReturnStatus = Status.Running;

				DoReset();

				BTProgram.current.currentNode = previous;
			}
		}

		public void Abort()
		{
			if( m_status == Status.Running)
			{
				foreach(var child in children)
				{
					if( child.status == Status.Running)
						child.Abort();
				}

				m_status = Status.Aborted;
				OnAbort?.Invoke();
				// UnityEngine.Debug.Log($"ABORT {this.GetType().ToString()} ");
			}
		}


		internal Status m_nextReturnStatus = Status.Running;
		/// <summary>
		/// Succeed the task. The task will report Status.Succeeded on the next status return.
		/// </summary>
		public void Succeed()
		{
			m_nextReturnStatus = Status.Succeeded;
			if (BTProgram.current != null && BTProgram.current.currentNode == this)
				m_status = m_nextReturnStatus;
		}

		/// <summary>
		/// Fail the task. The task will report Status.Failed on the next status return.
		/// </summary>
		public void Fail()
		{
			m_nextReturnStatus = Status.Failed;
			if (BTProgram.current != null && BTProgram.current.currentNode == this)
				m_status = m_nextReturnStatus;
		}

		public abstract void Dispose();
		public abstract BTNode[] children{get;}


		internal virtual BTNodeState state
		{
			get
			{
				return new BTNodeState(this);
			}

			set
			{
#if !PANDA_BT_FREE
				this.m_status = value.status;
				this.m_previousStatus = value.previousStatus;
				this.m_nextReturnStatus = value.nextReturnStatus;
				this.m_lastTick = value.lastTick;
#endif
			}
		}

	}
}
