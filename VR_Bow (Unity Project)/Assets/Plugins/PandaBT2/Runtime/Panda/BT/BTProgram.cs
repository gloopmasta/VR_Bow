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
using System.Collections;
using System.Collections.Generic;

using System.Reflection;

using PandaBT.Compilation;
using PandaBT.Runtime;
namespace PandaBT.Runtime
{

	// BT Program
	public class BTProgram : System.IDisposable
	{
		internal bool isInspected = false;
		internal static BTProgram current = null;
		internal string sourcesHash = null;
		internal BTNode currentNode = null;

		internal RuntimeUnit[] _runtimeUnits;

		public BTSource[] btSources;
		public bool repeatRetryOnSameTick = false;

		protected BindingInfo _bindingInfo;
		public BindingInfo bindingInfo => _bindingInfo;


		public RuntimeUnit[] runtimeUnits { get { return _runtimeUnits; } }

		internal BTTree _root;
		int _tickCount = 0;
		public int tickCount { get { return _tickCount; } }

		internal IVariable[] i_variables;
		public IVariable[] variables 
		{ 
			get
			{ 
				return (IVariable[])(i_variables != null ? i_variables.Clone() : null); 
			} 
		}


		public BTTree main 
		{ 
			get
			{  
				if( _root == null && _runtimeUnits != null )
				{
					for(int i=0; i < _runtimeUnits.Length; ++i)
					{
						var ru = _runtimeUnits[i];
						if (ru != null)
						{
							for (int j = 0; j < ru.trees.Length; ++j)
							{
								var b = ru.trees[j];
								if ( b.name != null && b.name.ToLower() == "root" || _root == null)
									_root = b;
							}
						}
					}
				}
				return _root;
			} 
		}

		internal CodeMap[] _codemaps;
		public CodeMap[] codemaps { get{return _codemaps;}}

		internal System.Exception[] _exceptions = null;
		public System.Exception[] exceptions 
		{ 
			get 
			{ 
				if (_exceptions == null)
					_exceptions = new System.Exception[0];

				return _exceptions; 
			} 
		}

		bool hasThrownExceptionOnTick = false;

		public Status status
		{
			get
			{
				var status = Status.Failed;
				if (main != null)
					status = main.m_status;
				return status;

			}
		}

		public BTProgram()
		{
			hasThrownExceptionOnTick = false;
		}

		public BTProgram( System.Exception[] exceptions)
		{
			_exceptions = exceptions;
			_boundState = BoundState.Failed;
			hasThrownExceptionOnTick = false;
		}

		public void Tick()
		{
			BTProgram previous = BTProgram.current;
			BTProgram.current = this;

			if (main != null && boundState == BoundState.Bound)
			{
				++_tickCount;
				main.Tick();
			}
			else
			{

				if (!hasThrownExceptionOnTick)
				{
					BTProgram.current = previous;
					hasThrownExceptionOnTick = true;
					throw new System.Exception("Can not tick behaviour tree program. Program is invalid.");
				}
			}
			BTProgram.current = previous;
		}

		public void Reset()
		{
			BTProgram previous = BTProgram.current;
			BTProgram.current = this;
			if (runtimeUnits != null)
			{
				foreach (var ru in runtimeUnits)
				{
					if (ru == null || ru.trees == null) continue;
					foreach (var t in ru.trees)
						if (t != null) t.Reset();
				}
			}
			BTProgram.current = previous;
		}

		public BoundState _boundState = BoundState.Unbound;
		public BoundState boundState { get{ return _boundState; } }
		
		object[] m_boundObjects;
		public object[] boundObjects{ get{ return m_boundObjects;}}

		public void Bind(BindingInfo bindingInfo)
		{
			var prevProgram = BTProgram.current;
			BTProgram.current = this;
			_boundState = BoundState.Unbound;
			hasThrownExceptionOnTick = false;
			_bindingInfo = bindingInfo;
			TaskBinder[] taskBinders = _bindingInfo.taskBinders;
			VariableBinder[] variableBinders = _bindingInfo.variableBinders;

		   m_boundObjects = taskBinders.Select( tb => tb.target).Concat( variableBinders.Select( sb => sb.target) ).ToArray();
			if (main != null)
			{
				BTRuntimeBuilder.Bind(this, taskBinders, variableBinders);
				_boundState = BoundState.Bound;
				var tasks = this.tasks;
				foreach(var t in tasks)
				{
					if( t.boundState != BoundState.Bound )
					{
						_boundState = BoundState.Failed;
						break;
					}
				}

				Reset();
			}
			else
			{
				_boundState = BoundState.Failed;
			}

			BTProgram.current = prevProgram;
		}

		public void Unbind()
		{
			if (main != null)
			{
				var tasks = this.tasks;
				foreach (var t in tasks)
					t.Unbind();
			}
			_boundState = BoundState.Unbound;
		}

		BTTask[] _tasks;
		public BTTask[] tasks
		{
			get
			{
				if (_tasks == null)
				{
					var taskList = new List<BTTask>();
					if (runtimeUnits != null)
					{
						foreach (var ru in runtimeUnits)
						{
							if (ru == null || ru.trees == null) continue;

							foreach (var tree in ru.trees)
							{
								if (tree != null)
								{
									foreach (var task in tree.tasks)
									{
										if (!taskList.Contains(task))
											taskList.Add(task);
									}
								}
							}
						}
					}
					_tasks = taskList.ToArray();
				}

				return _tasks;
			}
		}

		BTNode[] _nodes;
		public BTNode[] nodes
		{
			get
			{
				if (_nodes == null)
				{
					var nodeList = new List<BTNode>();
					if (runtimeUnits != null)
					{
						foreach (var ru in runtimeUnits)
						{
							if (ru == null || ru.trees == null) continue;

							foreach (var tree in ru.trees)
							{
								if (tree != null )
								{
									if (!nodeList.Contains(tree))
										nodeList.Add(tree);
									foreach (var node in tree.childrenRecursive)
									{
										if (!nodeList.Contains(node))
											nodeList.Add(node);
									}
								}
							}
						}
					}
					_nodes = nodeList.ToArray();
				}

				return _nodes;
			}
		}


		public void Dispose()
		{
			_root = null;
			_runtimeUnits = null;
			_codemaps = null;
			_tasks = null;
		}

		public int lastTickFrame
		{
			get
			{
				return main != null? main.lastTick: -1;
			}
		}


		#region serialization
		public BTSnapshot snapshot
		{
			get
			{
#if !PANDA_BT_FREE
				BTSnapshot _state = new BTSnapshot();
				_state.tickCount = _tickCount;
				var nodes = this.nodes;

				_state.nodeStates = new BTNodeState[nodes.Length];
				for (int i = 0; i < nodes.Length; i++)
				{
					_state.nodeStates[i] = nodes[i].state;
				}

				return _state;
#else
				return null;
#endif
			}

			set
			{
#if !PANDA_BT_FREE
				//this.Reset();
				BTSnapshot _state = value;
				_tickCount = _state.tickCount;

				var nodes = this.nodes;
				for (int i = 0; i < nodes.Length; i++)
				{
					nodes[i].state = _state.nodeStates[i];
				}
#endif
			}
		}
#endregion
	}
}
