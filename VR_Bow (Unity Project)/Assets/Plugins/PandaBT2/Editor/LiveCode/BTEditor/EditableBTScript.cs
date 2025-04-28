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
using System.Linq;

using UnityEngine;
using UnityEditor;

using PandaBT.Compilation;
using PandaBT.BTEditor;

namespace PandaBT.Runtime
{

	public class EditableBTScript 
	{

		BehaviourTree _behaviourTree = null;
		public BehaviourTree behaviourTree { get { return _behaviourTree; } }

		public static EditableBTScript current = null;

		public static Vector2 mouseLastPosition = Vector2.zero;
		public static Vector2 mouseDownPosition = Vector2.zero;
		public static Vector3 mouseDelta = Vector2.zero;

		static bool _isMouseHoverLineNumbers = false;
		static float _isMouseHoverLineNumbers_setTime = float.NegativeInfinity;
		public static bool isMouseHoverLineNumbers
		{
			get
			{
				return _isMouseHoverLineNumbers;
			}

			set
			{
				if (!value && Time.realtimeSinceStartup - _isMouseHoverLineNumbers_setTime < 0.1f)
					return;

				_isMouseHoverLineNumbers = value;
				_isMouseHoverLineNumbers_setTime = Time.realtimeSinceStartup;
			}
		}

		

		internal static BTLGUILine hoveredLineRun;
		internal static SourceDisplay sourceDisplay;

		internal static string[] taskList = null;
		internal static TaskBinder[] taskImplementations = null;


#region edit_breakpoints

		public static void BreakPoint_ClearAll()
		{
			BehaviourTree bt = null;
			int scriptIndex = -1;
			int lineNumber = -1;
			EditableBTScript editableScript = null;
			GetHovered(out bt, out editableScript, out scriptIndex, out lineNumber);

			if (!Application.isPlaying && !EditorApplication.isPaused)
				Undo.RecordObject(bt, "Set breakpoint");


			var sourceInfo = bt.sourceInfos[scriptIndex];
			sourceInfo.breakPoints.Clear();
			sourceInfo.breakPointStatuses.Clear();

			if (!Application.isPlaying && !EditorApplication.isPaused)
				PrefabUtility.RecordPrefabInstancePropertyModifications(bt);

			BehaviourTreeEditor.RefreshAll();
		}


		public static void BreakPoint_Set(Status status)
		{

			BehaviourTree bt = null;
			int scriptIndex = -1; 
			int lineNumber = -1;
			EditableBTScript editableScript = null;
			GetHovered(out bt, out editableScript, out scriptIndex, out lineNumber);
			if (bt == null)
				return;

			if (!Application.isPlaying && !EditorApplication.isPaused)
				Undo.RecordObject(bt, "Set breakpoint");

			var sourceInfo = bt.sourceInfos[scriptIndex];

			if (status != Status.Ready)
				sourceInfo.AddBreakPoint(lineNumber, status);
			else
				sourceInfo.RemoveBreakPoint(lineNumber);
	

			if(Application.isPlaying || EditorApplication.isPaused)
			{
				if (hoveredLineRun != null)
				{
					hoveredLineRun.breakPointStatus = status;
					hoveredLineRun.isBreakPointEnable = status != Status.Ready;
				}
			}
			
			if (!Application.isPlaying && !EditorApplication.isPaused)
				PrefabUtility.RecordPrefabInstancePropertyModifications(bt);

			BehaviourTreeEditor.RefreshAll();

		}

		private static void GetHovered( out BehaviourTree bt, out EditableBTScript editableScript,  out int scriptIndex, out int lineNumber )
		{
			if (hoveredLineRun == null)
			{
				bt = null;
				editableScript = null;
				scriptIndex = -1;
				lineNumber = -1;
				return;
			}

			editableScript = null;
			scriptIndex = -1;
			lineNumber =  hoveredLineRun.lineNumber;
			editableScript = hoveredLineRun.EditableScript;

			SourceDisplay sd = null;
			if (scriptIndex == -1 && hoveredLineRun != null)
			{
				sd = hoveredLineRun.SourceDisplay;
				scriptIndex = sd.scriptIndex;
			}

			if (scriptIndex == -1)
			{
				throw new Exception("Could not determine script index");
			}

			bt = null;
			if (editableScript != null)
				bt = editableScript.behaviourTree;

			if (bt == null && sd != null)
			{
				bt = sd.bt;
			}
		}
		#endregion


