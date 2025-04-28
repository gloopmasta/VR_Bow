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

using PandaBT.Compilation;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PandaBT.Runtime
{
	public class BTTask : BTNode
	{

		public string name
		{
			get
			{
				return taskName;
			}
		}

		public object item;
		internal System.Action m_taskAction;
		internal object m_boundObject => m_taskBinder?.target;
		internal MemberInfo m_boundMember => m_taskBinder?.memberInfo;
		internal TaskBinder m_taskBinder;
		internal string taskName;
		internal UserTask _userTask;

		internal BoundState m_boundState = BoundState.Unbound;

		public BoundState boundState
		{
			get { return m_boundState; }
		}

		public void Unbind()
		{
			m_taskAction = null;
			m_taskBinder = null;
			this.m_boundState = BoundState.Unbound;
		}


		public MemberInfo boundMember
		{
			get
			{
				return m_boundMember;
			}
		}

		public object boundObject
		{
			get
			{
				return m_boundObject;
			}
		}
		
		public BTTask()
		{
			_userTask = new UserTask();
			_userTask._task = this;
			this.OnAbort += () => { _userTask.OnAbort?.Invoke();};
		}
		
		
		protected override Status DoTick ()
		{
			Status status = Status.Failed;
		
			if (m_taskAction != null)
			{
				m_taskAction();
				status = this.m_status;
			}
			
			return status;
		}
		
		public override void AddChild (BTNode child)
		{
			throw new PandaScriptException("BT error: Unexpected node parented to Task node. Check the indentation.", null, -1);
		}
		
		public override void Dispose ()
		{
			m_taskAction = null;
			_userTask = null;
		}


		BTNode[] _children = new BTNode[]{};
		public override BTNode[] children 
		{
			get 
			{
				return _children;
			}
		}

		protected override void DoReset()
		{
			// nothing to do.
		}
		


		internal override BTNodeState state
		{
			get
			{
				return new BTTaskState(this);
			}

			set
			{
				var _state = value as BTTaskState;
				base.state = _state;
				this.item = _state.item;
				this.debugInfo = _state.debugInfo;
			}
		}
	}

}

