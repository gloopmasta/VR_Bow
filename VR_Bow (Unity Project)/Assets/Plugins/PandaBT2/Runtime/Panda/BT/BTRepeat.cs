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

	public class BTRepeat : BTNode
	{
		public static uint repeatOnSameTickLimit = 32000;
		bool m_repeatLimitReached = false;

		public BTRepeat()
		{
		}

		public BTRepeat( int count, BTNode child )
		{
			i_parameters = new object[] { count };
			m_limit = count;
			m_child = child;
		}
		
		internal int m_counter = 0;
		int m_limit   = 0;
		
		protected override void DoReset()
		{
			m_child.Reset();
			m_limit = parameters.Length > 0 ? System.Convert.ToInt32( parameters[0] ) : 0;
			m_counter = 0;
		}

		protected override Status DoTick ()
		{
			if (m_repeatLimitReached)
				return Status.Failed;

			uint repeatOnSameTick = 0;

			Status status = Status.Failed;
			do
			{

				if (repeatOnSameTick > repeatOnSameTickLimit)
				{
					m_repeatLimitReached = true;
					throw new System.Exception("repeat limit per tick reached");
				}

				if (this.m_status == Status.Running && m_child.m_status == Status.Succeeded && (m_counter < m_limit	|| m_limit == 0 )
					|| this.status == Status.Ready )
					m_child.Reset();

				status = m_child.Tick();

				if (status != Status.Running)
					++m_counter;

				if ((m_counter < m_limit || m_limit == 0) && status == Status.Succeeded)
					status = Status.Running;

				if (UserTask._isInspected && this.debugInfo != "BREAK")
					this.debugInfo = m_limit != 0 ? $"{m_counter}/{m_limit}" : this.debugInfo;
				
				repeatOnSameTick++;
			} while ( BTProgram.current.repeatRetryOnSameTick && m_child.status == Status.Succeeded && status == Status.Running);

			return status;
		}
		
		#region child management
		BTNode m_child;
		
		public override void AddChild (BTNode child)
		{
			if(child != null)
			{
				if( m_child == null  )
				{
					m_child = child;
					m_child.m_parent = this;
				}
				else
				{
					throw new System.Exception("BT error: Repeat node can have only one child.");
				}
			}
			_children = null;
		}
		
		public override void Dispose ()
		{
			m_child = null;
		}

		BTNode[] _children;
		public override BTNode[] children 
		{
			get 
			{
				if(_children == null)
					_children = new BTNode[]{m_child};
				return _children;
			}
		}
		#endregion

		internal override BTNodeState state
		{
			get
			{
				return new BTRepeatState(this);
			}

			set
			{
				var _state = value as BTRepeatState;
				base.state = _state;
				this.m_counter = _state.counter;
			}
		}
	}


}

