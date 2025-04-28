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

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using InspectorGuiData = PandaBT.Runtime.BehaviourTree.InspectorGuiData;

namespace PandaBT.Runtime
{

	[CustomEditor(typeof(BehaviourTree))]
	public class BehaviourTreeEditor : Editor
	{
		public static BehaviourTreeEditor active => _active;
		private static BehaviourTreeEditor _active;
		int size = 1;
		bool btEnabled = false;
		BehaviourTree bt = null;
		SourceDisplay[] sourceDisplays;

		EditableBTScript[] editableBTScripts;
		
		static List<BehaviourTreeEditor> instances = new List<BehaviourTreeEditor>();

		private bool _requiresRefresh = true;

		public void OnEnable()
		{
			_active = this;
			Undo.undoRedoPerformed += UndoRedoPerformed;
			SourceDisplay.refreshAllBehaviourTreeEditors = RefreshAll;
			if (!instances.Contains(this))
				instances.Add(this);

			BTLSyntaxHighlight.InitializeColors();
			bt = (BehaviourTree)target;

			if (bt != null && bt.scripts != null)
			{
				size = bt.scripts.Length;
			}
			else if (bt.scripts == null &&  bt.program == null)
			{
				bt.scripts = new PandaBTScript[size];
			}

			if ( bt != null )
			{
				bt._isInspected = true;
			}

			InitSourceInfos();

			UserTask.isInspected = bt._isInspected;
			bt.OnTicked += this.Repaint;

		}

		void InitSourceInfos()
		{

			var oldSourInfos = bt.sourceInfos;
			if (bt.sourceInfos == null || bt.sourceInfos.Length != size)
			{
				bt.sourceInfos = new InspectorGuiData[size];
				for (int i = 0; i < size; ++i)
				{
					if( oldSourInfos != null && i < oldSourInfos.Length && oldSourInfos[i] != null)
						bt.sourceInfos[i] = oldSourInfos[i];
					else
						bt.sourceInfos[i] = new InspectorGuiData();
				}
			}

			if( bt.program != null)
			{
				sourceDisplays = SourceDisplay.MapGUILines(bt.bindScripts, bt.program, bt.pandaExceptions);
			}
			else
			{
				sourceDisplays = bt.scripts.Select( script => {
					SourceDisplay sd = null;
					if( script != null )
						sd  = BTLGUILine.Analyse(script.compilationUnit.tokens, 0);
					return sd;
				}).ToArray();
			}

			if (sourceDisplays != null)
			{
				foreach (var sd in sourceDisplays)
				{
					if (sd != null)
						sd.bt = bt;
				}
			}

			if( bt != null && bt.scripts != null)
			{

				for (int i = 0; i < bt.scripts.Length; i++)
				{
					// Read line collapsed state from sourceInfos
					if (sourceDisplays != null && i < sourceDisplays.Length && sourceDisplays[i] != null)
					{
						var lines = sourceDisplays[i].flattenLines;
						var list = bt.sourceInfos[i].collapsedLines;
						foreach (var line in lines)
							line.isCollapsed = list.Contains(line.lineNumber);
					}

				}

				InitBreakPoints();
				bt.Apply();
			}


		}

		private void InitBreakPoints()
		{
			if (sourceDisplays == null)
				return;

			for (int i = 0; i < sourceDisplays.Length; ++i)
			{
				var sourceDisplay = sourceDisplays[i];
				if (sourceDisplay == null)
					continue;

				var lines = sourceDisplay.flattenLines;
				foreach (var line in lines)
				{
					line.isBreakPointEnable = false;
					line.breakPointStatus = Status.Ready;
				}

				if (! (i < bt.sourceInfos.Length))
					continue;

				var breakPoints = bt.sourceInfos[i].breakPoints;
				var breakPointStatuses = bt.sourceInfos[i].breakPointStatuses;
				for (int b = 0; b < breakPoints.Count; ++b)
				{
					var l = breakPoints[b] - 1;
					if( 0 <= l && l < lines.Length)
					{
						lines[l].isBreakPointEnable = true;
						lines[l].breakPointStatus = breakPointStatuses[b];
					}
				}
			}
		}

