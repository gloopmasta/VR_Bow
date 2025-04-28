using PandaBT.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor.Compilation;
using UnityEngine;


using PandaBT;
using UnityEditor;

namespace PandaBT.Explorer
{
	public class PandaBTExplorerDatabase
	{

		public static PandaBTMemberInfo[] memberInfos
		{
			get
			{
				if (_pandaMemberInfos.Count == 0)
				{
					Load();
				}

				return _pandaMemberInfos.ToArray();
			}
		}

		static List<PandaBTMemberInfo> _pandaMemberInfos = new List<PandaBTMemberInfo>();

		static bool _isLoading = false;
		public static void Load()
		{

			if (_isLoading)
				return;

			/*
			var assemblies = CompilationPipeline.GetAssemblies()
				.Select(assembly => assembly.assemblyReferences[0]);
			*/
			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			// System.AppDomain.currentDomain.GetAssemblies()

			_pandaMemberInfos.Clear();
			var types = assemblies
			.SelectMany(assembly => assembly.GetTypes())
			.Where(t => typeof(MonoBehaviour).IsAssignableFrom(t))
			;
			int n = types.Count();
			int i = 0;
			foreach (var type in types)
			{
				float progress = System.Convert.ToSingle(i) / System.Convert.ToSingle(n);

				EditorUtility.DisplayProgressBar($"Scanning assemblies for PandaBT bindables ({i}/{n})...", $"{type.Name}", progress);
				var pandaMemberInfos = PandaBTReflectionDatabase.GetMemberInfos(type);
				_pandaMemberInfos.AddRange(pandaMemberInfos);

				i++;
			}
			EditorUtility.ClearProgressBar();

		}

		public static IEnumerable<GameObject> selectedObjectsInScene
		{
			get
			{
				return Selection.gameObjects.Where(go => go.activeInHierarchy);
			}
		}
	}

	public enum Scope
	{
		ProjectAssemblies,
		Global,
		Hierarchy,
		ScenePandaLinkers,
		SelectedGameObjectBindingScope,
		SelectedGameObjectOnly
	}

	[Serializable]
	public class Filter
	{
		public Scope scope;
		public string searchString;


		public string _searchString;
		string[] _terms;
		public string[] terms
		{
			get
			{
				if( _terms == null || _searchString != searchString.ToLower())
				{
					_searchString = searchString.ToLower();
					_terms = _searchString.Split(' ').Select( t => t.Trim() ).ToArray();
				}

				return _terms;
			}
		}

		public bool MatchesSearchString(string str)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			bool isMatch = true;
			foreach(string term in terms)
			{
				if( !str.Contains(term))
				{
					isMatch = false;
					break;
				}
			}
			return isMatch;
		}
	}
}
