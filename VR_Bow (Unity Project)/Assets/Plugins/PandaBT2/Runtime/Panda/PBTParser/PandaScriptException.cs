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

using System;
using System.Linq;
using System.Collections.Generic;

namespace PandaBT.Compilation
{

	public class PandaScriptException : Exception
	{
		PandaScriptExceptionData _data;

		public string filePath { get { return _data.filePath; } }
		public int lineNumber { get { return _data.lineNumber; } }

		public PandaScriptExceptionData data
		{
			get
			{
				return _data;
			}
		}


		public PandaScriptException(string message, string filepath, int lineNumber)
			: base(message)
		{
			this._data = new PandaScriptExceptionData()
			{
				message = message,
				filePath = filepath,
				lineNumber = lineNumber
			};
		}
		public PandaScriptException(PandaScriptExceptionData data )
		{
			this._data = data;
		}

		public override string Message
		{
			get
			{
				string str = null;
				if (data.lineNumber != -1)
					str = string.Format("{0} in file '{1}': line {2}", _data.message, _data.filePath, _data.lineNumber);
				else
					str = data.message;

				return str;
			}
		}

	}
}
