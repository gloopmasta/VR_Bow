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

using System.Collections.Generic;
using System.Security.Cryptography;

namespace PandaBT.Compilation
{
    public class BTSourceString : BTSource
    {
        string _text;
        string _url;
        public BTSourceString(string text)
        {
            _text = text;
        }

        public BTSourceString(string text, string url)
        {
            _text = text;
            _url = url;
        }

        public override string source
        {
            get
            {
                return _text;
            }
        }

        public override string url
        {
            get
            {
                if (_url != null)
                    return _url;
                else
                    return GetHash();
            }
        }

        static SHA256Managed sha1 = null;
        string GetHash()
        {
            List<byte[]> hashes = new List<byte[]>();

            if (sha1 == null)
                sha1 = new SHA256Managed();

            // convert string to a byte[]
            byte[] bytes = new byte[_text.Length * sizeof(char)];
            System.Buffer.BlockCopy(_text.ToCharArray(), 0, bytes, 0, bytes.Length);

            var hashBytes = sha1.ComputeHash(bytes);
            hashes.Add(hashBytes);

            return System.Text.Encoding.UTF8.GetString(hashBytes);
        }


    }
}
