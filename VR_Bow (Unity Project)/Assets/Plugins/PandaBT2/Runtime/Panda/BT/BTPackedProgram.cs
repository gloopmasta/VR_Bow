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

#if false
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Panda.Runtime
{
	public class BTPackedProgram
	{
		public enum NodeType
		{
			Sequence,
			Fallback,
			Parallel,
			Race,
			Random,
			Repeat,
			Retry,
			While,
			Not,
			Mute,
			Tree,
			TreeReference,
			Task
		}

		public enum Status
		{
			Ready,
			Running,
			Succeeded,
			Failed,
		}

		public struct Node
		{
			internal NodeType type;
			internal object[] parameters;
			internal Status status;
			internal Status previousStatus;
			internal string debugInfo;
			internal int parent;
			internal int[] children;
			internal int tick;
			internal int currentChild;
			internal System.Action task;
		}

		void node_init( int i, NodeType type )
		{
			nodes[i].type = type;
			nodes[i].status = Status.Running;
			nodes[i].previousStatus = Status.Ready;
			nodes[i].debugInfo = "";
			nodes[i].parent = -1;
			nodes[i].children = null;
			nodes[i].tick = -1;
			nodes [i].task = null;
		}

		int tickCount = 0;
		internal Node[] nodes;
		int root;
		Stack<int> tickStack = new Stack<int> ();
		Stack<int> resetStack = new Stack<int> ();
		public void Tick()
		{
			if(!node_isTickable(root) )
				node_reset( root );

			if( node_isTickable(root) )
			{
				tickStack.Push (root);
				while (tickStack.Count > 0) 
				{
					while (resetStack.Count > 0) 
					{
						int r = resetStack.Pop();
						if( nodes[r].status != Status.Ready )
							node_reset( r );
						continue;
					}

					int i = tickStack.Peek();
					var node = nodes[i];
					if(node_isTickable(i))
					{
						node_tick( i );
						continue;
					}
					else
					{
						tickStack.Pop();
					}
				}
			}
		
		}

		bool node_isTickable(int i)
		{
			return nodes[i].tick < this.tickCount && nodes [i].status != Status.Failed && nodes [i].status != Status.Succeeded;
		}

		void node_tick( int i )
		{
			switch (nodes[i].type) 
			{
			case NodeType.Sequence: tick_sequence( i ); break;
			}
		}

		void node_tick_done(int i)
		{
			nodes[i].tick = tickCount;
		}

		void node_reset( int i )
		{
			nodes[i].status = Status.Ready;
			if (nodes [i].children != null) 
			{
				foreach (var c in nodes[i].children)
				{
					if( nodes[c].status != Status.Ready )
						resetStack.Push( c );
				}
			}

			switch (nodes[i].type) 
			{
			case NodeType.Sequence: reset_sequence( i ); break;
			}
		}

		void reset_sequence(int i)
		{
			nodes[i].currentChild = 0;
		}

		void tick_sequence(int i)
		{
			int c = nodes[i].children [nodes [i].currentChild];
			if (node_isTickable (c)) 
			{
				tickStack.Push (c);
				return;
			}

			switch (nodes[c].status) 
			{
			case Status.Running:
				nodes[i].status = Status.Running;
				node_tick_done(i);
				break;

			case Status.Succeeded:
				nodes[i].currentChild++;
				if (nodes[i].currentChild < nodes [i].children.Length)
				{
					c = nodes [i].children [nodes [i].currentChild];
					tickStack.Push (c);
				}
				else
				{
					nodes[i].status = Status.Succeeded;
				}
				break;
			
			case Status.Failed:
				nodes[i].status = Status.Failed;
				node_tick_done(i);
				break;
			}
		}
	}

}


#endif
