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
    [System.Xml.Serialization.XmlInclude(typeof(BTFallbackState))]
    [System.Xml.Serialization.XmlInclude(typeof(BTRandomState))]
    [System.Xml.Serialization.XmlInclude(typeof(BTRepeatState))]
    [System.Xml.Serialization.XmlInclude(typeof(BTRetryState))]
    [System.Xml.Serialization.XmlInclude(typeof(BTSequenceState))]
    [System.Xml.Serialization.XmlInclude(typeof(BTTaskState))]
    public class BTNodeState
    {
        public Status status { get; set; }
        public BTNodeState() // Required for serialization.
        {

        }
#if !PANDA_BT_FREE

        public BTNodeState(BTNode node)
        {
            status = node.m_status;
            previousStatus = node.m_previousStatus;
            nextReturnStatus = node.m_nextReturnStatus;
            lastTick = node.m_lastTick;
        }

  
        public Status previousStatus { get; set; }
        public Status nextReturnStatus { get; set; }
        public int lastTick { get; set; }
#else
        public BTNodeState(BTNode node)
        {
        }
#endif
    }
}
