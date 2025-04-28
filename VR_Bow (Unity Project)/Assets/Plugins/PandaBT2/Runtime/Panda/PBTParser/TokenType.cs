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


namespace PandaBT
{
		public enum TokenType
		{
			Word,
			If,
			IfElse,
			Coma,
			Comment,
			EOL,
			Fallback,
			Indent,
			Not,
			Mute,
			Parallel,
			Parenthesis_Open,
			Parenthesis_Closed,
			ParamParenthesis_Open,
			ParamParenthesis_Closed,
			ExpressionParenthesis_Open,
			ExpressionParenthesis_Closed,
			Race,
			Random,
			Shuffle,
			Tree,
			TreeReference,
			TreeIdentifier,
			TreeDefinition,
			TreeCall,
			Repeat,
			Retry,
			Sequence,
			Value,
			While,
			Break,
			VariableByValue,
			VariableByRef,
			SequenceOperator,
			FallbackOperator,
			ParallelOperator,
			RaceOperator,
			NotOperator,
			Before
		}
}
