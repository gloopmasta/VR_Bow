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

using System.Text;
using System.Text.RegularExpressions;

namespace PandaBT.Compilation
{
	public class EnumParameter
	{
		public EnumParameter(string source)
		{
			if(enumRegex.IsMatch(source) )
			{
				var match = enumRegex.Match(source);
				this.enumType = match.Groups[1].ToString().Trim();
				this.enumValue = match.Groups[2].ToString().Trim(); 
			}
		}

		public string enumType;
		public string enumValue;

		public object Parse( System.Type parameterType)
		{
			return System.Enum.Parse(parameterType, enumValue);
		}

		public bool Matches( System.Type other )
		{
			bool itIs = false;
			var fullname = other.FullName.Replace("+", ".");
			itIs = fullname.EndsWith(this.enumType);
			return itIs;
		}
		static Regex enumRegex = new Regex(@"^(.*)\.([^.]+)+$");

	}
}
