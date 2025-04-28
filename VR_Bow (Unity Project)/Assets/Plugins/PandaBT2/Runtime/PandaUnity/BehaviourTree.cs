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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PandaBT.Compilation;

namespace PandaBT.Runtime
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class BehaviourTree : MonoBehaviour
	{
		[Serializable]
		public class InspectorGuiData
		{
			[SerializeField]
			public bool isFoldout = true; // whether the source file appears folded.

			[SerializeField]
			public List<int> breakPoints = new List<int>(); // List of line numbers where a bp is set.

			[SerializeField]
			public List<Status> breakPointStatuses = new List<Status>();

			public List<int> collapsedLines = new List<int>(); // List of collapsed lines.

			public void AddBreakPoint( int line, Status status)
			{
				RemoveBreakPoint( line );
				if( status != Status.Ready)
				{
					breakPoints.Add(line);
					breakPointStatuses.Add(status);
				}
			}

			public void RemoveBreakPoint(int line)
			{
				int i;
				do
				{
					i = breakPoints.FindIndex(b => b == line);
					if (i != -1)
					{
						breakPoints.RemoveAt(i);
						breakPointStatuses.RemoveAt(i);
					}
				} while (i != -1);
			}
		}

		/// <summary>
		/// BT scripts
		/// </summary>
		public PandaBTScript[] scripts;

		[NonSerialized]
		public bool _isInspected = false;


		PandaBTScript[] _bindScripts;
		public PandaBTScript[] bindScripts
		{
			get
			{
				if( _bindScripts == null && scripts?.Length > 0)
					_bindScripts = scripts.Where(s => s != null).ToArray();
				return _bindScripts;
			}
		}

		private float _lastTickTime = float.NegativeInfinity;

		private static bool _variableFinderRegistered = false;
		public BindingInfo bindingInfo => program.bindingInfo;

		public IVariable[] variables => program?.variables;
		private Dictionary<string, IVariable> _variableDictionary;

		public IVariable GetVariable(string name)
		{
			IVariable variable = null;
			if( _variableDictionary == null)
			{
				_variableDictionary = new Dictionary<string,IVariable>();
				foreach( var v in variables )
					_variableDictionary[v.name] = v;
			}

			if( _variableDictionary.ContainsKey(name) )
				variable = _variableDictionary[name];

			return variable;
		}


		public void Apply()
		{
			_bindScripts = null;
			_exceptions = null;

			if( program != null)
			{
				var cus = bindScripts.Select(script => script.compilationUnit);
				if( BTRuntimeBuilder.IsStale(program, cus))
				{
					_program.Dispose();
					_program = null;
					_requiresRebinding = true;
				}
			}

			if( _program == null)
				_requiresRebinding = true;
		}

		/// <summary>
		/// Whether the root node is automatically reset when completed.
		/// </summary>
		public bool autoReset = true;

		/// <summary>
		/// On which update function the BT is ticked.
		/// </summary>
		public UpdateOrder tickOn = UpdateOrder.Update;
		public float customTickOnInterval = 1.0f;

		/// <summary>
		/// Whether the `repeat/retry` node restart on the same tick.
		/// </summary>
		public bool repeatRetryOnSameTick = false;

		public InspectorGuiData[] sourceInfos;

		/// <summary>
		/// Callback triggered when the BT is initialized.
		/// </summary>
		public System.Action OnInitialized;

		/// <summary>
		/// Callback triggered when the BT is ticked.
		/// </summary>
		public System.Action OnTicked;

		System.Exception[] _exceptions;
		public System.Exception[] exceptions
		{
			get
			{
				if (_exceptions == null)
					_exceptions = new System.Exception[0];

				return _exceptions;
			}
		}

		PandaScriptException[] _pandaExceptions;
		public PandaScriptException[] pandaExceptions
		{
			get
			{
				if (_pandaExceptions == null)
				{

					if(_exceptions != null)
					{
						_pandaExceptions = _exceptions
							.Select( e => e as PandaScriptException)
							.Where( e => e != null )
							.ToArray();
					}
					else
					{
						_pandaExceptions = new PandaScriptException[0];
					}
				}
				return _pandaExceptions;
			}
		}

		bool _isInitialized = false;
		bool _requiresRebinding = false;
		/// <summary>
		/// The BT current status.
		/// </summary>
		public Status status
		{
			get
			{
				return _program != null? _program.status: Status.Failed;
			}
		}

		public enum UpdateOrder
		{
			Update,
			LateUpdate,
			FixedUpdate,
			Custom,
			Manual
		}

		public BTProgram program 
		{ 
			get
			{
				return _program; 
			} 
		}

		/// <summary>
		/// Tick the BT.
		/// </summary>
		public virtual void Tick()
		{
			if (!_isInitialized)
				return;

				_lastTickTime = Time.time;
				if (program != null && this.enabled)
				{
					UserTask.isInspected = this._isInspected;
					program.repeatRetryOnSameTick = this.repeatRetryOnSameTick;
					if (program.status != Status.Running && program.status != Status.Ready && autoReset)
						program.Reset();

					program.Tick();

					if (OnTicked != null)
						OnTicked();

					UserTask.isInspected = false;
				}


		}

		/// <summary>
		/// Bind the BT script to the PandaTasks
		/// and prepare for execution
		/// </summary>
		public void Bind()
		{
            var settings = PandaBT.Config.PandaBTSettings.instance; // access the instance to be sure it's in the scene
            if (settings == null)
            {
                Debug.LogError("PandaBTSettings not found");
            }

			_exceptions = null;
			_pandaExceptions = null;
			_requiresRebinding = false;

			if (_program != null)
			{
				_program.Dispose();
				_program = null;
			}

			var scripts = bindScripts?.Where(s => s != null).ToArray();
			bool hasScript = scripts?.Count() > 0;
			bool hasExceptions = hasScript && scripts.Where( s => s.compilationUnit.exceptionsCount > 0 ).Count() > 0;
			if (hasScript)
				CreateProgramAndBind();

		}
	

		private void CreateProgramAndBind()
		{
			RecompileBindScriptsIfChanged();

			var cus = bindScripts.Where(s => s != null).Select(s => s.compilationUnit).ToArray();
			_program = BTRuntimeBuilder.BuildProgram(cus);
			_program.btSources = bindScripts.Select(src => new BTSourceString(src.source, src.path)).ToArray();

			try
			{
				RegisterVariableFinder();
				var bindingInfo = BinderUtils.GetBindingInfo(this.gameObject);
				_program.Bind(bindingInfo);
				_exceptions = _program.exceptions;

				foreach (var ex in _program.exceptions)
					Debug.LogException(ex, this);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e, this);
				_program = null;
			}

			if (_program != null)
				_program.Reset();
		}

		private void RecompileBindScriptsIfChanged()
		{
			foreach (var s in bindScripts)
			{
				if (s == null)
					continue;

				bool requiresCompilation = true;
				if (s.compilationUnit != null)
				{
					var srcHash = Compiler.Hash(s.source);
					requiresCompilation = srcHash != s.compilationUnit.sourceHash;

					if( s.compilationUnit.nodes != null)
					{
						foreach (var node in s.compilationUnit.nodes)
							node.Reset(s.compilationUnit);
					}
				}

				if (requiresCompilation)
				{
					s.compilationUnit = Compiler.Compile(s.source, s.path);
				}
			}
		}

		public void DebugLogErrors()
		{
			if (_program != null)
			{
				foreach (var ex in _program.exceptions)
					Debug.LogException(ex, this);
			}
		}

		/// <summary>
		/// Reset the BT.
		/// </summary>
		public void Reset()
		{
			if (program != null)
				program.Reset();
		}