		public override void OnInspectorGUI()
		{
			if (bt == null)
				return;


			if( bt.isActiveAndEnabled != btEnabled)
			{
				Refresh();
				btEnabled = bt.isActiveAndEnabled;
			}

			if(_requiresRefresh)
			{
				if( bt.bindScripts?.Length > 0)
				{
					if( bt.program == null && bt.bindScripts?.Length > 0 && bt.isActiveAndEnabled )
					{
						bt.Bind();
					}
					DoRefresh();
					_requiresRefresh = false;
				}
			}

			if (CheckComponentChanges() == true)
			{
				return;
			}

			var oldParams = getInspectorParams();
			var newParams = getInspectorParams();

			BTLSyntaxHighlight.InitializeStyles();

			EditorGUILayout.BeginVertical();
			
			// ERROR MESSAGE
			if ( bt.exceptions != null && bt.exceptions.Length > 0 && Application.isPlaying)
			{
				GUILayout.BeginHorizontal();
				var style = BTLSyntaxHighlight.style_failed;
				GUILayout.Label("This program contains errors. Please check console for details.", style);
				GUILayout.EndHorizontal();
			}

			// TICK ON
			EditorGUILayout.BeginHorizontal();
			newParams.tickOn = (BehaviourTree.UpdateOrder)EditorGUILayout.EnumPopup("Tick On:", oldParams.tickOn);
			EditorGUILayout.EndHorizontal();
			if (oldParams.tickOn == BehaviourTree.UpdateOrder.Custom)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Tick on every (seconds):");
				newParams.customTickInterval = EditorGUILayout.FloatField(oldParams.customTickInterval);
				EditorGUILayout.EndHorizontal();
			}

			// REPEAT ROOT
			newParams.reapeatRoot = EditorGUILayout.Toggle("Repeat root", oldParams.reapeatRoot);

			// REPEAT/RETRY ON SAME TICK
			newParams.repeatRetryOnSameTick = EditorGUILayout.Toggle("Repeat/Retry on same tick", oldParams.repeatRetryOnSameTick);

			// STATUS
			DisplayStatus();

			// COUNT
			newParams.scriptCount = EditorGUILayout.IntField("Size", oldParams.scriptCount);

			if (bt.scripts != null && bt.scripts.Length != oldParams.scriptCount )
				newParams.scriptCount = bt.scripts.Length;

			EditorGUILayout.EndVertical();

			// SCRIPT LIST
			if ( bt.scripts != null || bt.bindScripts.Length > 0 )
			{
				for (int i = 0; i < bt.scripts.Length; ++i)
				{

					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(1.0f));


					if( bt.sourceInfos != null && i < bt.sourceInfos.Length )
					{
						var isFoldout = EditorGUILayout.Foldout(bt.sourceInfos[i].isFoldout, "");

						if (isFoldout != bt.sourceInfos[i].isFoldout)
						{
							RecordUndo();
							bt.sourceInfos[i].isFoldout = isFoldout;
						}
					}

					EditorGUILayout.EndHorizontal();

					string label = null;

					label = string.Format("BT Script {0}", i);

					GUILayout.Label(label, GUILayout.MaxWidth(100.0f));

					if (bt.scripts != null)
					{
						if( newParams.scripts != null && i < newParams.scripts.Length)
							newParams.scripts[i] = EditorGUILayout.ObjectField(bt.scripts[i], typeof(PandaBTScript), allowSceneObjects: false) as PandaBTScript;
					}else
					{
						GUILayout.Label("[compiled from string]");
					}

					EditorGUILayout.EndHorizontal();

					DisplayBTScript(i);
				}
			} // end SCRIPT LIST

