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

using System.Reflection;

namespace PandaBT.Runtime
{
	public delegate void Notify();
	public class BTTree : BTNode
	{
        
		public string name = "";
		public event Notify OnReset;
		
		public override void Dispose ()
		{
			m_child = null;
		}
		
		public BTTree()
		{
		}
		
		public BTTree( BTNode child )
		{
			m_child = child;
		}

        protected override void DoReset()
        {
            m_child.Reset();
			OnReset?.Invoke();
        }

		protected override Status DoTick ()
		{
			return m_child.Tick();
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
				}
				else
				{
					throw new System.Exception("BT error: Root node can have only one child.");
				}
			}
			_children = null;
		}


		BTNode[] _children;
		public override BTNode[] children 
		{
			get 
			{
				if(_children == null)
					_children  = new BTNode[]{m_child}; 
				return _children;
			}
		}
#endregion		
		public BTTask[] tasks
		{
			get
			{
				// Get all tasks recursively
				var stack = new Stack<BTNode> ();
				var taskList = new List<BTTask>();
				stack.Push (this);
				while (stack.Count > 0) 
				{
					var node = stack.Pop ();
					if( node is BTTask)
						taskList.Add( node as BTTask);
					
					for (int c = node.children.Length - 1; c >= 0; --c) 
					{
						var child = node.children [c];
						stack.Push (child);
					}
				}
				return taskList.ToArray();
			}
		}

		public BTTreeReference[] treeReferences
		{ 
			get
			{
				List<BTTreeReference> treeRefs = new List<BTTreeReference>();

				List<BTTree> alreadySeen = new List<BTTree>();

				var fifo = new Queue<BTTree>();
				fifo.Enqueue(this);

				while (fifo.Count > 0)
				{
					var tree = fifo.Dequeue();
					alreadySeen.Add(tree);
					BTNode[] children = tree.childrenRecursive;
					foreach (var n in children)
					{
						BTTreeReference treeRef = n as BTTreeReference;
						if (treeRef != null )
						{
							treeRefs.Add(treeRef);

							if( treeRef.target != null && !alreadySeen.Contains(treeRef.target))
							{
								fifo.Enqueue(treeRef.target);
							}
						}
					}
				}

				return treeRefs.ToArray();
			}
		}
		
		public BTNode[] childrenRecursive
		{
			get
			{
				// Get all children recursively
				var stack = new Stack<BTNode> ();
				var nodes = new List<BTNode>();
				stack.Push (this);
				while (stack.Count > 0) 
				{
					var node = stack.Pop ();
					
					if( node != this)
						nodes.Add( node as BTNode);

					if( node.children != null )
						for (int c = node.children.Length - 1; c >= 0; --c) 
						{
							var child = node.children [c];
							stack.Push (child);
						}
				}
				return nodes.ToArray();
			}
			
		}

	}
	
	
}


