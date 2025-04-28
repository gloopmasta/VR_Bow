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

using System;
using System.Collections;
using System.Collections.Generic;

namespace PandaBT.Runtime
{
	
	public class BTBreak : BTNode
	{

		int m_depth = 0;

		public BTBreak()
		{
		
		}

		protected override Status DoTick ()
		{

			m_depth = parameters.Length > 0 ? System.Convert.ToInt32(parameters[0]) : 0;

			Status status = Status.Running;
			BTNode target = null;

			var depth = m_depth + 1;
			target = this;
			bool isBreakable = false;

			do
			{
				target = target.parent;
				if( target != null ) isBreakable = IsBreakable(target);
				if( isBreakable ) depth--;
			} while (target != null && (!isBreakable || isBreakable && depth > 0) );
 
			if ( target != null )
			{
				if ( UserTask._isInspected)
                {
					target.debugInfo = debugInfo = "BREAK";
                }
				target.Succeed();
				foreach( var child in target.children)
				{
					if (child.m_status == Status.Running)
						child.Abort();
				}
			}
			else
			{
				throw new Exception("Break node must have a repeat node as ancestor");
			}

			
			return status;
		}
		public override void AddChild (BTNode child)
		{
			throw new Exception("BT error: Break node can not have child.");
		}
		
		public override void Dispose ()
		{
		}

		BTNode[] _children;
		public override BTNode[] children 
		{
			get 
			{
				if( _children == null)
					_children = new BTNode[]{};
				return _children;
			}
		}

		protected override void DoReset()
		{
		}

		bool IsBreakable( BTNode node)
		{
			bool itIs = false;
			var nodeType = node.GetType();
			itIs = nodeType == typeof(BTRepeat);
			return itIs;
		}
		
		
	}
}


