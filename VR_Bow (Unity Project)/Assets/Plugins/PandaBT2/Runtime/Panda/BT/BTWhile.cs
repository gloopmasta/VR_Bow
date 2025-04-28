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
using System.Collections;
using System.Collections.Generic;

namespace PandaBT.Runtime
{

	public class BTWhile : BTNode
	{
		bool conditionHasSucceeded = false;

		public BTWhile()
		{
		}

		public BTWhile( BTNode condition, BTNode task)
		{
			if (condition != null)
				AddChild(condition);
			if (task != null)
				AddChild(task);
		}


		protected override void DoReset()
		{
			m_condition.Reset();
			m_action.Reset();
			conditionHasSucceeded = false;
		}

		protected override Status DoTick ()
		{
			if(m_condition.m_status == Status.Succeeded )
				m_condition.Reset();

			Status status = m_condition.Tick();
			if(m_condition.m_status != Status.Running)
				conditionHasSucceeded = m_condition.m_status == Status.Succeeded;

			if (conditionHasSucceeded)
				status = m_action.Tick();

			if(m_condition.m_status == Status.Failed)
			{
				foreach(var child in children)
				{
					if( child.status == Status.Running )
						child.Abort();
				}
			}

			return status;
		}
		
		#region childmanagement		
		BTNode m_condition;
		BTNode m_action;

		public override void AddChild (BTNode child)
		{
			if (m_condition == null)
				m_condition = child;
			else if (m_action == null)
				m_action = child;
			else
				throw new Exception("While node can not have more than two children.");
			_children = null;

			if (m_condition != null) m_condition.m_parent = this;
			if (m_action != null) m_action.m_parent = this;
		}
		
		public override void Dispose ()
		{
			m_condition = null;
			m_action = null;
		}

		BTNode[] _children;
		public override BTNode[] children
		{
			get
			{
				if( _children == null)
					_children = new BTNode[] { m_condition, m_action };
				return _children;
			}
		}
		#endregion
	}


}
