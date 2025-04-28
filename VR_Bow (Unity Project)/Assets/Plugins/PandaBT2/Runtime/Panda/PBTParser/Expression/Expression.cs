using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Compilation
{
	public class Expression
	{
		public Token[] tokens;
		public Expression[] children;

		public Token startToken;
		public Token endToken;

	}
}
