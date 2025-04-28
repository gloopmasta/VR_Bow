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
using UnityEngine;

using UnityEditor;
using PandaBT;
using PandaBT.Runtime;


namespace PandaBT.BTEditor
{
	[CustomEditor(typeof(PandaBTScript))]
	public class PandaBTScriptEditor : Editor
	{

		PandaBTScript script;
		SourceDisplay sourceDisplay = null;

		private void OnEnable()
		{
			BTLSyntaxHighlight.InitializeColors();
			script = (PandaBTScript)target;
			sourceDisplay = BTLGUILine.Analyse(script.compilationUnit.tokens, 0);
		}

		public override void OnInspectorGUI()
		{
			if (sourceDisplay != null)
				sourceDisplay.DisplayCode();
			else
				GUILayout.Label(script.source);
		}
	}
}
