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
using System;

using PandaBT.Compilation;
namespace PandaBT.Runtime
{


	public class SourceDisplay : System.IDisposable
	{
		public GUISkin skin;
		public BTLGUILine[] lines;
		public BTLGUILine[] orphans;
		public BehaviourTree bt;
		public int scriptIndex;
		public static System.Action refreshAllBehaviourTreeEditors = null;

		public static void RefreshAllBehaviourTreeEditors()
		{
			if (SourceDisplay.refreshAllBehaviourTreeEditors != null)
				SourceDisplay.refreshAllBehaviourTreeEditors();
		}

		public SourceDisplay(int scriptIndex)
		{
			this.scriptIndex = scriptIndex;
			InitRendering();
		}

		public void Dispose()
		{
			var lines = this.flattenLines;

			foreach(var l in lines)
				l.Dispose();

			orphans = null; lines = null;

		}



		public static SourceDisplay current;
		public static int currentSourceIndex;


		public void SetIsFoldoutAll(bool isCollapsed)
		{
			 SetIsFoldoutAll(this.lines, isCollapsed);
			 SetIsFoldoutAll(this.orphans, isCollapsed);

			if( !isCollapsed)
				bt.sourceInfos[currentSourceIndex].collapsedLines.Clear();
		}

		void SetIsFoldoutAll(BTLGUILine[] lines, bool isCollapse)
		{
			foreach (var line in flattenLines)
				CollapseLine(line, isCollapse);
		}

		void CollapseLine(BTLGUILine line, bool isCollapse)
		{
			line.isCollapsed = isCollapse;
			var collapsedLines = bt.sourceInfos[currentSourceIndex].collapsedLines;
			if ( isCollapse )
			{
				if( !collapsedLines.Contains(line.lineNumber) )
					collapsedLines.Add(line.lineNumber);
			}
			else
			{
				if (collapsedLines.Contains(line.lineNumber))
					collapsedLines.Remove(line.lineNumber);
			}
		}

		public BTLGUILine[] flattenLines
		{
			get
			{
				var lineList = new List<BTLGUILine>();

				var stack = new Stack<BTLGUILine>();

				for (int i = lines.Length - 1; i >= 0; i--)
					stack.Push(lines[i]);

				while (stack.Count > 0)
				{
					var line = stack.Pop();
					lineList.Add(line);
					for (int i = line.children.Count - 1; i >= 0; i--)
					{
						stack.Push(line.children[i]);
					}
				}

				if( orphans != null)
				{
					lineList.AddRange(orphans);
				}

				lineList.Sort((a, b) => a.lineNumber.CompareTo(b.lineNumber));

				return lineList.ToArray();
			}
		}

		#region code rendering
		BTTask clickedTask;
		float clickedTaskFade;

		void InitRendering()
		{
			clickedTask = null;
			clickedTaskFade = Time.realtimeSinceStartup - 1.0f;
		}

		public void DisplayCode()
		{
#if UNITY_EDITOR
			isPaused = UnityEditor.EditorApplication.isPaused;
			isPlaying = UnityEditor.EditorApplication.isPlaying;
#else
			isPlaying = true;
#endif

			var e = Event.current;

			if (e.isMouse && e.type == EventType.MouseDown)
			{
				EditableBTScript.mouseDownPosition = e.mousePosition;
			}

			if (e.isMouse)
			{
				EditableBTScript.mouseDelta = e.mousePosition - EditableBTScript.mouseLastPosition;
				EditableBTScript.mouseLastPosition = e.mousePosition;
			}


			SourceDisplay sourceDisplay = this;

			if (sourceDisplay != null)
			{
				SourceDisplay.current = sourceDisplay;
				SourceDisplay.currentSourceIndex = this.scriptIndex;
				if( bt != null)
				{
					switch (GUILayout.Toolbar(-1, new string[] { "-", "+" }, GUILayout.ExpandWidth(false)))
					{
						case 0: sourceDisplay.SetIsFoldoutAll(true); break;
						case 1: sourceDisplay.SetIsFoldoutAll(false); break;
					}
				}
				RenderLines(sourceDisplay);
			}

		}

