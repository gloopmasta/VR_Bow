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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AsyncTask = System.Threading.Tasks.Task<bool>;

using PandaBT.Runtime;

namespace PandaBT.Compilation
{

	public class BTRuntimeBuilder
	{

		public static BTProgram BuildProgram(CompilationUnit[] cus)
		{
			BTProgram program = new BTProgram();
			var prevProgram = BTProgram.current;
			BTProgram.current = program;
			program.sourcesHash = GetSourcesHash(cus);

			var hasExceptions = cus.Where( cu => cu.exceptionsCount > 0 ).Count() > 0;

			if (hasExceptions)
			{
				var exceptions = cus.Select(cu => cu.exceptions).Where( e => e != null).SelectMany( e => e).ToArray();
				program._exceptions = exceptions.Select(d => new PandaScriptException(d)).ToArray();
				return program;
			}

			int n = cus.Length;
			program._runtimeUnits = new RuntimeUnit[n];
			program._codemaps = new CodeMap[n];

			for (int i = 0; i < n; ++i)
			{
				program._runtimeUnits[i] = Build(cus[i], out program._codemaps[i], true);
			}

			List<BTTree> trees = new List<BTTree>();
			foreach (var ru in program.runtimeUnits)
				trees.AddRange(ru.trees);

			BTRuntimeBuilder.ResolveTreeReferences(trees.ToArray());
			BTProgram.current = prevProgram;
			return program;
		}

		static RuntimeUnit Build(CompilationUnit cu, out CodeMap codemap, bool createCodeMap )
		{
			var codeMapDict = new Dictionary<BTNode, SubstringLocation>();
			List<BTTree> trees = new List<BTTree>();


			// Get all parsed nodes
			var parseNodes = cu.nodes;
			
			var nodeID = new Dictionary<Node, int>();
			for(int i=0; i < parseNodes.Length;++i)
			{
				nodeID[parseNodes[i]] = i;
			}
			
			
			// create runtime nodes
			var nodes = new BTNode[ parseNodes.Length ];
			foreach( var n in parseNodes )
			{
				int id  = nodeID[n];
				var nodeToken = cu.GetToken(n);
				switch (nodeToken.type)
				{
				case TokenType.Tree: 
					var tree = new BTTree();

					if (n.parameters.Count > 0)
					{
						tree.name = n.parsedParameters[0] as string;
					}

					nodes[id] = tree;
					trees.Add(tree);
					
					break;
				
				case TokenType.TreeReference: 
					var treeRef = new BTTreeReference();

					if (n.parameters.Count > 0)
						treeRef.name = n.parsedParameters[0] as string;
					
					nodes[id] = treeRef;
					
					break;

				case TokenType.TreeDefinition:
					var treeDefinition = new BTTree();
					treeDefinition.name = nodeToken.substring.Substring(1);
					nodes[id] = treeDefinition;
					trees.Add(treeDefinition);
					break;

				case TokenType.TreeCall:
					var treeCall = new BTTreeReference();
					treeCall.name = nodeToken.substring.Substring(1);
					nodes[id] = treeCall;
					break;

				case TokenType.Value:
				case TokenType.Word:
					
					var task = new BTTask();
					task.taskName = nodeToken.substring.Trim();

					nodes[id] = task;
					
					break;
					
				case TokenType.Mute:
					nodes[id] = new BTMute();
					break;

				case TokenType.NotOperator:
				case TokenType.Not:
					nodes[id] = new BTNot();
					break;
				
				case TokenType.Repeat:
					nodes[id] = new BTRepeat();
					break;

				case TokenType.Retry:
					nodes[id] = new BTRetry();
					break;

					case TokenType.If:
				case TokenType.IfElse:
					nodes[id] = new BTIf();
					break;
				case TokenType.Before:
					nodes[id] = new BTBefore();
					break;
				case TokenType.SequenceOperator:
				case TokenType.Sequence: 	nodes[id] = new BTSequence(); break;
				case TokenType.FallbackOperator:
				case TokenType.Fallback: nodes[id] = new BTFallback(); break;
				case TokenType.ParallelOperator:
				case TokenType.Parallel: 	nodes[id] = new BTParallel(); break;
				case TokenType.RaceOperator:
				case TokenType.Race: nodes[id] = new BTRace(); break;
				case TokenType.While: 	nodes[id] = new BTWhile(); break;
				case TokenType.Random: nodes[id] = new BTRandom(); break;
				case TokenType.Shuffle: nodes[id] = new BTShuffle(); break;
				case TokenType.Break: nodes[id] = new BTBreak(); break;
				}



				if ( nodes[id] != null)
				{
					nodes[id].i_parameters = n.parsedParameters;
				}

				if( nodes[id] != null )
				{
					var loc = new SubstringLocation();
					loc.line = nodeToken.line;
					loc.start = nodeToken.substring_start;
					loc.length = nodeToken.substring_length;

					if( n.parameters.Count > 0)
					{
						loc.length = n.parseLength;
					}

					codeMapDict[nodes[id]] = loc;
				}
			}


			// parenting
			foreach ( var n in parseNodes )
			{
				int pid = nodeID[n];
				var parent = nodes[pid];
				if( parent != null )
				{
					var children = cu.GetChildren(n);
					foreach( var c in children)
					{
						if (!nodeID.ContainsKey(c))
							continue;
						int cid = nodeID[c];
						var child = nodes[cid];
						if( child != null )
							parent.AddChild( child );
					}
				}
			}

			if (createCodeMap)
				codemap = new CodeMap(codeMapDict);
			else
				codemap = null;

			var runtimeUnit = new RuntimeUnit();
			runtimeUnit.trees = trees.ToArray();
			return runtimeUnit;
			
		}

