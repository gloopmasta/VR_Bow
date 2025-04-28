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
using System.Linq;

using UnityEngine;
using UnityEditor;

using PandaBT;


namespace PandaBT.BTEditor
{
	[CustomEditor(typeof(PandaLinker))]
	[CanEditMultipleObjects]
	public class PandaLinkerEditor : Editor
	{
		SerializedProperty bindTargets;
		SerializedProperty scope;

		PandaLinker[] _linkers => targets.Select(t => t as PandaLinker).ToArray();
		void OnEnable()
		{
			scope = serializedObject.FindProperty("LinkTo");
			bindTargets = serializedObject.FindProperty("Targets");
		}

		public override void OnInspectorGUI()
		{
			bool hasChanged = false;
			serializedObject.Update();
			EditorGUILayout.PropertyField(scope);
			EditorGUILayout.PropertyField(bindTargets);
			if (serializedObject.hasModifiedProperties)
				hasChanged = true;
			serializedObject.ApplyModifiedProperties();

			if (hasChanged)
				RebindBT();
		}

		private void RebindBT()
		{

			List<PandaBehaviour> pandaBehaviours = new List<PandaBehaviour>();

			bool rebindAll = false;
			foreach (var linker in _linkers)
			{
				if (linker != null)
				{
					if (linker.LinkTo == PandaLinkerScope.AllPandaBehavioursInScene)
					{
						rebindAll = true;
						break;
					}

					if (linker.LinkTo == PandaLinkerScope.ThisPandaBehaviour)
					{
						var bt = linker.gameObject.GetComponent<PandaBehaviour>();
						if (bt != null)
						{
							bt.Bind();
						}
						else
						{
							Debug.LogError("Panda Behaviour component expected", linker);
						}
					}

				}
			}

			if (rebindAll)
			{
				var bts = FindObjectsOfType<PandaBehaviour>();
				foreach (var bt in bts)
					bt.Bind();
			}
		}
	}
}
