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
using System.IO;
#if UNITY_2020_3_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using PandaBT;
using PandaBT.Runtime;
using UnityEditor;

namespace PandaBT.Compilation
{
	[ScriptedImporter(1, "pbt")]
	public class PandaBTScriptImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			var source = File.ReadAllText(ctx.assetPath);
			var path = ctx.assetPath;
			var btScript = PandaBTScript.Compile(source, path);
			ctx.AddObjectToAsset("bt script", btScript);
			ctx.SetMainObject(btScript);

			// EditableBTScript.ParseAll();
			SourceDisplay.RefreshAllBehaviourTreeEditors();
			AssetDatabase.SaveAssetIfDirty(btScript);

		}
	}
}