		private void RenderLines(SourceDisplay sourceDisplay )
		{
			var stack = new Stack<BTLGUILine>();
			var lines = sourceDisplay.lines;
			var orphans = sourceDisplay.orphans;
			for (int i = lines.Length - 1; i >= 0; i--)
				stack.Push(lines[i]);

			int orphanIdx = 0;
			int lastRenderedLine = 0;


			GUILayout.BeginVertical();
			while (stack.Count > 0)
			{
				var line = stack.Pop();
				while (orphanIdx < orphans.Length
					 && lastRenderedLine < orphans[orphanIdx].lineNumber && orphans[orphanIdx].lineNumber < line.lineNumber)
				{
					RenderLine(orphans[orphanIdx]);
					if (!orphans[orphanIdx].isCollapsed)
						foreach (var c in orphans[orphanIdx].children)
							RenderLine(c);

					++orphanIdx;
				}

				RenderLine(line);
				lastRenderedLine = line.lineNumber;

				if (!line.isCollapsed)
				{
					for (int i = line.children.Count - 1; i >= 0; i--)
						stack.Push(line.children[i]);
				}
				else
				{
					while (orphanIdx < orphans.Length
							&& lastRenderedLine < orphans[orphanIdx].lineNumber && orphans[orphanIdx].lineNumber < line.lineNumberEnd)
					{
						++orphanIdx;
					}
				}
			}
			GUILayout.EndVertical();
		}

		void RenderLine(BTLGUILine line)
		{

			GUILayout.BeginHorizontal();
			GUI_lineNumber(line);
			GUI_indentation(line);
			bool isValidProgram = bt != null && bt.program != null && bt.program.codemaps != null && bt.program.exceptions.Length == 0;
			if ( isValidProgram && SourceDisplay.isPlaying && bt.isActiveAndEnabled)
				GUI_tokens_live(line);
			else
				GUI_tokens(line);

			GUILayout.EndHorizontal();
		}

		private void GUI_tokens_live(BTLGUILine line)
		{
			GUIStyle style = BTLSyntaxHighlight.style_label;
			for (int i = 0; i < line.tokens.Count; ++i)
			{
				BTNode btNode = null;
				
				if (i < line.btnodes.Count)
					btNode = line.btnodes[i];
				
				var token = line.tokens[i];
				style = BTLSyntaxHighlight.GetTokenStyle(token);

				if (btNode != null )
				{
					var nodeStyle = GetNodeStyle(btNode);
					if (nodeStyle != null)
						style = nodeStyle;
				}

				if (bt.exceptions.Length > 0)
					style = BTLSyntaxHighlight.style_comment;

				if (line.hasErrors)
					style = BTLSyntaxHighlight.style_failed;

				GUI_token(style, btNode, token);

				// debug info
				if (btNode !=null && !string.IsNullOrEmpty(btNode.debugInfo)  && isLastToken(line, i) )
				{
					GUILayout.Label(string.Format("[{0}]", btNode.debugInfo.Replace("\t", "   ")), BTLSyntaxHighlight.style_comment);
				}
			}// for tokens
		}

		private bool isLastToken(BTLGUILine line, int i)
		{
			bool itIs = false;
			
			if( i + 1 < line.btnodes.Count)
			{
				itIs = line.btnodes[i] != line.btnodes[i + 1];
			}
			else
			{
				itIs = true;
			}

			return itIs;
		}


		private void GUI_tokens(BTLGUILine line)
		{
			GUIStyle style = BTLSyntaxHighlight.style_label;
			for (int i = 0; i < line.tokens.Count; ++i)
			{
				BTNode node = null;
				if (i < line.btnodes.Count)
					node = line.btnodes[i];

				var token = line.tokens[i];
				style = BTLSyntaxHighlight.GetTokenStyle(token);

				if( bt != null)
				{
					if (bt.program == null || bt.program.codemaps == null)
						style = BTLSyntaxHighlight.style_comment;

					if (bt.exceptions.Length > 0)
						style = BTLSyntaxHighlight.style_comment;
				}

				if (line.hasErrors)
					style = BTLSyntaxHighlight.style_failed;

				GUI_token(style, node, token);
			}// for tokens
		}

