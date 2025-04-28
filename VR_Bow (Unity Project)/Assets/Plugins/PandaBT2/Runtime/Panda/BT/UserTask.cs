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
using System.Reflection;
using AsyncTask = System.Threading.Tasks.Task<bool>;

namespace PandaBT.Runtime
{

	/// <summary>
	///  
	/// Task implementation.
	/// </summary>
	///

	public class UserTask
	{ 
		/// <summary>
		/// Current status of the task.
		/// </summary>
		public Status status { get { return _task.m_status; } }

		public UserTask()
		{

		}

		/// <summary>
		/// Succeed the task.
		/// </summary>
		public void Succeed()
		{
			_task.Succeed();
		}

		/// <summary>
		/// Fail the task.
		/// </summary>
		public void Fail()
		{
			_task.Fail();
		}

		/// <summary>
		/// Complete the task. If succeed is true, the task succeeds otherwise the task fails.
		/// </summary>
		/// <param name="succeed">wether the task succeeds or fails</param>
		public void Complete( bool succeed )
		{
			if (succeed)
				Succeed();
			else
				Fail();
		}

		public Action OnAbort;

		/// <summary>
		/// The current ticked task. (Only valid within the scope of task method scope.)
		/// </summary>
		public static UserTask current
		{
			get
			{
				var task = BTProgram.current != null ? BTProgram.current.currentNode as BTTask : null;
				if (task == null)
				{
					throw new System.Exception("`Panda.Task.current` or `ThisTask` can only be accessed from a method implementing a task (having the [Task] attribute).");
				}
				return task._userTask;
			}
		}

		/// <summary>
		/// Use this to attach custom data needed for the computation of the task.
		/// </summary>
		[Obsolete("Deprecated, please use `data` instead.")]
		public object item
		{
			get
			{
				return _task.item;
			}

			set
			{
				_task.item = value;
			}
		}

		/// <summary>
		/// Use this to attach custom data needed for the computation of the task.
		/// (alias for item)
		/// </summary>
		public object data
		{
			get
			{
				return _task.item;
			}

			set
			{
				_task.item = value;
			}
		}

		/// <summary>
		/// Returned the item casted to the given type T
		/// </summary>
		/// <typeparam name="T"></typeparam>

		[Obsolete("Deprecated, please use `GetData` instead.")]
		public T GetItem<T>()
		{
			return (T)_task.item;
		}

		/// <summary>
		/// Returned the data casted to the given type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public T GetData<T>()
		{
			return (T)_task.item;
		}

		/// <summary>
		/// The text displayed next to the task in the inspector at runtime.
		/// Use to expose debugging information about the task.
		/// </summary>
		public string debugInfo
		{
			get
			{
				return _task.debugInfo;
			}

			set
			{
				_task.debugInfo = value;
			}
		}

		/// <summary>
		/// Returns true on first tick of the task. Use to initialise a task. 
		/// </summary>
		public bool isStarting
		{
			get
			{
				return BTProgram.current.currentNode == _task && _task.m_status == Status.Ready;
			}
		}

		/// <summary>
		/// Whether the current BT script is displayed in the Inspector.
		/// (Use this for GC allocation optimization)
		/// </summary>
		internal static bool _isInspected;
		public static bool isInspected
		{
			get
			{
				return _isInspected;
			}

			set
			{
				_isInspected = value;
			}
		}

		internal BTTask _task;

		internal AsyncTask _asyncTask;

	}


}
