/*
Copyright (c) 2015 Eric Begue (ericbeg@gmail.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

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

	public class BTIf : BTNode
	{
		
		public BTIf()
		{
		}

		public BTIf( BTNode condition, BTNode task)
		{
			AddChild(condition);
			AddChild(task);
		}

		public BTIf(BTNode condition, BTNode thenTask, BTNode elseTask)
		{
			AddChild(condition);
			AddChild(thenTask);
			AddChild(elseTask);
		}

		protected override void DoReset()
		{
			m_condition.Reset();
			m_thenTask.Reset();

			if( m_elseTask != null )
				m_elseTask.Reset();
		}

		protected override Status DoTick ()
		{
			Status status = Status.Failed;
			
			m_condition.Tick();
			status = m_condition.status;

			if (m_condition.status == Status.Failed)
				status = Status.Succeeded;
			
			if(m_condition.m_status == Status.Succeeded)
			{
				m_thenTask.Tick();
				status = m_thenTask.status;
			}
			else if (m_condition.m_status == Status.Failed && m_elseTask != null)
			{
				m_elseTask.Tick();
				status = m_elseTask.status;
			}

			return status;
		}

#region childmanagement		
		BTNode m_condition;
		BTNode m_thenTask;
		BTNode m_elseTask;

		public override BTNode[] children
		{
			get{
				var children = new List<BTNode>();
				if( m_condition != null)
					children.Add(m_condition);
				if( m_thenTask != null )
					children.Add(m_thenTask);
				if( m_elseTask!= null )
					children.Add(m_elseTask);
				return children.ToArray();
			}
		}

		public override void AddChild(BTNode child)
		{
			if (m_condition == null)
				m_condition = child;
			else if (m_thenTask == null)
				m_thenTask = child;
			else if (m_elseTask == null)
				m_elseTask = child;
			else
				throw new Exception("If node can not have more than two children.");

			child.m_parent = this;
		}
		
		public override void Dispose ()
		{
			m_condition = null;
			m_thenTask = null;
			m_elseTask = null;
		}
		
#endregion
		
	}


}
