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

namespace PandaBT.Runtime
{
    [System.Serializable]
    public class BTRandomState : BTNodeState
    {
        public BTRandomState() : base()
        {

        }

        public BTRandomState(BTRandom node) : base(node)
        {
            var children = node.children;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == node.m_selectedChild)
                {
                    selectedChildIndex = i;
                    break;
                }
            }
        }

        public int selectedChildIndex { get; set; }
    }
}
