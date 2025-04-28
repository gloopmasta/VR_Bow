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

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Runtime
{
	public static class BinderUtils
	{

		static int _cacheFrame = -1;
		static Dictionary<GameObject, BindingInfo> _BindingInfocache = new Dictionary<GameObject, BindingInfo>();

		public static BindingInfo GetBindingInfo(GameObject gameObject, bool resolvePandaLinker = true)
		{
			BindingInfo bindingInfo = null;
			if ( Application.isPlaying )
			{
				if (_cacheFrame != Time.frameCount)
					_BindingInfocache.Clear();

				if (_BindingInfocache.ContainsKey(gameObject))
				{
					bindingInfo = _BindingInfocache[gameObject];
				}
			}

			
			if( bindingInfo != null )
				return bindingInfo;

			Component[] targetObjects = GetTargetComponents(gameObject, resolvePandaLinker);

			var taskBinders = TaskBinder.Get(targetObjects);
			var variableBinders = VariableBinder.Get(targetObjects);

			bindingInfo = new BindingInfo(taskBinders, variableBinders);

			_BindingInfocache[gameObject] = bindingInfo;
			return bindingInfo;
		}


		public static Component[] GetTargetComponents(GameObject gameObject, bool resolvePandaLinker = true)
		{

			IEnumerable<GameObject> targets = null;

			if( resolvePandaLinker)
			{
				targets = ResolvePandaLinker(gameObject);
			}
			else
			{
				targets = new GameObject[1] {  gameObject };
			}
			var components = targets.Select(t => t.GetComponents<Component>()).SelectMany(c => c).Where(c => IsBindable(c)).ToArray();
			return components;
		}

		public static IEnumerable<GameObject> ResolvePandaLinker(GameObject gameObject, bool includeSceneLinkers = true)
		{
			var queue = new Queue<GameObject>();
			var targets = new List<GameObject>();

			queue.Enqueue(gameObject);
			if (includeSceneLinkers)
			{
				foreach (var linker in PandaLinker.SceneLinkers)
				{
					if( linker != null && linker.gameObject != null)
					{
						queue.Enqueue( linker.gameObject );
						targets.Add( linker.gameObject );

					}
				}
			}

			var visited = new List<GameObject>();
			targets.Add(gameObject);
			while (queue.Count > 0)
			{
				var go = queue.Dequeue();
				if (go == null)
					continue;

				var linkers = new List<PandaLinker>();
				linkers.AddRange(go.GetComponents<PandaLinker>().Where(l => IsBindable(l)));

				var btTargets = linkers
					.Where(linker => (linker.Targets != null && linker.Targets.Length > 0 || linker.LinkTo == PandaLinkerScope.AllPandaBehavioursInScene))
					.SelectMany(btTarget => btTarget.Targets)
					.Where(target => target != null && !targets.Contains(target)).ToList();

				foreach (var t in btTargets)
				{
					if (!visited.Contains(t))
						queue.Enqueue(t);
				}

				targets.AddRange(btTargets);
				visited.Add(go);
			}

			return targets;
		}

		public static bool IsBindable(Component component)
		{
			if (object.ReferenceEquals(component, null)) return false;
			if (component is UnityEngine.Object && component as UnityEngine.Object == null)
				return false;

			var monoBehaviour = component as MonoBehaviour;
			if( monoBehaviour != null)
			{
				if (monoBehaviour as PandaLinker != null)
					return true;

				if (PandaBTReflectionDatabase.GetMemberInfos(monoBehaviour.GetType()).Length > 0)
					return true;
			}

			if( HasVariables(component) )
				return true;

			return false;
		}

		static bool HasVariables(Component component)
		{

			var type  = component.GetType();
			if (PandaBTReflectionDatabase.GetVariableMemberInfos(type).Length > 0)
				return true;

			var variableBinders = VariableBinder.Get(component);
			if (variableBinders != null)
				return variableBinders.Count() > 0;

			return false;
		}

		public static void ClearCache()
		{
			_BindingInfocache.Clear();
		}
	}
}
