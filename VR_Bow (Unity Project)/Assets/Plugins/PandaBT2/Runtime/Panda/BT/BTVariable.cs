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
using System.Reflection;

namespace PandaBT.Runtime
{

	public class BTVariable : IVariable
	{
		string m_name = null;
		System.Reflection.MemberInfo m_memberInfo;
		object m_target;

		internal Type m_type = null;

		public System.Reflection.MemberInfo memberInfo => m_memberInfo;
		public object target => m_target;

		public string name
		{
			get
			{
				if (m_name == null)
				{
					var field = m_memberInfo as FieldInfo;
					var property = m_memberInfo as PropertyInfo;
					m_name = field != null ? field.Name : (property != null ? property.Name : null);
				}

				return m_name;
			}
		}

		public Type valueType
		{
			get
			{
				if (m_type == null)
				{
					var field = m_memberInfo as FieldInfo;
					var property = m_memberInfo as PropertyInfo;
					m_type = field != null ? field.FieldType : (property != null ? property.PropertyType : null);
				}
				return m_type;
			}
		}

		public object value
		{
			get
			{
				if (m_target == null)
					return null;

				var memberInfo = m_memberInfo as FieldInfo;
				var property = m_memberInfo as PropertyInfo;

				var value = memberInfo != null ? memberInfo.GetValue(m_target) : property.GetValue(m_target);
				return value;
			}

			set
			{
				if (m_target != null)
				{
					var fieldInfo = m_memberInfo as FieldInfo;
					var property = m_memberInfo as PropertyInfo;

					if (fieldInfo != null)
						fieldInfo.SetValue(m_target, value);

					if (property != null)
						property.SetValue(m_target, value);
				}
			}
		}

		public BTVariable(System.Reflection.MemberInfo memberInfo, object target)
		{
			m_name = name;
			m_memberInfo = memberInfo;
			m_target = target;
		}
	}
}

