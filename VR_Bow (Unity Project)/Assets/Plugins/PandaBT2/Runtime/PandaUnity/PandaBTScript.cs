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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using PandaBT.Compilation;
namespace PandaBT
{
	public class PandaBTScript : ScriptableObject
	{
		[SerializeField] public string source;
		[SerializeField] public string path;

		[SerializeField] public CompilationUnit compilationUnit;

		static public PandaBTScript Compile(string source, string path = null)
		{
			PandaBTScript pbt = CreateInstance<PandaBTScript>();
			pbt.source = source;
			pbt.path = path;
			pbt.compilationUnit = Compiler.Compile(source, path);
			return pbt;
		}
	}
}
