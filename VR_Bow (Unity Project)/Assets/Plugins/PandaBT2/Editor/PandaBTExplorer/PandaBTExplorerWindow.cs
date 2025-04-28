using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using PandaBT.Explorer;

using System.Reflection;
using UnityEditor.UIElements;
using PandaBT;
using PandaBT.Runtime;
using PandaBT.Config;
using System.Text;
namespace PandaBT.BTEditor
{

	public class PandaBTExplorerWindow : EditorWindow
	{

		VisualElement typeListView => rootVisualElement.Q<VisualElement>("TypeListView");
		ToolbarSearchField searchField => rootVisualElement.Q<ToolbarSearchField>("SearchField");
		DropdownField scopeDropdown => rootVisualElement.Q<DropdownField>("ScopeDropdown");

		class BTItemVisualElement
		{

			BTBinder _binder;
			PandaBTMemberInfo _pandaBTMemberInfo;

			string _searchString = null;

			public string searchString
			{
				get
				{
					if (_searchString != null)
						return _searchString;

					var sb = new StringBuilder();
					if (binder != null)
					{
						sb.AppendLine(binder.name);
						sb.AppendLine(binder.memberInfo?.DeclaringType.Name);
						var component = binder.target as Component;
						if (component != null)
						{
							sb.AppendLine(component.GetType().Name);
							sb.AppendLine(component.gameObject.name);
						}
					}

					if (pandaBTMemberInfo != null)
					{
						sb.AppendLine(pandaBTMemberInfo.memberInfo?.Name);
						sb.AppendLine(pandaBTMemberInfo.memberInfo?.DeclaringType.Name);
					}

					_searchString = sb.ToString().ToLower();
					return _searchString;
				}
			}

			public BTBinder binder => _binder;
			public PandaBTMemberInfo pandaBTMemberInfo => _pandaBTMemberInfo;


			public VisualElement visualElement;
			public string name;
			public GameObject gameObject;

			public BTItemVisualElement(PandaBTMemberInfo pMember)
			{
				_pandaBTMemberInfo = pMember;
				VisualElement visualElement = null;

				if (pMember.methodInfo != null)
					visualElement = CreateTaskMethodView(pMember.methodInfo);
				else
					visualElement = CreateNonMethodView(pMember);

				this.visualElement = visualElement;
				this.name = pMember.memberInfo.Name;
				this.gameObject = null;
			}

			public BTItemVisualElement(BTBinder btBinder)
			{
				_binder = btBinder;
				VisualElement visualElement = null;

				var taskBinder = btBinder as TaskBinder;

				if (taskBinder != null && taskBinder.methodInfo != null)
					visualElement = CreateTaskMethodView(taskBinder.methodInfo);
				else
					visualElement = CreateNonMethodView(btBinder);

				this.visualElement = visualElement;
				this.visualElement.style.flexDirection = FlexDirection.Row;
				this.name = btBinder.name;
				this.gameObject = (btBinder.target as Component)?.gameObject;

				if (gameObject != null)
				{
					var go = new ObjectField();
					go.objectType = go.GetType();
					go.value = gameObject;
					go.SetEnabled(false);

					var goContainer = new VisualElement();
					goContainer.name = "GameObject";
					goContainer.Add(go);

					this.visualElement.Add(goContainer);
					goContainer.SendToBack();

				}
			}

			private static VisualElement CreateNonMethodView(PandaBTMemberInfo pMember)
			{
				var label = new Label();
				var name = pMember.memberInfo.Name;
				string binderName = null;

				if (pMember.isVariable)
				{
					binderName = $"@{pMember.memberInfo.Name}";
				}
				else
				{
					binderName = $"{name}()";
				}
				label.text = $"{pMember.memberInfo.DeclaringType.Name}.{binderName}";
				var container = new VisualElement();
				container.name = "signature";
				container.Add(label);
				container.EnableInClassList("task", true);
				return container;
			}

			private static VisualElement CreateNonMethodView(BTBinder btBinder)
			{
				var taskBinder = btBinder as TaskBinder;
				var variableBinder = btBinder as VariableBinder;
				bool isVariable = variableBinder != null;

				var name = btBinder.name;

				VisualElement view = new Label();
				var component = btBinder.target as Component;
				if (isVariable)
				{
					var binderName = $"@{name}";
					view = GetParamView(binderName, variableBinder.variable.valueType);
				}
				else
				{
					view = NewLabel($"{taskBinder.name}", "task", "name");
				}

				var declaringTypeView = NewLabel($"{component.GetType().Name}.", "declaringType");

				var container = new VisualElement();
				container.style.flexDirection = FlexDirection.Row;

				var signature = new VisualElement();
				signature.style.flexDirection = FlexDirection.Row;
				signature.name = "signature";

				signature.Add(declaringTypeView);
				signature.Add(view);

				container.Add(signature);

				return container;
			}

		}

		BTItemVisualElement[] _BTItemVisualElements = null;

		struct ScopeDropOption
		{
			public string label;
			public Scope value;
		}