		private void GUI_token(GUIStyle style, BTNode node, Token token)
		{
			var label = token.substring.Replace("\t", "   ");
			if (clickedTask != null && clickedTaskFade < Time.realtimeSinceStartup)
				clickedTask = null;


#if UNITY_EDITOR
			var task = node as BTTask;

			if (task != null && task.boundState != BoundState.Bound)
				style = BTLSyntaxHighlight.style_failed;

			if (task != null && task.boundState == BoundState.Bound && token.type ==TokenType.Word)
			{
				if (GUILayout.Button(label, style) && Event.current.button == 0)
				{
					if (clickedTask == task)
					{
					   EditableBTScript.OpenScript(task.boundObject as MonoBehaviour, task.boundMember);
					}

					clickedTask = task;
					clickedTaskFade = Time.realtimeSinceStartup + 0.5f;
				}
			}
			else
			{
				GUILayout.Label(label, style);
			}
#else
					GUILayout.Label(label, style);
#endif
		}

		private GUIStyle GetNodeStyle(BTNode node)
		{
			GUIStyle style = null;
			if (node != null && bt.program != null && SourceDisplay.isPlaying )
				style = BTLSyntaxHighlight.statusStyle[node.status];
			return style;
		}

		private static void GUI_indentation(BTLGUILine line)
		{
			string strIndent = "";
			for (int i = 0; i < line.indent + (line.isFoldable ? 0 : 1); i++)
			{
				strIndent += "    ";
			}
			{
				GUILayout.Label(strIndent, BTLSyntaxHighlight.style_label);
			}

			if (line.isFoldable)
			{
				if (SourceDisplay.current.bt != null)
				{
					if (GUILayout.Button("    ", line.isCollapsed ? BTLSyntaxHighlight.style_INFoldout : BTLSyntaxHighlight.style_INFoldin))
					{
						line.isCollapsed = !line.isCollapsed;
					}
				}
				else
				{
					GUILayout.Label("    ", BTLSyntaxHighlight.style_label);
				}
			}
		}

		private void GUI_lineNumber(BTLGUILine line)
		{

			// Determine whether the lines contains a node that has been ticked on this frame.
			bool hasBeenTickedOnThisFrame = HasBeenTickedOnThisFrame(line);
			bool containsLeafNode = hasBeenTickedOnThisFrame ? ContainsLeafNodes(line) : false;

			var lineNumberStyle = BTLSyntaxHighlight.style_lineNumber;

			bool isActive = hasBeenTickedOnThisFrame && (containsLeafNode || line.isCollapsed) && !line.isBreakPointEnable;


			if (isActive)
				lineNumberStyle = BTLSyntaxHighlight.style_lineNumber_active;

			if (line.isBreakPointEnable)
			{
				if (hasBeenTickedOnThisFrame && isPaused && line.isBreakPointActive)
				{
					lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_active;
				}
				else
				{
					 switch( line.breakPointStatus )
					{
						case Status.Running:
							lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_running;
							break;
						case Status.Succeeded:
							lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_succeeded;
							break;
						case Status.Failed:
							lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_failed;
							break;
						case Status.Aborted:
							lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_aborted;
							break;
						default:
							lineNumberStyle = BTLSyntaxHighlight.style_breakpoint_set_running;
							break;
					}
				}
			}
			if (line.hasErrors)
				lineNumberStyle = BTLSyntaxHighlight.style_lineNumber_error;

#if UNITY_EDITOR 
			#region break points
			if (line.btnodes != null && line.btnodes.Count > 0)
			{
				if (GUILayout.Button(line.lineNumberText, lineNumberStyle) )
				{
					if (Event.current.button == 0)
						line.ToggleBreakPoint();
					else if (Event.current.button == 1)
						line.ClearBreakPoint();

					var sourceInfo = bt.sourceInfos[scriptIndex];

					sourceInfo.RemoveBreakPoint(line.lineNumber);

					if (line.isBreakPointEnable)
					{
						sourceInfo.AddBreakPoint(line.lineNumber, line.breakPointStatus);
					}
				}
			}
			else
			{
				GUILayout.Label( line.lineNumberText, lineNumberStyle);
			}

			var prect = GUILayoutUtility.GetLastRect();
			var mpos = Event.current.mousePosition;

			if (prect.Contains(mpos))
			{
				EditableBTScript.hoveredLineRun = line;
				EditableBTScript.isMouseHoverLineNumbers = true;
				EditableBTScript.sourceDisplay = SourceDisplay.current;
			}

			// Popup contextual menu on right click.
			if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 1)
			{
				if (EditableBTScript.isMouseHoverLineNumbers && line.btnodes.Count > 0 )
				{
					UnityEditor.EditorUtility.DisplayPopupMenu(new Rect(EditableBTScript.mouseLastPosition.x, EditableBTScript.mouseLastPosition.y, 0, 0), "Window/Panda BT 2 Editor/Break Point", null);
				}
			}

