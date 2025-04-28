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

	public class BTFallback : BTCompositeNode
	{
		internal int _idx;
        BTNode m_current;
        public BTFallback()
		{
		
		}
		
		public BTFallback( params BTNode[] children)
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

			if (this.m_status == Status.Failed)
				return Status.Failed;

			do
			{
                status = m_current.Tick();
                if (status == Status.Failed)
                {
                    ++_idx;
                    var children = this.children;
                    m_current = _idx < children.Length ? children[_idx] : null;
                }
            } while(m_current != null && (status == Status.Failed  ));

			return status;
		}
		
		public override void Dispose ()
		{
			base.Dispose();
		}

        internal override BTNodeState state
        {
            get
            {
                return new BTFallbackState(this);
            }

            set
            {
                var _state = value as BTFallbackState;
                this.m_status = _state.status;
                _idx = _state.idx;
                m_current = children[_idx];
            }
        }

    }


}

