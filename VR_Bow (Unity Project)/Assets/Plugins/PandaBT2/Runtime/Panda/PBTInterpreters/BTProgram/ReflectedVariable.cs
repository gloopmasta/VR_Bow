
using System;
using System.Reflection;

namespace PandaBT.Runtime
{
	public class ReflectedVariable : IVariable
	{

		public ReflectedVariable(string name, MemberInfo memberInfo, object target, IVariable parent)
		{
			_name = name;
			_memberInfo = memberInfo;
			_fieldInfo = memberInfo as FieldInfo;
			_propertyInfo = memberInfo as PropertyInfo;
			_target = target;
			_parent = parent;

			if(_fieldInfo == null && _propertyInfo == null)
				throw new Exception("Invalid memberInfo");
		}

		public string name => _name;

		public object value
		{
			get
			{
				object _value = null;
				if( _propertyInfo != null)
				{
					_value = _propertyInfo.GetValue(target);
					return _value;
				}

				if( _fieldInfo != null)
				{
					_value = _fieldInfo.GetValue(target);
					return _value;
				}

				return _value;

			}
			set
			{
				if( _propertyInfo != null)
				{
					_propertyInfo.SetValue(target, value);
					return;
				}

				if(_fieldInfo != null)
				{
					_fieldInfo.SetValue(target, value);
					return;
				}
			}
		}

		Type IVariable.valueType
		{
			get
			{
				if (_propertyInfo != null)
					return _propertyInfo.PropertyType;

				if (_fieldInfo != null)
					return _fieldInfo.FieldType;

				return null;
			}
		}

		private object target
		{
			get
			{
				
				if( _parent != null)
				{
					return _parent.value;
				}

				return _target;
			}
		}

		string _name;
		MemberInfo _memberInfo;
		FieldInfo _fieldInfo;
		PropertyInfo _propertyInfo;
		object _target;
		IVariable _parent;
	}
}