			#endregion

#else
			GUILayout.Label(line.lineNumberText, lineNumberStyle);
#endif
		}

		private static bool ContainsLeafNodes(BTLGUILine line)
		{
			bool containsLeafNode = false;
			if (line.btnodes != null)
			{
				foreach (var n in line.btnodes)
				{
					if (n == null)
						continue;
					if (n.GetType() == typeof(BTTask) || n.GetType() == typeof(BTTreeReference))
					{
						containsLeafNode = true;
						break;
					}
				}
			}

			return containsLeafNode;
		}

		private bool HasBeenTickedOnThisFrame(BTLGUILine line)
		{
			bool hasBeenTickedOnThisFrame = false;
			if (line.btnodes != null)
			{
				foreach (var n in line.btnodes)
				{
					hasBeenTickedOnThisFrame = n != null && bt.program != null
						&& bt.status != Status.Ready
						&& n.lastTick == bt.program.tickCount;
					if (hasBeenTickedOnThisFrame)
						break;
				}
			}
			return hasBeenTickedOnThisFrame;
		}

		public static SourceDisplay[] MapGUILines(PandaBTScript[] btlSources, BTProgram program, PandaScriptException[] pandaExceptions)
		{
			if (btlSources == null || program == null)
				return null;

			var sourceDisplays = new SourceDisplay[btlSources.Length];
			for (int i = 0; i < btlSources.Length; i++)
			{
				if (btlSources[i] == null)
					continue;

				SourceDisplay sourceDisplay = null;
				// var tokens = BTLAssetManager.GetCompilationUnits(btlSources, btlSources[i]);
				var cu =  btlSources[i].compilationUnit;
				var tokens = cu.tokens;
				sourceDisplay = BTLGUILine.Analyse(tokens, i);

				sourceDisplays[i] = sourceDisplay;

				CodeMap codemap = null;
				if (program.codemaps != null && i < program.codemaps.Length)
					codemap = program.codemaps[i];

				if (codemap != null)
				{
					BTLGUILine.MapNodes(sourceDisplay.lines, codemap);

					var lines = sourceDisplay.flattenLines;
					foreach (var line in lines)
					{
						foreach (var n in line.btnodes)
						{
							var task = n as BTTask;
							if (task != null && task.boundState != BoundState.Bound)
								line.hasErrors = true;
						}
					}
				}

				if (sourceDisplay != null)
				{
					var lines = sourceDisplay.flattenLines;
					foreach (var line in lines)
					{
						foreach (var pandaException in pandaExceptions)
						{
							if (pandaException != null)
							{
								if (pandaException.filePath == btlSources[i].path && line.lineNumber == pandaException.lineNumber)
								{
									line.hasErrors = true;
								}
							}
						}
					}
				}

			}

			return sourceDisplays;
		}


#endregion

#region runtime highlighting
		public static bool isPaused = false;
		public static bool isPlaying = false;
#endregion


	}




	public class BTLGUILine : System.IDisposable
	{
		private SourceDisplay _sourceDisplay;
		public BTLGUILine(SourceDisplay sd)
		{
			_editableScript = EditableBTScript.current;
			_sourceDisplay = sd;
		}

		public EditableBTScript EditableScript => _editableScript;
		public SourceDisplay SourceDisplay => _sourceDisplay;

		private EditableBTScript _editableScript;
		public int indent = 0;

		int _lineNumber = 0;
		public int lineNumber
		{
			get
			{
				return _lineNumber;
			}

			set
			{
				_lineNumber = value;
				_lineNumberText = string.Format("{0,5:####0} ", _lineNumber);
			}
		}

		bool _isFoldout = false;
		public bool isCollapsed
		{
			get
			{
				return _isFoldout;
			}

			set
			{
				bool hasChanged = _isFoldout != value;
				 
				_isFoldout = value;

				if (hasChanged)
				{
					BehaviourTree behaviourTree = null;
					int scriptIndex = 0;

					if (SourceDisplay.current != null)
					{
						behaviourTree = SourceDisplay.current.bt;
						scriptIndex = SourceDisplay.currentSourceIndex;
					}

					if (behaviourTree != null && behaviourTree.sourceInfos != null && scriptIndex < behaviourTree.sourceInfos.Length)
					{
						var list = behaviourTree.sourceInfos[scriptIndex].collapsedLines;

						if (_isFoldout)
						{
							if (!list.Contains(lineNumber))
								list.Add(lineNumber);
						}
						else
						{
							if (list.Contains(lineNumber))
								list.Remove(lineNumber);
						}
					}
				}
			}
		}
		public bool isFoldable = false;
		public bool hasErrors = false;
		public List<BTLGUILine> children = new List<BTLGUILine>();
		public List<Token> tokens    = new List<Token>();
		public List<BTNode> btnodes = new List<BTNode>();

		string _lineNumberText;
		public string lineNumberText
		{
			get
			{
				return _lineNumberText;
			}
		}



		internal bool _isBreakPointEnable = false;
		internal bool _isSubscribedToDebugBreak = false;
		internal Dictionary<BTNode, Action> _debugBreakCallbacks = new Dictionary<BTNode, Action>();
		public bool isBreakPointEnable
		{
			get
			{
				return _isBreakPointEnable;
			}

			set
			{
				_isBreakPointEnable = value;

				if (btnodes != null)
				{
					if (isBreakPointEnable)
						SubscribeToDebugBreak();
					else
						UnsubscribeToDebugBreak();
				}
			}
		}
		public Status breakPointStatus = Status.Failed;

		public BTNode lastNode
		{
			get
			{
				BTNode node = null;
				foreach (var n in btnodes)
				{
					if (n != null)
					{
						node = n;
					}
				}
				return node;

			}
		}

		public void ClearBreakPoint()
		{
			isBreakPointEnable = false;
			breakPointStatus = Status.Running;
		}

		public void ToggleBreakPoint()
		{
			if( !isBreakPointEnable )
			{
				isBreakPointEnable = true;
				breakPointStatus = Status.Running;
			}else if(breakPointStatus == Status.Running)
			{
				breakPointStatus = Status.Succeeded;
			}else if(breakPointStatus == Status.Succeeded)
			{
				breakPointStatus = Status.Failed;
			}else if(breakPointStatus == Status.Failed)
			{
				breakPointStatus = Status.Aborted;
			}
			else if (breakPointStatus == Status.Aborted)
			{
				isBreakPointEnable = false;
			}



			if (btnodes.Count == 0)
				isBreakPointEnable = false;
		}

		bool _isBreakPointActive;
		public bool isBreakPointActive
		{
			get
			{
				return _isBreakPointActive;
			}
		}


		void DebugBreak(BTNode node)
		{

			_isBreakPointActive = node != null &&
				( 
				   node.previousStatus == Status.Ready && node.status != Status.Ready && breakPointStatus == Status.Running
				|| node.status == Status.Succeeded && breakPointStatus == Status.Succeeded
				|| node.status == Status.Failed && breakPointStatus == Status.Failed
				|| node.status == Status.Aborted && breakPointStatus == Status.Aborted
				);

			if (_isBreakPointActive)
			{
				// SourceDisplay.RefreshAllBehaviourTreeEditors();
				Debug.Break();
			}
		}

		public int lineNumberEnd // The line number where this line block ends
		{
			get
			{
				int max  = this.lineNumber;
				foreach(var c in children)
				{
					max = Mathf.Max(max, c.lineNumberEnd);
				}
				return max;
			}
		}

		public override string ToString()
		{
			string str = "";
			foreach (var t in this.tokens)
				str += t.substring;

			return str;
		}

		public static SourceDisplay Analyse(Token[] tokens, int sourceIndex)
		{
			if (tokens == null)
				return null;

			SourceDisplay sourceDisplay = new SourceDisplay(sourceIndex);
			 int lineNumber = 1;
			var lines = new List<BTLGUILine>();
			var orphans = new List<BTLGUILine>();
			var parentStack = new Stack<BTLGUILine>();

			BTLGUILine current = new BTLGUILine(sourceDisplay);
			current.lineNumber = lineNumber;
			foreach(var t in tokens)
			{
				bool isComment = current.tokens.Count > 0 && current.tokens[0].type ==TokenType.Comment;

				if (t.type ==TokenType.EOL && current.tokens.Count == 0 )
				{// Empty line
					orphans.Add(current);
					++lineNumber;
					current = new BTLGUILine(sourceDisplay);
					current.indent = 0;
					current.lineNumber = lineNumber;
				}
				else if( t.type ==TokenType.EOL && isComment )
				{// End of comments
					++lineNumber;
					orphans.Add(current);
					current = new BTLGUILine(sourceDisplay);
					current.indent = 0;
					current.lineNumber = lineNumber;

				}
				else if (t.type ==TokenType.EOL && !isComment)
				{// End of line containing nodes
					++lineNumber;
					ParentOrAddToLines(lines, parentStack, current);

					if ( current.tokens.Count > 0 )
						parentStack.Push(current);


					current = new BTLGUILine(sourceDisplay);
					current.indent = 0;
					current.lineNumber = lineNumber;

				}
				else if(t.type ==TokenType.Indent && current.tokens.Count == 0)
				{
				   current.indent++;
				}
				else if(t.type ==TokenType.Comment)
				{
					var commentLines = t.substring.Split('\n');
					if (commentLines.Length > 0)
					{
						current.tokens.Add(GenerateCommentToken(commentLines[0], current.lineNumber));
						for (int i = 1; i < commentLines.Length; ++i)
						{
							++lineNumber;
							var commentline = commentLines[i];
							var commentGUILine = GenerateCommentLineGui(commentline, current.indent, lineNumber, sourceDisplay);
							current.children.Add(commentGUILine);
						}
					}
				}
				else
				{
					current.tokens.Add(t);
				}
			}

			ParentOrAddToLines(lines, parentStack, current);


			sourceDisplay.lines = lines.ToArray();
			sourceDisplay.orphans =  orphans.ToArray();

			ProcessFoldable(sourceDisplay.lines);
			ProcessFoldable(sourceDisplay.orphans);

			
			return sourceDisplay;

		}

		private static void ParentOrAddToLines(List<BTLGUILine> lines, Stack<BTLGUILine> parentStack, BTLGUILine current)
		{
			if (current.tokens.Count > 0)
			{
				while (parentStack.Count > 0 && parentStack.Peek().indent >= current.indent)
					parentStack.Pop();
			}

			if (parentStack.Count > 0 && !parentStack.Peek().children.Contains(current))
			{
				parentStack.Peek().children.Add(current);
			}
			else
			{
				if(!lines.Contains(current))
					lines.Add(current);
			}
		}

		static BTLGUILine GenerateCommentLineGui(string lineContent, int indent, int lineNumber, SourceDisplay sourceDisplay)
		{
			var commentGUILine = new BTLGUILine(sourceDisplay);
			commentGUILine.indent = indent;
			commentGUILine.lineNumber = lineNumber;
			string tabs= "";
			for (int i = 0; i < indent; ++i)
				tabs += "\t";

			string cleaned = lineContent;
			if( tabs != "")
			  cleaned =  cleaned.Replace(tabs, "");

			cleaned = cleaned.Replace("\t", "    ");

			var token = GenerateCommentToken(cleaned, lineNumber);
			commentGUILine.tokens.Add( token );
			return commentGUILine;
		}

		private static Token GenerateCommentToken(string lineContent, int lineNumber)
		{
			var token = new Token(TokenType.Comment, 0, lineContent.Length, lineContent, 0);
			token.line = lineNumber;
			return token;
		}

		static void ProcessFoldable(BTLGUILine[] lines )
		{
			var stack = new Stack<BTLGUILine>();

			for (int i = lines.Length - 1; i >= 0; i--)
				stack.Push(lines[i]);

			while (stack.Count > 0)
			{
				var line = stack.Pop();
				bool hasEmptyChildren = true;
				for (int i = line.children.Count - 1; i >= 0; i--)
				{
					stack.Push(line.children[i]);
					if( line.children[i].tokens.Count > 0)
					{
						hasEmptyChildren = false;
					}
				}
				line.isFoldable = line.children.Count > 0 && line.tokens.Count > 0 && !hasEmptyChildren;
			}
		}

		public static void MapNodes( BTLGUILine[] lines,  CodeMap codemap )
		{
			var stack = new Stack<BTLGUILine>();

			for (int i = lines.Length - 1; i >= 0; i--)
				stack.Push(lines[i]);

			while (stack.Count > 0)
			{
				var line = stack.Pop();
				MapNodes(line, codemap);
				for (int i = line.children.Count - 1; i >= 0; i--)
					stack.Push(line.children[i]);
			}
		}

		// Map each token to its corresponding node.
		public static void MapNodes( BTLGUILine line,  CodeMap codemap )
		{
			line.btnodes.Clear();
			for (int t = 0; t < line.tokens.Count; ++t)
				line.btnodes.Add(null);

			for (int t = 0; t < line.tokens.Count; ++t)
			{
				
				var token = line.tokens[t];

				if ( token.type ==TokenType.Comment )
					continue;

				for (int n = 0; n < codemap.nodes.Length; ++n)
				{
					var node = codemap.nodes[n];
					var nodeLoc = codemap.substringLocations[n];

					if( 
						 nodeLoc.start <= token.substring_start
						&& (token.substring_start + token.substring_length) <= (nodeLoc.start + nodeLoc.length)
						)
					{
						line.btnodes[t] = node;
						break;
					}

				}
			}
		}

		private void UnsubscribeToDebugBreak()
		{
			if (_isSubscribedToDebugBreak)
			{
				foreach(var pair in _debugBreakCallbacks)
				{
					var node = pair.Key;
					var callback = pair.Value;
					node.OnTick -= callback;
					node.OnAbort -= callback;

				}
				_debugBreakCallbacks.Clear();
				_isSubscribedToDebugBreak = false;
			}
		}

		private void SubscribeToDebugBreak()
		{
			if (!_isSubscribedToDebugBreak)
			{
				foreach (var n in btnodes)
				{
					if (n != null)
					{
						Action callback = () => DebugBreak(n);
						_debugBreakCallbacks[n] = callback;
						n.OnTick += callback;
						n.OnAbort += callback;
					}
				}
				_isSubscribedToDebugBreak = true;
			}
		}



		public void Dispose()
		{
			isBreakPointEnable = false;
		}
	}

}
