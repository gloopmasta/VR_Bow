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

namespace PandaBT.Runtime
{
	public class BTTreeReference : BTNode
	{
		public string name = "";

		private BTTree _target;
		private bool _ignoreOnTargetReset = false;

		public BTTreeReference()
		{
		}

		public BTTree target
		{
			get
			{
				return _target;
			}

			set
			{
				if( _target != null )
					_target.OnReset -= OnTargetReset;

				_target = value;
				_target.OnReset += OnTargetReset; 
			}
		}
		
		protected override void DoReset()
		{
			m_status = Status.Ready;
			ResolveTarget();
			if( target != null)
			{
				_ignoreOnTargetReset = true;
				target.Reset();
				_ignoreOnTargetReset = false;
			}
			else
			{
				var variableName = i_parameters.Length > 0 ? i_parameters[0] as IVariable : null;
				if (variableName != null)
				{
					m_nextReturnStatus = Status.Failed;
					if( UserTask._isInspected)
						debugInfo = $"No tree named '{variableName.value}'";
				}
				else
				{
					throw new System.Exception($"Can not resolve tree name '{name}'");
				}
			}

		}

		protected override Status DoTick ()
		{
			if( target == null )
				return Status.Failed;

			if( this.m_status == Status.Ready && target.m_status == Status.Running )
			{
				if (target.lastTick == this.lastTick)
				{
					string msg = string.Format("The tree(\"{0}\") is already running. Concurrent execution of the same subtree is not supported.", this.name);
					this.Fail();
					throw new System.Exception(msg);
				}
				else
				{
					DoReset();
				}
			}
			else if( this.m_status == Status.Ready && this.m_status != target.m_status)
			{
				DoReset();
			}

			return target.Tick();
		}
		
		public override void AddChild (BTNode child)
		{
			throw new System.Exception("BT error: node parented to TreeReference.");
		}
		

		BTNode[] _children;
		public override BTNode[] children 
		{
			get 
			{
				if(_children == null)
					_children = new BTNode[]{};
				return _children;
			}
		}


		public override void Dispose ()
		{
			target.OnReset -= OnTargetReset;
			target = null;
		}

		private void OnTargetReset()
		{
			if (_ignoreOnTargetReset) return;
			m_status = Status.Ready;
		}

		
		private void ResolveTarget()
		{
			var variableName = i_parameters != null && i_parameters.Length > 0 ? i_parameters[0] as IVariable : null;
			if( variableName != null)
			{
				_target = null;
				foreach(var n in BTProgram.current.nodes)
				{
					bool isTarget = false;
					var tree = n as BTTree;
					var treeName = tree != null && tree.parameters.Length > 0 ? tree.parameters[0] as string : null;
					if (treeName != null && treeName == variableName.value as string)
					{
						isTarget = true;
					}
					
					if( isTarget)
					{
						if (UserTask._isInspected)
							debugInfo = $"\"{variableName.value}\"";
						_target = tree;
						break;
					}
				}

			}

		}
	
	}
}