		ScopeDropOption[] _scopeDropOptions = new ScopeDropOption[]
		{
		new ScopeDropOption(){ label = "Project Assemblies", value = Scope.ProjectAssemblies },
		new ScopeDropOption(){ label = "PandaBTSettings (global)", value = Scope.Global },
		new ScopeDropOption(){ label = "Hierarchy", value = Scope.Hierarchy },
		new ScopeDropOption(){ label = "Selected GameObject(s) binding scope", value = Scope.SelectedGameObjectBindingScope },
		new ScopeDropOption(){ label = "Selected GameObject(s) only", value = Scope.SelectedGameObjectOnly },
		};

		Filter _filter = new Filter();

		[MenuItem("Window/Panda BT Explorer")]
		static void InitWindow()
		{
			// Get existing open window or if none, make a new one:
			var window = (PandaBTExplorerWindow)EditorWindow.GetWindow(typeof(PandaBTExplorerWindow));
			var title = new GUIContent("Panda BT explorer");
			title.text = "Panda BT Explorer";
			window.titleContent = title;
			window.Show();
			window._filter.scope = Scope.SelectedGameObjectOnly;
			window.Initialize();
		}

		bool _initialized = false;
		void Initialize()
		{
			if (searchField == null || scopeDropdown == null || typeListView == null)
				return;

			PopulateScopeDropDown();
			RefreshScope();
			RegisterCallBacks();
			_initialized = true;
		}

		void PopulateScopeDropDown()
		{
			var choices = _scopeDropOptions.Select(o => o.label).ToList();
			scopeDropdown.choices = choices;
			var index = 0;
			for (int i = 0; i < _scopeDropOptions.Length; i++)
			{
				if (_scopeDropOptions[i].value == _filter.scope)
				{
					index = i;
					break;
				}
			}
			scopeDropdown.index = index;
		}

		private static IEnumerable<BTItemVisualElement> GetAssembliesBTItemVisualElements()
		{
			var pandaMemberInfos = PandaBTExplorerDatabase.memberInfos;
			var memberDisplayInfos = pandaMemberInfos.Select(pMember => new BTItemVisualElement(pMember));
			return memberDisplayInfos;
		}

		private static IEnumerable<BTItemVisualElement> GetBTItemVisualElements(IEnumerable<GameObject> gameObjects, bool resolvePandaLinker = true)
		{
			List<BTItemVisualElement> btItems = new List<BTItemVisualElement>();
			foreach (var go in gameObjects)
			{
				var bi = BinderUtils.GetBindingInfo(go, resolvePandaLinker: resolvePandaLinker);
				foreach (var binder in bi.binders)
				{
					var visualElement = new BTItemVisualElement(binder);
					btItems.Add(visualElement);
				}
			}
			return btItems;
		}

		private void RefreshScope()
		{
			IEnumerable<GameObject> gameObjects = null;
			switch (_filter.scope)
			{
				case Scope.ProjectAssemblies:
					_BTItemVisualElements = GetAssembliesBTItemVisualElements().ToArray();
					break;
				case Scope.Global:
					gameObjects = PandaBTSettings.instance.gameObject.GetComponentsInChildren<Transform>(includeInactive: true)
					.Select(t => t.gameObject);
					_BTItemVisualElements = GetBTItemVisualElements(gameObjects, resolvePandaLinker: false).ToArray();
					break;
				case Scope.Hierarchy:
					var allGameObjects = Object.FindObjectsOfType<GameObject>(includeInactive: true);
					_BTItemVisualElements = GetBTItemVisualElements(allGameObjects, resolvePandaLinker: false).ToArray();
					break;
				case Scope.ScenePandaLinkers:
					break;
				case Scope.SelectedGameObjectBindingScope:
					gameObjects = Selection.gameObjects
						.SelectMany(go => go.GetComponentsInChildren<Transform>(includeInactive: true))
						.Select(t => t.gameObject);
					_BTItemVisualElements = GetBTItemVisualElements(gameObjects, resolvePandaLinker: true).ToArray();
					break;
				case Scope.SelectedGameObjectOnly:
					gameObjects = Selection.gameObjects
						.SelectMany(go => go.GetComponentsInChildren<Transform>(includeInactive: true))
						.Select(t => t.gameObject);
					_BTItemVisualElements = GetBTItemVisualElements(gameObjects, resolvePandaLinker: false).ToArray();
					break;
				default:
					break;
			}

			var listView = this.typeListView;
			listView.Clear();
			foreach (var mdi in _BTItemVisualElements)
			{
				listView.hierarchy.Add(mdi.visualElement);
				// mdi.visualElement.RegisterCallback<ClickEvent>((ClickEvent evt) => PandaBTExplorerWindow.OnBindableClick(mdi));
				var gameObjectField = mdi.visualElement.Q<VisualElement>("GameObject");
				if (gameObjectField != null)
				{
					gameObjectField.RegisterCallback<ClickEvent>((ClickEvent evt) => PandaBTExplorerWindow.OnGameObjectClick(mdi.gameObject));
				}

				var signature = mdi.visualElement.Q<VisualElement>("signature");
				if (signature != null)
				{
					signature.RegisterCallback<ClickEvent>((evt) =>
						{
							if (evt.clickCount == 2)
							{
								PandaBTExplorerWindow.OnSignatureClick(mdi);
							}
						}
					);
				}

			}
		}

