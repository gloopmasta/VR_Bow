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

namespace PandaBT.BTEditor
{
	public static class PathUtils
	{
		/// <summary>
		/// Converts an absolute file path to a Unity asset path relative to the Assets folder.
		/// </summary>
		/// <param name="absolutePath">The absolute file system path.</param>
		/// <returns>The asset path relative to the Assets folder, or null if the path is invalid.</returns>
		public static string GetAssetPathFromAbsolutePath(string absolutePath)
		{
			// Get the absolute path to the Assets folder
			string assetsAbsolutePath = Application.dataPath;

			// Normalize the paths to use forward slashes and remove any relative segments
			absolutePath = Path.GetFullPath(absolutePath).Replace("\\", "/");
			assetsAbsolutePath = Path.GetFullPath(assetsAbsolutePath).Replace("\\", "/");

			if (absolutePath.StartsWith(assetsAbsolutePath))
			{
				// Get the relative path by trimming the Assets folder path
				string relativePath = "Assets" + absolutePath.Substring(assetsAbsolutePath.Length);
				return relativePath;
			}
			else
			{
				Debug.LogError("The provided path is not located within the Assets folder.");
				return null;
			}
		}
	}
}
