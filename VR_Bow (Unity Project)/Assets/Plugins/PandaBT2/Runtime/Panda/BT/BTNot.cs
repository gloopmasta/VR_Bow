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
	
	public class BTNot : BTNode
	{
		BTNode m_child;
		
		public BTNot()
		{
		
		}
		
		public BTNot( BTNode child )
		{
			m_child = child;
		}
		
		protected override Status DoTick ()
		{
			Status status = m_child.Tick();

            if (status != Status.Running)
                status = status == Status.Succeeded ? Status.Failed : Status.Succeeded;
			
			return status;
		}
		
		public override void AddChild (BTNode child)
		{
			if( m_child == null  )
			{
				if( child != null)
				{
					m_child = child;
					m_child.m_parent = this;
				}
			}
			else
			{
				throw new System.Exception("BT error: Inverter node can have only one child.");
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
				if( _children == null)
					_children = new BTNode[]{m_child};
				return _children;
			}
		}

        protected override void DoReset()
        {
            m_child.Reset();
        }
		
		
	}
}