		private void OnInspectorUpdate()
		{
			InitializeIfNot();
		}

		private void InitializeIfNot()
		{
			if (!_initialized)
			{
				Initialize();
			}
		}

		private void OnDestroy()
		{
			_callBackRegistered = false;
			Debug.Log("OnDestoy");
		}

		private void OnSelectionChange()
		{
			if (_filter.scope == Scope.SelectedGameObjectOnly || _filter.scope == Scope.SelectedGameObjectBindingScope)
			{
				RefreshScope();
				Filter();
			}
		}

		bool _callBackRegistered = false;
		void RegisterCallBacks()
		{
			if (_callBackRegistered)
				return;

			var window = this;
			EventCallback<ChangeEvent<string>> onSearchFilterChanged = (ChangeEvent<string> evt) =>
			{
				_filter.searchString = evt.newValue;

				window.Filter();
			};
			window.searchField.RegisterValueChangedCallback(onSearchFilterChanged);

			EventCallback<ChangeEvent<string>> onScopeDropdownValueChange = (ChangeEvent<string> evt) => window.OnScopeDropdownValueChange();

			window.scopeDropdown.RegisterValueChangedCallback(onScopeDropdownValueChange);


			window.rootVisualElement.RegisterCallback<DetachFromPanelEvent>((evt) =>
			{
				_initialized = false;
				_callBackRegistered = false;
			});

			_callBackRegistered = true;
		}

		void OnScopeDropdownValueChange()
		{
			var i = scopeDropdown.index;
			_filter.scope = _scopeDropOptions[i].value;
			RefreshScope();
			Filter();
		}

		public void Filter()
		{

			if (_BTItemVisualElements == null)
				RefreshScope();

			foreach (var bindableRowInfo in _BTItemVisualElements)
			{
				bool isMatch = false;

				if (string.IsNullOrEmpty(_filter.searchString))
					isMatch = true;
				else if (_filter.MatchesSearchString(bindableRowInfo.searchString))
					isMatch = true;

				bindableRowInfo.visualElement.style.display = isMatch ? DisplayStyle.Flex : DisplayStyle.None;
			}

		}

		public void CreateGUI()
		{
			var path = AssetDatabase.GUIDToAssetPath("54ee3e31b12e70149871fe3f61494b64");
			var visualTree = AssetDatabase.LoadAssetAtPath(path, typeof(VisualTreeAsset)) as VisualTreeAsset;
			var windowContent = visualTree.Instantiate();
			rootVisualElement.Add(windowContent);
		}

		static VisualElement CreateTaskMethodView(MethodInfo methodInfo)
		{
			var taskView = new VisualElement();

			var signature = new VisualElement();
			signature.style.flexDirection = FlexDirection.Row;
			signature.name = "signature";

			signature.EnableInClassList("task", true);
			signature.Add(NewLabel($"{methodInfo.DeclaringType.Name}.", "declaringType"));


			signature.Add(NewLabel(methodInfo.Name, "name"));
			signature.Add(NewLabel("("));
			var parameters = methodInfo.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				var isLast = i == parameters.Length - 1;
				var param = parameters[i];
				VisualElement paramView = GetParamView(param.Name, param.ParameterType);
				signature.Add(paramView);
				if (!isLast)
					signature.Add(NewLabel(","));
			}
			signature.Add(NewLabel(")"));

			taskView.Add(signature);

			return taskView;
		}

		private static VisualElement GetParamView(string name, System.Type type)
		{
			var variableContainer = new VisualElement();
			variableContainer.style.flexDirection = FlexDirection.Row;
			variableContainer.Add(NewLabel(type.Name, "type"));
			variableContainer.Add(NewLabel(name, "name", "param"));
			return variableContainer;
		}

		static Label NewLabel(string text, params string[] classNames)
		{
			var lbl = new Label();
			lbl.text = text;

			foreach (var classname in classNames)
			{
				lbl.EnableInClassList(classname, true);
			}
			return lbl;
		}

		static void OnSignatureClick(BTItemVisualElement binderDisplays)
		{
#if PANDA_BT_WITH_CECIL
			string path = null;
			int lineNumber = -1;
			CecilUtils.GetMemberSourceAssetPath(binderDisplays.pandaBTMemberInfo.memberInfo, out path, out lineNumber);
			if (path != null)
			{
				var text = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
				if (text != null)
					UnityEditor.AssetDatabase.OpenAsset(text, lineNumber);
			}
#endif
		}

		static void OnGameObjectClick(GameObject gameObject)
		{
			UnityEditor.EditorGUIUtility.PingObject(gameObject);
		}

		static void CopyToClipboard(BTItemVisualElement pandaBinderDisplay)
		{
			var text = pandaBinderDisplay.ToString();
			GUIUtility.systemCopyBuffer = text;
			Debug.Log($"copy to clipboard: {text}");
		}

	}
}
