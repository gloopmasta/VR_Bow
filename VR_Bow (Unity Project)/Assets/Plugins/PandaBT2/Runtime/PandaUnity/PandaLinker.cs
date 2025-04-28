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
using UnityEngine;
using PandaBT.Runtime;

namespace PandaBT
{
	[ExecuteInEditMode]
	[AddComponentMenu("Panda BT/Panda Linker")]
	public class PandaLinker : MonoBehaviour
	{
		public PandaLinkerScope LinkTo = PandaLinkerScope.ThisPandaBehaviour;
		public GameObject[] Targets;

		public static PandaLinker[] SceneLinkers
		{
			get
			{
				if (_sceneLinkersArray == null)
				{
					_sceneLinkersArray = _sceneLinkers.Where( l => l != null).ToArray();
				}
				return _sceneLinkersArray;
			}
		}
		private static List<PandaLinker> _sceneLinkers = new List<PandaLinker>();
		private static PandaLinker[] _sceneLinkersArray = null;

		internal static void ClearSceneLinkers()
		{
			_sceneLinkers.Clear();
			_sceneLinkersArray = null;

		}

		private void Awake()
		{
			Initialize();
		}
		private void OnEnable()
		{
			Initialize();
		}

		private void OnDisable()
		{
			if (_sceneLinkers.Contains(this))
			{
				_sceneLinkers.Remove(this);
				_sceneLinkersArray = null;
			}
		}

		private void Reset()
		{
			var components = this.gameObject.GetComponents<MonoBehaviour>();
			bool hasPandaBehaviour = components.FirstOrDefault(c => c.GetType().Name == "PandaBehaviour") != null;

			if( hasPandaBehaviour )
			{
				LinkTo = PandaLinkerScope.ThisPandaBehaviour;
			}
			else
			{
				LinkTo = PandaLinkerScope.AllPandaBehavioursInScene;
			}
		}

		public void Initialize()
		{
			
			if (this.gameObject.scene.name == null && (this.gameObject.hideFlags & HideFlags.NotEditable) == 0)
				return;

			if (LinkTo == PandaLinkerScope.AllPandaBehavioursInScene)
			{
				if (!_sceneLinkers.Contains(this))
					_sceneLinkers.Add(this);
				_sceneLinkersArray = null;
			}

		}

	}

	public enum PandaLinkerScope
	{
		ThisPandaBehaviour,
		AllPandaBehavioursInScene
	}
}
