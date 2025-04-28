using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PandaBT.Compilation
{
	public class Compiler
	{

		public static CompilationUnit Compile(BTSource source)
		{
			return Compile(source.source, source.url );
		}

		public static CompilationUnit Compile(string source, string path = null)
		{
			CompilationUnit cu = new CompilationUnit();
			cu.sourceHash = HashString(source);
			PBTTokenizer.filepath = path;
			cu.tokens = PBTTokenizer.Tokenize(source);
			PBTParser.ParseTokens(cu);
			return cu;
		}

		public static string Hash(string source)
		{
			string hash = null;
			hash = HashString(source);
			return hash;
		}

		static string HashString(string text, string salt = "")
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			// Uses SHA256 to create the hash
			using (var sha = new System.Security.Cryptography.SHA256Managed())
			{
				// Convert the string to a byte array first, to be processed
				byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
				byte[] hashBytes = sha.ComputeHash(textBytes);

				// Convert back to a string, removing the '-' that BitConverter adds
				string hash = BitConverter
					.ToString(hashBytes)
					.Replace("-", String.Empty);

				return hash;
			}
		}
	}
}