		static bool Locate( BTProgram program, BTTask task, out string path, out int line)
		{

			bool found = false;
			path = null;
			line = 0;
			for(int c =0; c < program.codemaps.Length; c++ )
			{
				var cm = program.codemaps[c];
				for ( int i=0; i < cm.nodes.Length; i++)
				{
					if( cm.nodes[i] == task)
					{
						path = program.btSources[c].url;
						line = cm.substringLocations[i].line;
						found = true;
						break;
					}
				}

				if (found)
					break;
			}

			return found;

		}

		public static void Bind( BTProgram program, TaskBinder[] taskBinders, VariableBinder[] variableBinders )
		{

			// Get all the [Task]s.
			List<Exception> exceptions = new List<Exception>();
				
			var variableExceptions = ResolveVariableReferences(program, variableBinders);
			if( variableExceptions.Count > 0)
				exceptions.AddRange(variableExceptions);

			// Find for each task the corresponding implementations.
			var taskBinderCandidates = new Dictionary<BTTask, List<TaskBinder>>();
			var tasks = program.tasks;
			foreach (var task in tasks)
			{
				taskBinderCandidates[task] = new List<TaskBinder>();
				foreach (var tb in taskBinders)
				{
					if (tb.IsMatch(task, variableBinders ))
					{
						taskBinderCandidates[task].Add(tb);
					}
				}
			}

			// Check whether all the task are unambiguously defined.
			var unimplementedTasks = new List<BTTask>();
			var over_implementedTasks = new List<BTTask>();
			Func<BTTask, bool> hasUndefinedVariables = (t) =>
			{
				bool itHas = false;
				foreach (var p in t.i_parameters)
				{
					var varRef = p as VariableParameter;
					if (varRef != null)
					{
						var variable = variableBinders.Where( vb => vb.name == varRef.name ).FirstOrDefault()?.variable;
						itHas = variable == null;
						if (itHas)
							break;
					}
				}
				return itHas;
			};

			foreach (var kv in taskBinderCandidates)
			{
				var task = kv.Key;
				var candidates = kv.Value;

				// if a task as un undefined variable as parameter, there was already an exception raised about that.
				// no need to raise an extra exception.
				if (candidates.Count == 0 && !hasUndefinedVariables(task))
					unimplementedTasks.Add(task);

				if (candidates.Count > 1)
					over_implementedTasks.Add(task);
			}

			if (unimplementedTasks.Count > 0)
			{
				int line;
				string path;
				foreach (var task in unimplementedTasks)
				{
					var m = GetMethodSignatureString(task);
					string msg = string.Format("The task `{0}' is not defined.\n", m);
					msg += string.Format("\nA method as follow is expected:\n");

					string tpl = taskTpl;
					tpl = tpl.Replace("{!methodSignature}", m);
					tpl = tpl.Replace("{!statusType}", "Status");
					msg += tpl;

					if (Locate(program, task, out path, out line))
					{
						exceptions.Add(new PandaScriptException(msg, path, line));
					}
					else
					{
						exceptions.Add(new System.NotImplementedException(msg));
					}
				}
			}

			// Check method return type (must be void or AsyncTask)
			foreach (var kv in taskBinderCandidates)
			{
				var task = kv.Key;
				var candidates = kv.Value;

				foreach (var c in candidates)
				{
					int line;
					string path;
					MethodInfo method = c.memberInfo as MethodInfo;
					if (method != null)
					{
						if (!(method.ReturnType == typeof(void) || method.ReturnType == typeof(bool) || method.ReturnType == typeof(AsyncTask)))
						{
							var m = GetMethodSignatureString(task);
							string msg = string.Format("The [Task] `{0}' must return void, bool, or System.Threading.Tasks.Task<bool>.\n", method.Name);
							if (Locate(program, task, out path, out line))
							{
								exceptions.Add(new PandaScriptException(msg, path, line));
							}
						}
					}
				}

			}

			if (over_implementedTasks.Count > 0)
			{
				int line;
				string path;
				foreach (var task in over_implementedTasks)
				{
					var m = GetMethodSignatureString(task);
					string msg = string.Format("The task `{0}' has too many definitions.\n", m);
					msg += string.Format("The task is implemented in:\n");

					foreach (var o in taskBinderCandidates[task])
					{
						msg += string.Format(" - `{0}'\n", o.target.GetType().ToString());
					}

					if (Locate(program, task, out path, out line))
					{
						exceptions.Add(new PandaScriptException(msg, path, line));
					}
					else
					{
						exceptions.Add(new System.NotImplementedException(msg));
					}
				}
			}

			if (exceptions.Count == 0)
				exceptions.AddRange(CheckRootTree(program));

			program._exceptions = exceptions.ToArray();

			// Bind the tasks.
			foreach (var kv in taskBinderCandidates)
			{
				var task = kv.Key;
				var candidates = kv.Value;

				if (candidates.Count == 1)
				{
					var ti = candidates[0];
					Bind(task, ti);
				}
				else
				{
					task.Unbind();
				}
			}
		}