			bool hasChanged = !BTInspectorParams.equals(oldParams, newParams);
			if (hasChanged)
			{
				applyInspectorParams(newParams);
			}

		}


		private void DisplayStatus()
		{
			var style = BTLSyntaxHighlight.style_label;
			if (bt.program != null)
			{
				string strStatus = null;
				var statusStyle = BTLSyntaxHighlight.style_comment;
				bool hasErrors = bt.program._boundState != BoundState.Bound;
				if (hasErrors)
				{
					statusStyle = BTLSyntaxHighlight.style_failed;
					strStatus = "Status: Error";
				}
				else
				{
					strStatus = bt.status.ToString();
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"Status: {strStatus}", statusStyle);

				if(hasErrors)
				{
					if (GUILayout.Button("Print errors"))
					{
						bt.DebugLogErrors();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.Button(string.Format("Status: {0}", "N/A"), style);
			}
		}


		private void DisplayBTScript(int i)
		{
			bool isSourceAvailable = false;
			if (bt.scripts != null && i < bt.scripts.Length && bt.scripts[i] != null)
				isSourceAvailable = true;

			if (isSourceAvailable && bt.sourceInfos[i].isFoldout)
			{
				SourceDisplay sourceDisplay = null;
				if (sourceDisplays != null && i < sourceDisplays.Length)
					sourceDisplay = sourceDisplays[i];

				if (sourceDisplay != null)
				{
					sourceDisplay.DisplayCode();
				}else
				{
					GUILayout.Label("Error parsing script. See console for details.", BTLSyntaxHighlight.style_failed);
				}
			}
			else if (bt.scripts != null && bt.scripts[i] != null && !bt.sourceInfos[i].isFoldout)
			{
				if (bt.sourceInfos[i].breakPoints.Count > 0)
				{
					bt.sourceInfos[i].breakPoints.Clear();
					InitBreakPoints();
				}
			}
		}

		private void RecordUndo()
		{
			Undo.RecordObject(bt, "Undo Inspector");
		}

		private void UndoRedoPerformed()
		{
			if (bt != null)
				bt.Bind();
		}

		public override bool RequiresConstantRepaint()
		{
			return true;
		}


		public void Refresh()
		{
			_requiresRefresh = true;
		}



		public static void RefreshAll()
		{
			var pandaBehaviours = FindObjectsOfType<PandaBehaviour>();
			foreach(var bt in pandaBehaviours)
			{
				bt.Apply();
			}

			foreach (var ed in instances)
			{
				ed.Refresh();
			}
		}

		static Dictionary<Behaviour, int> _componentCount = new Dictionary<Behaviour, int>();
		bool CheckComponentChanges()
		{
			var bt = target as BehaviourTree;
			bool hasChanged = false;

			if(bt != null )
			{

				int n = bt.gameObject.GetComponents<Component>().Length;
				int prev_n = n;

				if (_componentCount.ContainsKey(bt))
					prev_n = _componentCount[bt];

				if ( n != prev_n )
				{
					bt.Apply();
					bt.Bind();
					Clear_guiBTScripts();
					InitSourceInfos();
				}

				_componentCount[bt] = n;
			}
			return hasChanged;
		}


		void Clear_guiBTScripts()
		{
			if (editableBTScripts != null)
			{
				editableBTScripts = null;
			}
		}

		void OnDestroy()
		{
			Clear_guiBTScripts();

			if ( sourceDisplays != null)
			{
				foreach(var sd in sourceDisplays)
				{
					if (sd != null)
						sd.Dispose();
				}
			}
			sourceDisplays = null;

			while (instances.Contains(this))
				instances.Remove(this);

			while (instances.Contains(null))
				instances.Remove(null);

			if (instances.Count == 0)
				SourceDisplay.refreshAllBehaviourTreeEditors = null;

			if (bt != null)
			{
				bt._isInspected = false;
				bt.OnInitialized -= OnEnable;
				bt.OnTicked -= this.Repaint;
			}
		}


		private BTInspectorParams getInspectorParams()
		{
			BTInspectorParams p = new BTInspectorParams();
			p.tickOn = bt.tickOn;
			p.customTickInterval = bt.customTickOnInterval;
			p.reapeatRoot = bt.autoReset;
			p.repeatRetryOnSameTick = bt.repeatRetryOnSameTick;
			p.scriptCount = bt.scripts != null ? bt.scripts.Length : 0;
			p.scripts = (PandaBTScript[])bt.scripts.Clone();
			return p;
		}

		private void applyInspectorParams( BTInspectorParams p)
		{
			var o = getInspectorParams();
			bool isSame = BTInspectorParams.equals( o, p);

			if (isSame)
				return;

			RecordUndo();

			bt.tickOn = p.tickOn;
			bt.customTickOnInterval = p.customTickInterval;
			bt.autoReset = p.reapeatRoot;
			bt.repeatRetryOnSameTick = p.repeatRetryOnSameTick;
			size = p.scriptCount;

			if( p.scriptCount != o.scriptCount)
			{
				// Resize the TextAsset array
				var old = new PandaBTScript[bt.scripts.Length];
				p.scripts.CopyTo(old, 0);
				p.scripts = new PandaBTScript[size];

				for (int i = 0; i < size; ++i)
					p.scripts[i] = i < old.Length ? old[i] : null;
			}

			if ( !BTInspectorParams.sameScripts(p, o))
			{
				bt.scripts = (PandaBTScript[])p.scripts.Clone();
				bt.Apply();
				bt.Bind();

				Clear_guiBTScripts();

				InitSourceInfos();
				Refresh();
			}

			PrefabUtility.RecordPrefabInstancePropertyModifications(bt);

		}

		private void DoRefresh()
		{
			Clear_guiBTScripts();
			InitSourceInfos();
			if (bt)
			{
				EditorUtility.SetDirty(bt);
			}
			Repaint();
		}
	}

}