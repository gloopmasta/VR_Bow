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
using System.Linq;


namespace PandaBT.Runtime
{

	public class BTRandom : BTProbalisticNode
	{
		protected override Status DoTick ()
		{

			Status status = Status.Failed;

			if( m_selectedChild == null || m_uncompletedChildren.Count > 0 && m_selectedChild.status != Status.Running)
			{
				ProcessWeights();
				SelectNextChildToRun();
				if( m_selectedChild != null )
					m_uncompletedChildren.Remove(m_selectedChild);
				
			}

			if ( m_selectedChild != null)
			{
				status =  m_selectedChild.Tick();
				if( status == Status.Failed && m_uncompletedChildren.Count > 0 )
					status = Status.Running;
			}

			return status;
		}

		internal override BTNodeState state
		{
			get
			{
				return new BTRandomState(this);
			}

			set
			{
				var _state = value as BTRandomState;
				base.state = _state;
				m_selectedChild = children[_state.selectedChildIndex];
			}
		}

	}

}