		private static List<Exception> ResolveVariableReferences(BTProgram program, VariableBinder[] variableBinders)
		{

			program.i_variables = variableBinders.Select( vb => vb.variable ).ToArray();
			// Generate eventual exceptions.
			var exceptions = new List<System.Exception>();

			// check for parameter name uniqueness
			var varBindersGoupedByName = from grp in
								 (from p in variableBinders
								  group p by p.variable.name into grp
								  select new { name = grp.Key, count = grp.Count() })
										 where grp.count > 0
										 select grp;

			var duplicatedVarBinders = (from grp in varBindersGoupedByName
										where grp.count > 1
										select grp).ToArray();

			bool hasVariableDuplication = duplicatedVarBinders.Length > 0;
			if (hasVariableDuplication)
			{
				var names = from d in duplicatedVarBinders select d.name;
				var duplicateParamExceptions = (
										  from n in names
										  select new Exception($"ERROR: Multiple implementation of variable @{n}")
										  ).ToArray();
				exceptions.AddRange(duplicateParamExceptions);
			}

			return exceptions;
		}

		static IVariable ResolveVariablePath(IVariable var, string[] path)
		{
			IVariable resolvedVar = null;
			IVariable parent= var;
			for(var i = 1; i < path.Length; i++)
			{
				bool isLast = i == path.Length - 1;
				var name = path[i];
				if( parent != null)
				{
					var value = parent.value;
					var memberInfo = value.GetType().GetMember(name).FirstOrDefault();
					if( memberInfo != null)
					{
						var reflectedVar = new PandaBT.Runtime.ReflectedVariable(name, memberInfo, value, parent);
						if( isLast)
							resolvedVar = reflectedVar;
						else
							parent = reflectedVar;
					}
				}
			}
			return resolvedVar;
		}

		static bool Bind( BTTask task, TaskBinder taskBinder)
		{
			bool bindingSucceeded = false;

			var memberInfo = taskBinder.memberInfo;
			var target = taskBinder.target;

			MethodInfo method = memberInfo as MethodInfo;
			FieldInfo field = memberInfo as FieldInfo;
			PropertyInfo property = memberInfo as PropertyInfo;

			System.Action taskAction = null;

			if (method != null)
				taskAction = BindMethod(task, target, method);

			if (field != null)
				taskAction = () => UserTask.current.Complete((bool)field.GetValue(target));

			if (property != null)
				taskAction = () => UserTask.current.Complete((bool)property.GetValue(target, null));

			if (taskAction != null)
			{
				task.m_taskBinder = taskBinder;
				task.m_taskAction = taskAction;
				task.m_boundState = BoundState.Bound;
				task.m_parameters = null;
				task._parameterTypes = null;

				bindingSucceeded = true;
			}

			return bindingSucceeded;
		}

