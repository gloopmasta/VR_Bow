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



// Requires com.unity.nuget.mono-cecil


#if !PANDA_BT_WITH_CECIL
namespace PandaBT.BTEditor
{
    using System;
    using System.Reflection;
    using System.Linq;
    public static class CecilUtils
    {
        public static void GetMemberSourceAssetPath(MemberInfo member, out string filepath, out int lineNumber)
        {
            GetMemberSourceLocation(member, out filepath, out lineNumber);
            if( filepath != null)
                filepath = PathUtils.GetAssetPathFromAbsolutePath(filepath);
        }

        public static void GetMemberSourceLocation(MemberInfo member, out string fileName, out int lineNumber)
        {
            throw new System.Exception("The `com.unity.nuget.mono-cecil` is requiered to use this method.");
        }
    }
}
#endif

#if PANDA_BT_WITH_CECIL
using System;
using System.Reflection;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PandaBT.BTEditor
{
	public static class CecilUtils
	{

		/// <summary>
		/// Returns the source file name and line number where the given member is defined.
		/// </summary>
		/// <param name="member">The MemberInfo of the member to locate.</param>
		/// <param name="filepath">Outputs the source file name.</param>
		/// <param name="lineNumber">Outputs the line number in the source file.</param>
		public static void GetMemberSourceAssetPath(MemberInfo member, out string filepath, out int lineNumber)
		{
			GetMemberSourceLocation(member, out filepath, out lineNumber);
			if (filepath != null)
				filepath = PathUtils.GetAssetPathFromAbsolutePath(filepath);
		}


		/// <summary>
		/// Returns the source file name and line number where the given member is defined.
		/// </summary>
		/// <param name="member">The MemberInfo of the member to locate.</param>
		/// <param name="filepath">Outputs the source file name.</param>
		/// <param name="lineNumber">Outputs the line number in the source file.</param>
		public static void GetMemberSourceLocation(MemberInfo member, out string filepath, out int lineNumber)
		{
			filepath = null;
			lineNumber = -1;

			if (member == null)
				throw new ArgumentNullException(nameof(member));

			// Get the path to the assembly containing the member
			string assemblyPath = member.Module.FullyQualifiedName;

			// Prepare to read the assembly with debug symbols
			var readerParameters = new ReaderParameters { ReadSymbols = true };

			ModuleDefinition moduleDefinition = null;

			try
			{
				moduleDefinition = ModuleDefinition.ReadModule(assemblyPath, readerParameters);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"Error reading assembly: {ex.Message}");
				return;
			}

			// Find the type definition corresponding to the member's declaring type
			var declaringType = member.DeclaringType;

			if (declaringType == null)
			{
				UnityEngine.Debug.LogError("Member does not have a declaring type.");
				return;
			}

			var typeDefinition = moduleDefinition.GetType(declaringType.FullName);

			if (typeDefinition == null)
			{
				UnityEngine.Debug.LogError($"Type {declaringType.FullName} not found in module.");
				return;
			}

			if (member is MethodInfo methodInfo)
			{
				// Handle method
				var methodDefinition = FindMethodDefinition(typeDefinition, methodInfo);

				if (methodDefinition == null)
				{
					UnityEngine.Debug.LogError($"Method {methodInfo.Name} not found in type {typeDefinition.FullName}.");
					return;
				}

				// Get debug information from the method definition
				GetDebugInfo(methodDefinition.DebugInformation, out filepath, out lineNumber);
			}
			else if (member is FieldInfo fieldInfo)
			{
				// Handle field
				var fieldDefinition = typeDefinition.Fields.FirstOrDefault(f => f.Name == fieldInfo.Name);

				if (fieldDefinition == null)
				{
					UnityEngine.Debug.LogError($"Field {fieldInfo.Name} not found in type {typeDefinition.FullName}.");
					return;
				}

				// Fields typically don't have sequence points, so get the file name from the type
				filepath = GetTypeSourceFilepath(typeDefinition);

				// And search the text file for the field using regex
				GetFieldSourceLocation(fieldInfo, filepath, out lineNumber);

				if (filepath == null)
				{
					UnityEngine.Debug.LogError($"Could not determine source file for field {fieldInfo.Name}.");
					return;
				}
			}
			else if (member is PropertyInfo propertyInfo)
			{
				// Handle property
				var propertyDefinition = typeDefinition.Properties.FirstOrDefault(p => p.Name == propertyInfo.Name);

				if (propertyDefinition == null)
				{
					UnityEngine.Debug.LogError($"Property {propertyInfo.Name} not found in type {typeDefinition.FullName}.");
					return;
				}

				// Try to get debug info from getter or setter methods
				MethodDefinition accessorMethod = propertyDefinition.GetMethod ?? propertyDefinition.SetMethod;

				if (accessorMethod == null)
				{
					UnityEngine.Debug.LogError($"No accessor methods found for property {propertyInfo.Name}.");
					return;
				}

				GetDebugInfo(accessorMethod.DebugInformation, out filepath, out lineNumber);
			}
			else
			{
				UnityEngine.Debug.LogError($"Member type {member.MemberType} is not supported.");
				return;
			}
		}


		/// <summary>
		/// Retrieves the source file path and line number where the given field is defined.
		/// </summary>
		/// <param name="fieldInfo">The FieldInfo object representing the field.</param>
		/// <returns>A tuple containing the file path and line number, or null if not found.</returns>
		public static void GetFieldSourceLocation(FieldInfo fieldInfo, string filepath, out int lineNumber)
		{
			lineNumber = -1;
			if (fieldInfo == null)
			{
				Debug.LogError("FieldInfo provided is null.");
				return;
			}



			try
			{
				// Read the script file

				// Construct a regex pattern to find the field declaration
				// This pattern matches common field declarations, considering access modifiers, static, readonly, etc.
				string fieldName = fieldInfo.Name;


				// Construct a regex pattern to find the field declaration
				// This pattern matches common field declarations, considering access modifiers, static, readonly, etc.
				string pattern = $@".*\b{Regex.Escape(fieldName)}\b";
				Regex regex = new Regex(pattern);

				using (StreamReader sr = File.OpenText(filepath))
				{
					string line = String.Empty;
					int i = 0;
					while ((line = sr.ReadLine()) != null)
					{
						i++;
						if (regex.IsMatch(line))
						{
							lineNumber = i;
							return;
						}
					}
				}

				return;
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error retrieving field source location: {ex.Message}");
				return;
			}
		}


		private static void GetDebugInfo(MethodDebugInformation debugInfo, out string fileName, out int lineNumber)
		{
			fileName = null;
			lineNumber = -1;

			if (debugInfo == null || debugInfo.SequencePoints == null || debugInfo.SequencePoints.Count == 0)
			{
				UnityEngine.Debug.LogError("No debug information found for member.");
				return;
			}

			// Find the first valid sequence point
			foreach (var sequencePoint in debugInfo.SequencePoints)
			{
				if (sequencePoint.Document != null &&
					sequencePoint.StartLine != -1 &&
					sequencePoint.StartLine != 0xFEEFEE)
				{
					fileName = sequencePoint.Document.Url;
					lineNumber = sequencePoint.StartLine;
					return;
				}
			}

			UnityEngine.Debug.LogError("No valid sequence points found for member.");
		}

		private static MethodDefinition FindMethodDefinition(TypeDefinition typeDefinition, MethodInfo methodInfo)
		{
			foreach (var methodDefinition in typeDefinition.Methods)
			{
				if (methodDefinition.Name != methodInfo.Name)
					continue;

				if (MethodsHaveSameSignature(methodDefinition, methodInfo))
					return methodDefinition;
			}

			return null;
		}

		private static bool MethodsHaveSameSignature(MethodDefinition methodDefinition, MethodInfo methodInfo)
		{
			// Compare parameter counts
			var parameters1 = methodDefinition.Parameters;
			var parameters2 = methodInfo.GetParameters();

			if (parameters1.Count != parameters2.Length)
				return false;

			// Compare parameter types
			for (int i = 0; i < parameters1.Count; i++)
			{
				if (parameters1[i].ParameterType.FullName != parameters2[i].ParameterType.FullName)
					return false;
			}

			// Compare return types
			if (methodDefinition.ReturnType.FullName != methodInfo.ReturnType.FullName)
				return false;

			return true;
		}

		private static string GetTypeSourceFilepath(TypeDefinition typeDefinition)
		{
			foreach (var methodDefinition in typeDefinition.Methods)
			{
				var debugInfo = methodDefinition.DebugInformation;

				if (debugInfo != null && debugInfo.SequencePoints != null && debugInfo.SequencePoints.Count > 0)
				{
					foreach (var sequencePoint in debugInfo.SequencePoints)
					{
						if (sequencePoint.Document != null &&
							sequencePoint.StartLine != -1 &&
							sequencePoint.StartLine != 0xFEEFEE)
						{
							return sequencePoint.Document.Url;
						}
					}
				}
			}

			return null;
		}
	}
}
#endif