		#region open in external editor helpers
		public static string GetScriptPath(MonoBehaviour monoBehaviour)
		{
			string scriptPath = null;
#if UNITY_EDITOR
			var classname = monoBehaviour.GetType().Name;
			var filter = string.Format("t:script {0}", classname);
			foreach (var guid in UnityEditor.AssetDatabase.FindAssets(filter))
			{
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

				var end = string.Format("/{0}.cs", classname);
				if (path.EndsWith(end))
					scriptPath = path;
			}
#endif

			return scriptPath;
		}


		static readonly Dictionary<System.Type, string> typeregex = new Dictionary<System.Type, string>()
			{
				{typeof(System.Boolean), @"\b(bool|(System\.)?Boolean)\b"},
				{typeof(System.Int32)  , @"\b(int|(System\.)?Int32)\b"},
				{typeof(System.Single) , @"\b(float|(System\.)?Single)\b"},
				{typeof(System.String) , @"\b(string|(System\.)?String)\b"},
			};


		static string GetMemberRegexPattern(System.Reflection.MemberInfo member)
		{
			string pattern = "";

			var method = member as System.Reflection.MethodInfo;
			var field = member as System.Reflection.FieldInfo;
			var property = member as System.Reflection.PropertyInfo;

			if (method != null)
			{
				string regparams = @"\s*";
				var parameters = method.GetParameters();
				for (int i = 0; i < parameters.Length; ++i)
				{
					var param = parameters[i];
					if(typeregex.ContainsKey(param.ParameterType))
						regparams += typeregex[param.ParameterType];
					else
						regparams += "\\b" + param.ParameterType.Name + "\\b";

					regparams += @"[^,]*";
					if (i + 1 < parameters.Length)
					{
						regparams += @",\s*";
					}
				}

				pattern = method.Name + @"\s*\(" + regparams + @"\)";
			}

			if (field != null)
			{
				pattern = @"\b(bool|(System\.)?Boolean)\b\s+" + field.Name + @"\s*";
			}

			if (property != null)
			{
				pattern = @"\b(bool|(System\.)?Boolean)\b\s+" + property.Name + @"\s*";
			}


			return pattern;
		}

		public static void GetMemberLocation(MonoBehaviour monobeHaviour, System.Reflection.MemberInfo memberInfo, out string path, out int lineNumber)
		{
			path = null;
			lineNumber = -1;
#if UNITY_EDITOR
#if PANDA_BT_WITH_CECIL
			CecilUtils.GetMemberSourceAssetPath(memberInfo, out path, out lineNumber);
#else
			path = GetByRegext(monobeHaviour, memberInfo, ref lineNumber);
#endif
#endif
		}

#if !PANDA_BT_WITH_CECIL
		private static string GetByRegext(MonoBehaviour monobeHaviour, MemberInfo memberInfo, ref int lineNumber)
		{
			string path = GetScriptPath(monobeHaviour);
			var text = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;

			var source = text.text;

			var ptrn = GetMemberRegexPattern(memberInfo);

			var regex = new System.Text.RegularExpressions.Regex(ptrn);
			var matches = regex.Matches(source);

			if (matches.Count > 0)
			{
				var pos = matches[0].Index;
				lineNumber = 1;
				for (int i = 0; i < pos; ++i)
				{
					if (source[i] == '\n')
						++lineNumber;
				}
			}

			return path;
		}
#endif
		public static void OpenScript(MonoBehaviour monobeHaviour, System.Reflection.MemberInfo memberInfo)
		{
#if UNITY_EDITOR
			string path;
			int lineNumber;
			GetMemberLocation(monobeHaviour, memberInfo, out path, out lineNumber);

			var text = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
			UnityEditor.AssetDatabase.OpenAsset(text, lineNumber);
#endif
		}


#endregion
	}

}