		private static Action BindMethod(BTTask task, object target, MethodInfo method)
		{
			Action taskAction = null;
			var methodParameters = method.GetParameters();

			// void method
			if (method.ReturnType == typeof(void))
			{
				if (methodParameters.Length > 0)
					taskAction = () => method.Invoke(target, task.ParameterValues);
				else
					taskAction = System.Delegate.CreateDelegate(typeof(System.Action), target, method) as Action;
			}

			// bool method
			if (method.ReturnType == typeof(bool))
			{
				if (methodParameters.Length > 0)
				{
					taskAction = () => UserTask.current.Complete((bool)method.Invoke(target, task.ParameterValues));
				}
				else
				{
					var del = System.Delegate.CreateDelegate(typeof(Func<bool>), target, method) as Func<bool>;
					taskAction = () => UserTask.current.Complete(del());
				}
			}

			// AsyncTask method
			if (method.ReturnType == typeof(AsyncTask))
			{
				Func<AsyncTask> asyncTaskFn;
				if (methodParameters.Length > 0)
				{
					asyncTaskFn = () => method.Invoke(target, task.ParameterValues) as AsyncTask ;
				}
				else
				{
					var del = Delegate.CreateDelegate(typeof(System.Func<AsyncTask>), target, method) as System.Func<AsyncTask>;
					asyncTaskFn = () => del();
				}

				taskAction = () =>
				{
					if (UserTask.current.isStarting)
					{
						UserTask.current._task.m_status = Status.Running;
						UserTask.current._asyncTask = asyncTaskFn();
					}

					var	asyncTask = UserTask.current._asyncTask;

					if( asyncTask.IsCompleted)
						UserTask.current.Complete( asyncTask.Result);
				};
			}

			return taskAction;
		}

		public static System.Exception[] CheckRootTree( BTProgram program )
		{
			var exceptions = new List<System.Exception>();
			var rus = program.runtimeUnits;

			int mainCount = 0;

			if (rus != null)
			{
				for (int i = 0; i < rus.Length; ++i)
				{
					var ru = rus[i];
					if (ru != null)
					{
						for (int j = 0; j < ru.trees.Length; ++j)
						{
							var b = ru.trees[j];
							if (b.name != null && b.name.ToLower() == "root")
								++mainCount;
						}
					}
				}
			}

			if( mainCount == 0)
			{
				string msg = string.Format("No root tree  found. tree \"Root\" is expected.\n");
				exceptions.Add(new System.Exception(msg));
			}

			if (mainCount > 1)
			{
				string msg = string.Format("Too many root trees found. Only one tree \"Root\" is expected.\n");
				exceptions.Add(new System.Exception(msg));
			}


			if( exceptions.Count > 0 )
			{
				program._boundState = BoundState.Failed;
				program._codemaps = null;
			}

			return exceptions.ToArray();
		}
		
		static string GetMethodSignatureString( BTTask task )
		{
			var parameterArray = task.i_parameters;
			// expected method signature 
			string sign = "void" + " " + task.taskName + "(";
			if( parameterArray != null)
			{
				for(int p=0; p < parameterArray.Length; ++p)
				{
					var variable = parameterArray[p] as IVariable;
					if( variable != null)
					{
						var strType = (variable.valueType != null ? variable.valueType.ToString() : "?");
						sign += " " + strType;
					}
					else
					{
						var type = parameterArray[p]?.GetType();
						sign += " " + (type == null ? "null": type.ToString() );
					}
					
					sign += " p" + p.ToString();
					
					if( p +1 < parameterArray.Length ) sign += ",";
				}
				sign += " " ;
			}
			sign += ")";
			sign = sign.Replace("System.Single", "float");
			sign = sign.Replace("System.Int32", "int");
			sign = sign.Replace("System.Boolean", "bool");
			sign = sign.Replace("System.String", "string");
			return sign;
		}

		public static void ResolveTreeReferences(BTTree[] trees)
		{
			foreach (var tree in trees)
			{
				if (tree != null)
				{
					// Resolve tree references
					foreach (var treeRef in tree.treeReferences)
					{
						foreach (var t in trees)
						{
							if (t.name == treeRef.name)
								treeRef.target = t;
						}
					}
				}
			}
		}


		public static BTTree GetMain( BTTree[] _trees )
		{
			BTTree main = null;
			if (_trees != null && _trees.Length > 0)
			{
				// Search for the main tree
				foreach (var bt in _trees)
				{
					if (bt.name.ToLower() == "root")
					{
						main = bt;
					}
				}

				if (main == null)
					main = _trees[0];

			}

			return main;
		}

		
		public static bool IsStale(BTProgram program, IEnumerable<CompilationUnit> cus)
		{
			bool itIs = false;
			if( program != null && cus != null)
			{
				var sourcesHashes = GetSourcesHash(cus);
				itIs = program.sourcesHash != sourcesHashes;
			}
			return itIs;
		}

		private static string GetSourcesHash(IEnumerable<CompilationUnit> cus)
		{
			string sourcesHash = null;
			if (cus != null)
			{
				sourcesHash = "";
				foreach (CompilationUnit cu in cus)
					sourcesHash += cu.sourceHash;
			}

			return sourcesHash;
		}


		const string taskTpl = @"
			[Task]
			{!methodSignature}
			{
				var task = Task.current;

				if( task.isStarting )
				{// Use this for initialization 
					
				}

				throw new System.NotImplementedException();
			}
";
	}


}
