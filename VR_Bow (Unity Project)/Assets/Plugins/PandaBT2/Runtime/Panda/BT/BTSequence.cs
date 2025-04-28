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
using System.Runtime.Serialization;

namespace PandaBT.Runtime
{

	public class BTSequence : BTCompositeNode
	{
		internal int _idx = 0;
        BTNode m_current;
		
		public BTSequence()
			: base()
		{
			
		}
		
	
		public BTSequence( params BTNode[] children)
			: base( children )
		{
			
		}

        protected override void DoReset()
        {
            base.DoReset();
            _idx = 0;
            m_current = children[0];
        }
		
		protected override Status DoTick ()
		{
			Status status = Status.Failed;
            do
            {

                status = m_current.Tick();

                if (status == Status.Succeeded)
                {
                    ++_idx;
                    var children = this.children;
                    m_current = _idx < children.Length ?  children[_idx] : null;
                }
				
			}while( status == Status.Succeeded && m_current != null);

			if( status == Status.Succeeded && m_current != null)
				status = Status.Running;

            return status;
		}


        internal override BTNodeState state
        {
            get
            {
                return new BTSequenceState(this);
            }

            set
            {
                var _state = value as BTSequenceState;
                base.state = _state;

                _idx = _state.idx;
                if (! (_idx < children.Length))
                    _idx = 0;

                m_current = children[_idx];

            }
        }


    }

}
