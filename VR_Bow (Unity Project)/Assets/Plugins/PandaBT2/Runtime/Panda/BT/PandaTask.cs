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

using PandaBT.Runtime;
using System;

namespace PandaBT
{
    /// <summary>
    ///  
    /// Task implementation.
    /// 
    /// This class gives you access to a Task at runtime. When a task is ticked, the method implementing
    /// the task is called and Task.current give access to the corresponing task.
    /// 
    /// </summary>
    ///
    [AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false)  // multiuse attribute
    ]
    public class PandaTask : System.Attribute, BTAttribute
    {
        /// <summary>
        /// The current ticked task. (Only valid within the scope of task method.)
        /// </summary>
        public static UserTask self => UserTask.current;

        /// <summary>
        /// Returns true on first tick of the task. Use to initialise a task. 
        /// </summary>
        public static bool isStarting => UserTask.current.isStarting;

        /// <summary>
        /// Succeed the task.
        /// </summary>
        public static void Succeed() => UserTask.current.Succeed();

        /// <summary>
        /// Fail the task.
        /// </summary>
        public static void Fail() => UserTask.current.Fail();

        /// <summary>
        /// Complete the task. If succeed is true, the task succeeds otherwise the task fails.
        /// </summary>
        /// <param name="succeed">wether the task succeeds or fails</param>
        public static void Complete(bool succeed) => UserTask.current.Complete(succeed);

        /// <summary>
        /// Use this to attach custom data needed for the computation of the task.
        /// </summary>
        public static object data
        {
            get
            {
                return UserTask.current.data;
            }

            set
            {
               UserTask.current.data = value;
            }
        }

        /// <summary>
        /// Returned the item casted to the given type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T GetData<T>()
        {
            return UserTask.current.GetData<T>();
        }

        /// <summary>
        /// The text displayed next to the task in the inspector at runtime.
        /// Use to expose debugging information about the task.
        /// </summary>
        public static string debugInfo
        {
            get
            {
                return UserTask.current.debugInfo;
            }

            set
            {
               UserTask.current.debugInfo = value;
            }
        }

        /// <summary>
        /// Whether the current BT script is displayed in the Inspector.
        /// (Use this for GC allocation optimization)
        /// </summary>
        public static bool isInspected => UserTask.isInspected;

        /// <summary>
        /// Current status of the task.
        /// </summary>
        public static Status status => UserTask.current.status;

        public System.Reflection.MemberInfo m_memberInfo { get; set; }
        public object m_target { get; set; }

    }


}
