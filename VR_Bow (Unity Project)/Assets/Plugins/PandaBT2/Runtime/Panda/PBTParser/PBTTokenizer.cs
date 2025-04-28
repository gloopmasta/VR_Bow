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
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PandaBT.Compilation
{

	internal struct Block
	{
		public SubstringLocation loc;
		public string substring;

		public Block(int line, int pos, int length, string src)
		{
			loc.line = line;
			loc.start = pos;
			loc.length = length;
			substring = src.Substring(pos, length);
		}
	}

	internal class PBTScanner
	{
		public static int indentSize = 4;
		private static char[] singleCharSymbols = new char []{',', '\t', '(', ')'};
		
		public static int ScanSingleCharSymbol(int i, string src)
		{
			int len = -1;
			var c = src[i];
			if( singleCharSymbols.Contains(c))
				len = 1;
			return len;
		}

		public static int ScanSpaceIndent(int i, string src)
		{
			int len = indentSize;
			for(int j = 0; j < indentSize; j++)
			{
				if(i + j < src.Length )
				{
					char c =  src[i+j];
					if( c != ' ')
					{
						len = -1;
						break;
					}
				}
				else if( len == 0 )
				{
					len = -1;
				}
			}
			return len;
		}

		public static int ScanSpaces(int i, string src)
		{
			int len = -1;
			if ( src[i] == ' ' )
			{
				len = 0;
				while (i + len + 1 < src.Length && src[i + len + 1] == ' ')
				{
					len++;
				}

				len++;
			}
			return len;
		}

		public static int ScanEOL(int i, string src)
		{
			int len = -1;
			if (src[i] == '\n')
			{
				len = 1;
			} else
			if (i + 1 < src.Length && src[i] == '\r' && src[i+1] == '\n')
			{
				len = 2;
			}
			return len;
		}

		public static int ScanEndOfBlockComment(int i, string src)
		{
			int len = -1;
			if (i + 1 < src.Length && src[i] == '*' && src[i + 1] == '/')
			{
				len = 2;
			}
			return len;
		}

		public static int ScanStringValue(int i, string src)
		{
			int len = -1;
			if( src[i] == '\"')
			{
				len = 0;
				while( i + len + 1 < src.Length && src[i+len+1] != '\"' && ScanEOL(i+len+1, src) == -1)
				{
					len++;
				}
				len++;
				if (src[i + len] == '\"')
					len++;
			}
			return len;
		}

		public static int ScanInlineComment(int i, string src)
		{
			int len = -1;
			if ( i + 1 < src.Length && src[i] == '/' && src[i+1] == '/')
			{
				len = 2;
				while (i + len + 1 < src.Length && ScanEOL(i + len + 1, src) == -1)
				{
					len++;
				}
				len++;
			}
			return len;
		}

		public static int ScanBlockComment(int i, string src)
		{
			int len = -1;
			if (i + 1 < src.Length && src[i] == '/' && src[i + 1] == '*')
			{
				len = 2;
				while (i + len + 1 < src.Length && ScanEndOfBlockComment(i + len, src) == -1)
				{
					len++;
				}
				var eocLen = ScanEndOfBlockComment(i + len, src);
				if (eocLen != -1)
				{
					len += eocLen;
				}
			}
			return len;
		}

		public static int ScanWord(int i, string src)
		{
			int len = -1;
			char c = src[i];
			if (char.IsLetter(c) || c == '@' || c == '&' || c == '_' || c == '#' || c == '.' )
			{
				len = 0;
				while ( i + len + 1 < src.Length && ( char.IsLetterOrDigit(src[i + len + 1]) || src[i + len + 1] == '_' || src[i + len + 1] == '.'))
				{
					len++; 
				}
				len++;
			}

			if (c == '&' && len == 1)
				len = -1;

			return len;
		}

		static readonly string[] operators =  new string[] {"&&", "||", "&", "|", "!"};
		public static int ScanOperator(int i, string src)
		{
			int len = -1;
			for(int j = 0; j < operators.Length; j++)
			{
				len = ScanEquals(i, src, operators[j]);
				if (len != -1)
					break;
			}
			return len;
		}

		public static int ScanEquals(int i, string src, string str)
		{
			int len = -1;
			if( i + str.Length <= src.Length)
			{
				bool matches = true;
				for (int j = 0; j < str.Length; j++)
				{
					if( src[i+j] != str[j])
					{
						matches = false;
						break;
					}
				}

				if (matches)
					len = str.Length;
			}
			return len;
		}



		public static int ScanNumber(int i, string src)
		{
			int len = -1;
			if( char.IsDigit( src[i]) || src[i] == '-' )
			{
				len = 0;
				while( i + len + 1 < src.Length && ( char.IsNumber(src[i + len + 1]) || src[i + len + 1] == '.' || src[i + len + 1] == '-') )
				{
					len++;
				}
				len++;
			}
			return len;
		}
		
	}


	public class PBTTokenizer 
	{
		public static string filepath= "";

		public static string CleanBlanks( string source)
		{
			string src = source;

			// Do some source clean up
			src = src.Replace("\r\n", "\n"); // Let's process only one kind of EOL.
			src = src.Replace("\r", "\n");

			var sb = new StringBuilder ();

			bool isIdenting = true;
			int spaceCount = 0;
			foreach (var c in src) 
			{
				if(c == '\n')
				{
					isIdenting = true;
					spaceCount = 0;
					sb.Append( c );
					continue;
				}

				if( c == ' ' )
					spaceCount++;
				else
					isIdenting = false;

				if( isIdenting  )
				{
					if( spaceCount == 2)
					{
						sb.Append('\t');
						spaceCount = 0;
					}
				}
				else
				{
					sb.Append( c );
				}
			}
			
			return sb.ToString();
		}

		private static Block[] SplitByBlocks(string source)
		{
			var blocks = new List<Block>();
			int i = 0;
			int line = 1;
			while ( i < source.Length)
			{
				int len = -1;

				if (len == -1){
					len = PBTScanner.ScanEOL(i, source);
					if (len != -1) line++;
				}

				if (len == -1) {
					len = PBTScanner.ScanInlineComment(i, source);
				}

				if (len == -1)
				{
					len = PBTScanner.ScanBlockComment( i, source);
				}

				if (len == -1) len = PBTScanner.ScanSpaceIndent(i, source);
				if (len == -1) len = PBTScanner.ScanSingleCharSymbol(i, source);
				if (len == -1) len = PBTScanner.ScanSpaces(i, source);
				if (len == -1) len = PBTScanner.ScanNumber(i, source);
				if (len == -1) len = PBTScanner.ScanStringValue(i, source);
				if (len == -1) len = PBTScanner.ScanWord(i, source);
				if (len == -1) len = PBTScanner.ScanOperator(i, source);

				if ( len != -1)
				{
					blocks.Add(new Block(line, i, len, source));
				}
				else
				{
					// unknown char
					len = 1;
					blocks.Add(new Block(line, i, len, source));
					break;
				}

				i += len;
			}

			return blocks.ToArray();
		}

		private static string currentSource = null;

		public static Token[] Tokenize(string source)
		{
			currentSource = source;
			var blocks = SplitByBlocks(source);
			List<Token> tokens = new List<Token>();

			foreach (var block in blocks)
			{
				Token token = null;

				if (token == null) token = ParseTreeIdentifier(block);
				if (token == null) token = ParseString(block);
				if (token == null) token = ParseInlineComment(block);
				if (token == null) token = ParseBlockComment(block);
				if (token == null) token = ParseEOL(block);
				if (token == null) token = ParseSingleCharSymbol(block);
				if (token == null) token = ParseSpaceIndent(block);
				if (token == null) token = ParseStructuralNode(block);
				if (token == null) token = ParseValue(block);
				if (token == null) token = ParseOperator(block);

				if (token != null)
					tokens.Add(token);
			}

			QualifyParentheses(tokens);

			return tokens.ToArray();
		}

		private static void QualifyParentheses(List<Token> tokens)
		{
			bool previousWasWord = false;
			bool paramParenthesisOpened = false;
			for (int i = 0; i < tokens.Count; i++)
			{
				Token token = tokens[i];

				// skip blank space
				if (token.type == TokenType.Indent)
					continue;

				if (token.type == TokenType.EOL)
				{
					previousWasWord = false;
					paramParenthesisOpened = false;
				}


				if (token.type == TokenType.Parenthesis_Open && previousWasWord)
				{
					token.type = TokenType.ParamParenthesis_Open;
					paramParenthesisOpened = true;
				}

				if (token.type == TokenType.Parenthesis_Closed && paramParenthesisOpened)
				{
					token.type = TokenType.ParamParenthesis_Closed;
					paramParenthesisOpened = false;
				}

				previousWasWord = token.type == TokenType.Word || IsStructuralWithParameters(token.type);
			}

			// The remaining parenthesis are part of expressions
			for (int i = 0; i < tokens.Count; i++)
			{
				Token token = tokens[i];
				if (token.type == TokenType.Parenthesis_Open)
					token.type = TokenType.ExpressionParenthesis_Open;

				if (token.type == TokenType.Parenthesis_Closed)
					token.type = TokenType.ExpressionParenthesis_Closed;

			}
		}

		private static readonly TokenType[] structuralsWithParameters = new TokenType[]{
			TokenType.Tree,
			TokenType.TreeDefinition,
			TokenType.TreeReference,
			TokenType.Random,
			TokenType.Shuffle,
			TokenType.Repeat,
			TokenType.Retry,
			TokenType.Break,
		};
		private static bool IsStructuralWithParameters(TokenType type)
		{
			var itIs = structuralsWithParameters.Contains(type);
			return itIs;
		}

		public static TokenValueType GetValueType(string content)
		{
			TokenValueType valueType = TokenValueType.None;
			string str = content.Trim();

			float f;
			int i;
			// (float.TryParse(comboCurrencyValue.SelectedItem.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,out currency)&& float.TryParse(txtYourValue.Text,out inputValue))
			if (str == "true" || str == "false") valueType = TokenValueType.Boolean;
			else if (str.Length > 1 && str == "null") valueType = TokenValueType.Null;
			else if (str.Length > 1 && str.StartsWith("@")) valueType = TokenValueType.VariableByValue;
			else if (str.Length > 1 && str.StartsWith("&")) valueType = TokenValueType.VariableByRef;
			else if (str.Contains(".") && float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out f)) valueType = TokenValueType.Float;
			else if (int.TryParse(str, out i)) valueType = TokenValueType.Integer;
			else if (str.StartsWith("\"") && str.EndsWith("\"")) valueType = TokenValueType.String;
			else if (str.Contains(".") && enumRegex.Match(str).Success) valueType = TokenValueType.Enum;

			return valueType;
		}

		public static string ToString(Token[] tokens)
		{
			var sb = new StringBuilder();
			foreach(var t in tokens)
			{
				sb.Append(t.ToString());
			}
			return sb.ToString();
		}

		public static string ToString(Token[][] tokenSets)
		{
			var sb = new StringBuilder();
			foreach (var s in tokenSets)
			{
				sb.Append(ToString(s));
				sb.Append("-----------------------------\n");
			}
			return sb.ToString();
		}

		public static object ParseParameter(Token token)
		{
			object o = null;
			o = token.parsedParameter;
			return o;
		}


		private static Token ParseString(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanStringValue(0, block.substring);
			if ( len != -1)
			{
				for(int j = 0; j < len; j++)
				{
					if( PBTScanner.ScanEOL(j, block.substring) != -1)
					{
						throw new PandaScriptException("Expected double-quotes before end-of-lines", PBTTokenizer.filepath, block.loc.line);
					}
				}
				token = NewToken(block, TokenType.Value, TokenValueType.String);
			}
			return token;
		}

		private static Token ParseInlineComment(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanInlineComment(0, block.substring);
			if (len != -1)
			{
				token = NewToken(block, TokenType.Comment);
			}
			return token;
		}

		private static Token ParseBlockComment(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanBlockComment(0, block.substring);
			if (len != -1)
			{
				token = NewToken(block, TokenType.Comment);
			}
			return token;
		}

		private static Token ParseTreeIdentifier(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanWord(0, block.substring);
			if (len > 0 )
			{
				if (block.substring.StartsWith('#'))
				{
					token = NewToken(block, TokenType.TreeIdentifier);
				}
			}
			return token;
		}

		private static Token ParseOperator(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanOperator(0, block.substring);
			if (len > 0)
			{
				TokenType type = TokenType.Comment;
				switch (block.substring)
				{
					case "&": type = TokenType.SequenceOperator; break;
					case "|": type = TokenType.FallbackOperator; break;
					case "&&": type = TokenType.ParallelOperator; break;
					case "||": type = TokenType.RaceOperator; break;
					case "!": type = TokenType.NotOperator; break;
					default: type = TokenType.Comment; break;
				}

				if( type != TokenType.Comment)
				{
					token = NewToken(block, type);
				}
			}
			return token;
		}



		private static Token ParseStructuralNode(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanWord(0, block.substring);
			if (len != -1)
			{
				string lc_word = block.substring.Trim().ToLower();
				var start = block.loc.start;
				var line = block.loc.line;
				var src = currentSource;
				if (lc_word.Trim() != "")
				{
					TokenType type = TokenType.Comment;
					switch (lc_word)
					{
						case "fallback": type = TokenType.Fallback; break;
						case "if": type = TokenType.If; break;
						case "ifelse": type = TokenType.IfElse; break;
						case "not": type = TokenType.Not; break;
						case "parallel": type = TokenType.Parallel; break;
						case "repeat": type = TokenType.Repeat; break;
						case "retry": type = TokenType.Retry; break;
						case "race": type = TokenType.Race; break;
						case "random": type = TokenType.Random; break;
						case "shuffle": type = TokenType.Shuffle; break;
						case "tree": type = TokenType.Tree; break;
						case "sequence": type = TokenType.Sequence; break;
						case "while": type = TokenType.While; break;
						case "mute": type = TokenType.Mute; break;
						case "break": type = TokenType.Break; break;
						case "before": type = TokenType.Before; break;
						default: token = null; break;
					}

					if( type != TokenType.Comment)
						token = NewToken(block, type);
				}
			}
			return token;
		}

		private static Token ParseValue(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanWord(0, block.substring);
			if( len == -1 )
				len = PBTScanner.ScanNumber(0, block.substring);

			if (len != -1)
			{
				string word = block.substring;
				if (word.Trim() != "")
				{
					var tokenType = GetValueType(word);
					if (tokenType == TokenValueType.VariableByValue)
						token = NewToken(block, TokenType.VariableByValue, tokenType);
					else if (tokenType == TokenValueType.VariableByRef)
						token = NewToken(block, TokenType.VariableByRef, tokenType);
					else if (tokenType != TokenValueType.None)
						token = NewToken(block, TokenType.Value, tokenType);
					else
						token = NewToken(block, TokenType.Word);
				}
			}
			return token;
		}

		private static Token ParseSingleCharSymbol(Block block)
		{
			Token token = null;

			if (block.loc.length != 1)
				return token;

			var c = block.substring[0];

			TokenType type = TokenType.Comment;
			switch (c)
			{
				case '\t':type = TokenType.Indent; break;
				case ',': type = TokenType.Coma; break;
				case '(': type = TokenType.Parenthesis_Open; break;
				case ')': type = TokenType.Parenthesis_Closed; break;
			}

			if ( type != TokenType.Comment)
				token = NewToken(block, type);
			return token;
		}

		private static Token ParseEOL(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanEOL(0, block.substring);
			if (len != -1)
			{
				token = NewToken(block, TokenType.EOL);
			}
			return token;
		}

		private static Token ParseSpaceIndent(Block block)
		{
			Token token = null;
			var len = PBTScanner.ScanSpaceIndent(0, block.substring);
			if (len != -1)
			{
				token = NewToken(block, TokenType.Indent);
			}
			return token;
		}

		private static Token NewToken(Block block, TokenType type, TokenValueType valueType = TokenValueType.None)
		{
			var token = new Token()
			{
				substring_start = block.loc.start,
				substring_length = block.loc.length,
				substring = block.substring,
				line = block.loc.line,
				type = type,
				valueType = valueType
			};
			return token;
		}



		static Regex enumRegex   = new Regex(@"^[a-zA-Z]+[a-zA-Z0-9_.]*\.[a-zA-Z0-9_.]+$");

	}
}
