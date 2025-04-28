using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Collections;

using PandaBT.Runtime;

namespace PandaBT.BTEditor
{
	public class RefreshPandaBTInspectorOnImport : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
		{
			_ = Refresh();
		}

		static async Task Refresh()
		{
			await Task.Delay(300);
			SourceDisplay.refreshAllBehaviourTreeEditors();
		}

	}
}
