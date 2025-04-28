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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


namespace PandaBT.Compilation
{

	public static class VisualScriptingVariableFinder 
	{
		public static IVariable[] FindVisualScriptingVariables(object target)
		{
			IVariable[] iVariables = null;
			var variablesComponent = target as Variables;
			if (variablesComponent != null)
			{
				iVariables = variablesComponent.declarations
					.Select(declaration => new VisuaScriptingVariable(variablesComponent, declaration.name)).ToArray();
			}
			return iVariables;
		}
	}
}
#endif
