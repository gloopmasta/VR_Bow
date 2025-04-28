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

using System.Linq;
using UnityEngine;

namespace PandaBT.Runtime
{
	public class BindingInfo
	{
		BTBinder[] _binders;
		TaskBinder[] _taskBinders;
		VariableBinder[] _variableBinders;

		public TaskBinder[] taskBinders => _taskBinders;
		public VariableBinder[] variableBinders => _variableBinders;
		public BTBinder[] binders => _binders;


		public BindingInfo(TaskBinder[] taskBinders, VariableBinder[] variableBinders)
		{
			this._taskBinders = taskBinders;
			this._variableBinders = variableBinders;
			this._binders = _taskBinders.Select( tb => tb as BTBinder).Concat( _variableBinders.Select( vb => vb as BTBinder ) ).ToArray();
		}
	}
}
