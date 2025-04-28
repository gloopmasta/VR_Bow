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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PandaBT.Runtime;

namespace PandaBT
{
    public static class PandaBTReflectionDatabase 
    {

		public static void ClearCache()
		{
			_memberInfoFromType.Clear();
			_taskMemberInfoFromType.Clear();
			_variablMemberInfoFromType.Clear();
		}

		static Dictionary<Type, PandaBTMemberInfo[]> _memberInfoFromType = new Dictionary<Type, PandaBTMemberInfo[]>();
		static Dictionary<Type, PandaBTMemberInfo[]> _taskMemberInfoFromType = new Dictionary<Type, PandaBTMemberInfo[]>();
		static Dictionary<Type, PandaBTMemberInfo[]> _variablMemberInfoFromType = new Dictionary<Type, PandaBTMemberInfo[]>();
		public static PandaBTMemberInfo[] GetMemberInfos(Type type)
		{
			if (_memberInfoFromType.ContainsKey(type))
				return _memberInfoFromType[type];

			var baseTypes = GetBaseClasses(type);
			var pandaMemberInfos = baseTypes.SelectMany(baseType => GetMemberInfoFromType(baseType)).Distinct().ToArray();

			_memberInfoFromType[type] = pandaMemberInfos;
			_variablMemberInfoFromType[type] = pandaMemberInfos.Where( pmi => pmi.isVariable).ToArray();
			_taskMemberInfoFromType[type] = pandaMemberInfos.Where(pmi => pmi.isTask).ToArray();

			return pandaMemberInfos;
		}

		public static PandaBTMemberInfo[] GetTaskMemberInfos(Type type)
		{
			if (_taskMemberInfoFromType.ContainsKey(type) == false)
				GetMemberInfos(type);
			return _taskMemberInfoFromType[type];
		}

		public static PandaBTMemberInfo[] GetVariableMemberInfos(Type type)
		{
			if (_variablMemberInfoFromType.ContainsKey(type) == false)
				GetMemberInfos(type);
			return _variablMemberInfoFromType[type];
		}


		public static bool IsBoundable(Type type)
		{
			bool isBoundable = false;
			isBoundable = GetMemberInfos(type).Length > 0;
			return isBoundable;
		}

		public static readonly BindingFlags taskBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.GetField;
		public static readonly BindingFlags parameterBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField;
		private static IEnumerable<PandaBTMemberInfo> GetMemberInfoFromType(Type type)
		{
			var members = type.GetMembers(taskBindingFlags | parameterBindingFlags);
			var pandaMemberInfos = new List<PandaBTMemberInfo>();
			foreach (var member in members)
			{
				BTAttribute attribute = null;

				attribute = member.GetCustomAttribute(typeof(PandaTask)) as BTAttribute;
				if( attribute != null)
					pandaMemberInfos.Add( GetPandaBTMemberInfo(attribute, member) );

				attribute = member.GetCustomAttribute(typeof(PandaVariable)) as BTAttribute;
				if (attribute != null)
					pandaMemberInfos.Add(GetPandaBTMemberInfo(attribute, member));


			};
			return pandaMemberInfos;
		}

		static PandaBTMemberInfo GetPandaBTMemberInfo(BTAttribute btAttribute, MemberInfo member)
		{
			PandaTask pandaTask = btAttribute as PandaTask;
			PandaVariable pandaVariable = btAttribute as PandaVariable;

			var pMemberInfo = new PandaBTMemberInfo();
			pMemberInfo.type = member.DeclaringType;
			pMemberInfo.memberInfo = member;
			pMemberInfo.methodInfo = member as MethodInfo;
			pMemberInfo.fieldInfo = member as FieldInfo;
			pMemberInfo.propertyInfo = member as PropertyInfo;
			pMemberInfo.isTask = pandaTask != null;
			pMemberInfo.isVariable = pandaVariable != null;

			if (pMemberInfo.methodInfo != null)
				pMemberInfo.taskType = TaskType.Method;
			else if (pMemberInfo.fieldInfo != null)
				pMemberInfo.taskType = TaskType.Field;
			else if (pMemberInfo.propertyInfo != null)
				pMemberInfo.taskType = TaskType.Property;

			return pMemberInfo;
		}

		public static IEnumerable<System.Type> GetBaseClasses(System.Type type)
		{
			List<System.Type> types = new List<System.Type>();
			var current = type;
			while (current != null)
			{
				types.Add(current);
				current = current.BaseType;
			}
			return types;
		}

	}


	public enum TaskType
	{
		Method,
		Field,
		Property,
	}

	public class PandaBTMemberInfo
	{
		public Type type;
		public MemberInfo memberInfo;
		public MethodInfo methodInfo;
		public FieldInfo fieldInfo;
		public PropertyInfo propertyInfo;
		public TaskType taskType;
		public bool isVariable = false;
		public bool isTask = false;
	}
}
