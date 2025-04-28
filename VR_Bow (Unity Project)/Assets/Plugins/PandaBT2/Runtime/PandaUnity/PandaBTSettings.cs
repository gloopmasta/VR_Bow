using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace PandaBT.Config
{
	public class PandaBTSettings : MonoBehaviour
	{

		public bool HideInScene = true;

		private static bool _childrenEnabled = false;

		private void Awake()
		{
			_childrenEnabled = false;
		}

		private void OnValidate()
		{

			if (_instance == null)
				_instance = PandaBTSettings.FindInstance();

			if( _instance != null )
				_instance.HideInScene = HideInScene;

			RefreshHideFlags();
		}

		public static PandaBTSettings instance
		{
			get
			{
				if (_instance == null)
					_childrenEnabled = false;


				if (_instance == null)
				{
					_instance = PandaBTSettings.FindInstance();
					if( _instance != null)
					{
						RefreshHideFlags();
					}
				}

				if (_instance == null)
				{
					var name = "PandaBTSettings";
					var prefab = Resources.Load<GameObject>(name);
					prefab.SetActive(false);
					if (prefab == null)
					{
						throw new System.Exception($"Resources/{name} not found.");
					}
					var go = Instantiate(prefab);
					go.name = name;

					_instance = go.GetComponent<PandaBTSettings>();

					RefreshHideFlags();

				}

				if( _instance != null && _childrenEnabled == false )
				{
					EnableComponentsInChildren();
					_childrenEnabled = true;
				}

				return _instance;
			}
		}

		private static void RefreshHideFlags()
		{
			if (_instance == null)
				return;

			HideFlags hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

			if (_instance.HideInScene)
				hideFlags |= HideFlags.HideInHierarchy;

			foreach (Transform t in _instance.transform.GetComponentsInChildren<Transform>())
				t.gameObject.hideFlags = hideFlags;

		}

		private static void EnableComponentsInChildren()
		{
			var monoBehaviours = _instance.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
			var flag = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
			foreach (var monoBehaviour in monoBehaviours)
			{
				if (!monoBehaviour.enabled)
					continue;

				var OnEnableMethod = monoBehaviour.GetType().GetMethod("OnEnable", flag);
				if (OnEnableMethod != null)
					OnEnableMethod.Invoke(monoBehaviour, null);
			}
		}

		private static PandaBTSettings _instance = null;
		
		public void OnDestroy()
		{
			_instance = null;
		}

		private static PandaBTSettings FindInstance()
		{
			PandaBTSettings pbtSettings = null;
			var instances = Resources.FindObjectsOfTypeAll<PandaBTSettings>();
			foreach(var inst in instances)
			{
				bool isPrefab = false;
				isPrefab = (inst.hideFlags & HideFlags.NotEditable) == 0;
				if (!isPrefab)
					pbtSettings = inst;
			}

			return pbtSettings;
		}

		
	}
}

