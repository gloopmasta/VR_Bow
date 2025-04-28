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

using System.Collections;
using System.Collections.Generic;

namespace PandaBT.Runtime
{
	public class CodeMap
	{
		public BTNode[] nodes
		{
			get
			{
				return _nodes;
			}
		}

		public SubstringLocation[] substringLocations
		{
			get
			{
				return _locations;
			}
		}

		BTNode[] _nodes;
		SubstringLocation[] _locations;

		internal CodeMap( Dictionary<BTNode, SubstringLocation> codemap )
		{

			var nodeList = new List<BTNode>();
			var locList = new List<SubstringLocation>();

			nodeList.AddRange(codemap.Keys);
			nodeList.Sort((a, b) => codemap[a].start.CompareTo(codemap[b].start));

			_nodes = nodeList.ToArray();

			foreach(var n in _nodes)
			{
				locList.Add(codemap[n]);
			}

			_locations = locList.ToArray();

			
		}
	}

}
