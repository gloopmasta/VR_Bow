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
using System.Globalization;

namespace PandaBT.Compilation
{
		[Serializable]
		public class Token
		{
			public TokenType type;
			public TokenValueType valueType = TokenValueType.None;
			public int substring_start;
			public int substring_length;
			public string substring;
			public int line;

			public Token(TokenType type, int start, int length, string source, int line, TokenValueType valueType)
			{
				this.type = type;
				this.substring_start = start;
				this.substring_length = length;
				this.line = line;
				this.valueType = valueType;
				_parsedParameter = null;
				this.substring =  source.Substring(substring_start, substring_length);
			}

			public Token(TokenType type, int start, int length, string source, int line)
				: this( type, start, length, source, line, TokenValueType.None)
			{
			}

			public Token()
			{
			}
			

			public override string ToString()
			{
				string str = "";
				if (type == TokenType.Word)
					str = substring;
				else if (type == TokenType.EOL)
					str = "[EOL]\n";
				else if (type == TokenType.Value || type == TokenType.VariableByValue)
					str = PBTTokenizer.ParseParameter(this).ToString();
				else
					str = string.Format("[{0}]", type.ToString());
				return str;
			}

			object _parsedParameter;
			public object parsedParameter
			{
				get
				{
					if( _parsedParameter == null)
					{
						var str = this.substring.Trim();

						switch( this.valueType )
						{
							case TokenValueType.Float:
								_parsedParameter = float.Parse(str, CultureInfo.InvariantCulture.NumberFormat);
								break;
							case TokenValueType.Integer:
								_parsedParameter = int.Parse(str, CultureInfo.InvariantCulture.NumberFormat);
								break;
							case TokenValueType.Boolean:
								_parsedParameter = str == "true";
								break;
							case TokenValueType.Null:
								_parsedParameter = null;
								break;
							case TokenValueType.String:
								_parsedParameter = str.Substring(1, str.Length - 2);
								break;
							case TokenValueType.Enum:
								_parsedParameter = new EnumParameter(str);
								break;
							case TokenValueType.VariableByValue:
							case TokenValueType.VariableByRef:
								_parsedParameter = new VariableParameter(str);
								break;
						}

					}
					return _parsedParameter;
				}
			}

		}

}