#region internals

		BTProgram _program;

#endregion

		void Initialize()
		{
			_bindScripts = null;
			Bind();
			_isInitialized = true;
			if (OnInitialized != null)
				OnInitialized();
		}

		private void OnEnable()
		{
			Initialize();
		}

		private void OnDisable()
		{
			if (_program != null)
			{
				_program.Dispose();
				_program = null;
			}
			_isInitialized = false;
			_bindScripts = null;
		}

		protected virtual void Update()
		{
			if(!_isInitialized)
			{
				Initialize();
			}

			if( _requiresRebinding){
				Bind();
			}

			if (tickOn == UpdateOrder.Update && Application.isPlaying)
				Tick();
			else if (tickOn == UpdateOrder.Custom && Application.isPlaying && _lastTickTime + customTickOnInterval < Time.time)
				Tick();

		}

		protected virtual void FixedUpdate()
		{
			if (tickOn == UpdateOrder.FixedUpdate && Application.isPlaying)
				Tick();
		}

		protected virtual void LateUpdate()
		{
			if (tickOn == UpdateOrder.LateUpdate && Application.isPlaying)
				Tick();
		}

		protected virtual void OnDestroy()
		{
			OnInitialized = null;
		}

		/// <summary>
		/// (Serializable) The current BT execution state.
		/// </summary>
		public BTSnapshot snapshot
		{
			get
			{
				BTSnapshot _state = null;
#if !PANDA_BT_FREE
				if (program != null)
					_state = program.snapshot;
#else
				Debug.LogWarning("PandaBehaviour.snapshot is Pro feature.");
#endif
				return _state;
			}

			set
			{
#if !PANDA_BT_FREE
				if (program != null)
					program.snapshot = value;
#endif
			}
		}


		private List<PandaTree> getTreeCache = new List<PandaTree>();
		/// <summary>
		/// Returns the tree having the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PandaTree GetTree(string name)
		{
			PandaTree wantedTree = getTreeCache.Where( tree => tree.name == name ).FirstOrDefault();

			if (this.program != null && wantedTree == null)
			{
				var matchingTree = this.program.runtimeUnits
					.Select(ru => ru.trees)
					.SelectMany(tree => tree)
					.Where(tree => tree.name == name)
					.FirstOrDefault();

				if( matchingTree != null)
				{
					wantedTree = new PandaTree();
					var treeRef = new BTTreeReference();
					treeRef.target = matchingTree;
					treeRef.name = name;
					wantedTree.tree = treeRef;
					getTreeCache.Add(wantedTree);
				}
			}

			return wantedTree;
		}

		private static void RegisterVariableFinder()
		{
			if( !_variableFinderRegistered)
			{
#if PANDA_BT_WITH_VISUAL_SCRIPTING
				VariableBinder.RegisterVariableFinder(PandaBT.Compilation.VisualScriptingVariableFinder.FindVisualScriptingVariables);
#endif
				_variableFinderRegistered = true;
			}
		}
	}
}
