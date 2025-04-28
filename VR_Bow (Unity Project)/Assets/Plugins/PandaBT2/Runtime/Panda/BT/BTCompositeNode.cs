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

	public abstract class BTCompositeNode : BTNode
	{
		List<BTNode> m_children = new List<BTNode>();
		
		public BTCompositeNode()
		{
		}
		

        public BTCompositeNode(params BTNode[] children)
        {
            foreach (var c in children)
                AddChild(c);
        }

		
		public override void AddChild (BTNode child)
		{
			if( child != null)
			{
				m_children.Add( child );
				child.m_parent = this;
                _children = null;
            }
		}
		
		public override void Dispose ()
		{
			m_children.Clear();
		}

        BTNode[] _children = null;
        public override BTNode[] children 
		{
			get 
			{
                if (_children == null)
                    _children = m_children.ToArray();
                return _children;
			}
		}

        protected override void DoReset()
        {
            var children = this.children;
            foreach (var c in children)
                c.Reset();
        }
		
	}
	
}

