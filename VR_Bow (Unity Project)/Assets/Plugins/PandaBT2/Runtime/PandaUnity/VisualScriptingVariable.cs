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

#if PANDA_BT_WITH_VISUAL_SCRIPTING
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Unity.VisualScripting;

namespace PandaBT.Compilation
{
	public class VisuaScriptingVariable : IVariable
	{
		string _name;
		Variables _variablesComponent;

		public VisuaScriptingVariable(Variables variablesComponent, string variableName)
		{
			_variablesComponent = variablesComponent;
			_name = variableName;
		}

		public string name => _name;
		public object value
		{
			get => _variablesComponent.declarations.Get<object>(_name);
			set => _variablesComponent.declarations.Set(_name, value);
		}

		Type _valueType = null;
		public Type valueType
		{
			get
			{
				if( _valueType != null)
					return _valueType;

				if (value != null)
				{
					_valueType = value.GetType();
				}
				else
				{
					var declaration = _variablesComponent.declarations.Where(d => d.name == _name).FirstOrDefault();
					if( declaration == null)
					{
						throw new Exception($"No variable named `{_name}`.");
					}

					if( declaration.typeHandle != null )
					{
						_valueType = Type.GetType(declaration.typeHandle.Identification);
					}
					else
					{
						_valueType = typeof(object);
						Debug.LogWarning($"Could not determine type of variable named `{_name}`.");
					}
				}

				return _valueType;
			}
		}

	}
}
#endif