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

	public class BTBefore : BTNode
	{
		
		public BTBefore()
		{
		}

		public BTBefore( BTNode condition, BTNode action)
		{
			AddChild(condition);
			AddChild(action);
		}

		protected override void DoReset()
		{
			m_condition.Reset();
			m_action.Reset();
		}

		protected override Status DoTick ()
		{
			Status status = Status.Running;
			
			m_condition.Tick();

			if (m_condition.status == Status.Succeeded || m_condition.status == Status.Failed)
			{
				status = Status.Failed;
				if( m_action.status == Status.Running )
					m_action.Abort();
			}
			
			if(m_condition.m_status == Status.Running)
			{
				m_action.Tick();
				status = m_action.status;
				if( status != Status.Running )
					m_condition.Abort();
			}

			return status;
		}

#region childmanagement		
		BTNode m_condition;
		BTNode m_action;
		public override BTNode[] children
		{
			get{
				var children = new List<BTNode>();
				if( m_condition != null)
					children.Add(m_condition);
				if( m_action != null )
					children.Add(m_action);
				return children.ToArray();
			}
		}

		public override void AddChild(BTNode child)
		{
			if (m_condition == null)
				m_condition = child;
			else if (m_action == null)
				m_action = child;
			else
				throw new Exception("Before node can not have more than two children.");

			child.m_parent = this;
		}
		
		public override void Dispose ()
		{
			m_condition = null;
			m_action = null;
		}
		
#endregion
		
	}


}
