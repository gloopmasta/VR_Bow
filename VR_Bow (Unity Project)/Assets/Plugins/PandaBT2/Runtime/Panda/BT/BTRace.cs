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

	public class BTRace : BTCompositeNode
	{
		public BTRace()
		{
		
		}
		
		public BTRace( params BTNode[] children)
		: base( (BTNode[]) children )
		{
			
		}

		protected override Status DoTick ()
		{
			Status status = Status.Failed;

            var children = this.children;
			// Check children status
			bool AllFailed = true;
			foreach (var c in children)
			{
				c.Tick();

				if (c.m_status == Status.Succeeded)
				{
					status = Status.Succeeded;
					AllFailed = false;
					foreach (var child in children)
					{
						if (child.status == Status.Running)
							child.Abort();
					}
					break;
				}

				if (c.m_status == Status.Running)
				{
					status = Status.Running;
					AllFailed = false;
				}
			}


			if (AllFailed)
				status = Status.Failed;

			return status;
		}
		
		public override void Dispose ()
		{
			base.Dispose();
		}
	}


}
