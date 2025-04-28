using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using PandaBT.BTEditor;

#if PANDA_BT_WITH_VISUAL_SCRIPTING
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
#endif

using PandaBT;

namespace PandaBT.Runtime
{

	public class PBTNCalc : MonoBehaviour
	{

#if PANDA_BT_WITH_VISUAL_SCRIPTING
		List<EvaluateParameterHandler> _parameterHandlers = new List<EvaluateParameterHandler>();
		List<EvaluateFunctionHandler> _functionHandlers = new List<EvaluateFunctionHandler>();

		public void RegisterParameterHandler(EvaluateParameterHandler parameterHandler)
		{
			if (_parameterHandlers.Contains(parameterHandler) == false)
			{
				_parameterHandlers.Add(parameterHandler);
				foreach (var expression in PBTNCalcCache.GetAll())
					expression.EvaluateParameter += parameterHandler;
			}
		}

		public void UnregisterParameterHandler(EvaluateParameterHandler parameterHandler)
		{
			if (_parameterHandlers.Contains(parameterHandler) == true)
			{
				_parameterHandlers.Add(parameterHandler);
				foreach (var expression in PBTNCalcCache.GetAll())
					expression.EvaluateParameter -= parameterHandler;
			}
		}

		public void RegisterFunctionHandler(EvaluateFunctionHandler functionHandler)
		{
			if (_functionHandlers.Contains(functionHandler) == false)
			{
				_functionHandlers.Add(functionHandler);
				foreach (var expression in PBTNCalcCache.GetAll())
					expression.EvaluateFunction += functionHandler;
			}
		}

		public void UnregisterFunctionHandler(EvaluateFunctionHandler functionHandler)
		{
			if (_functionHandlers.Contains(functionHandler) == true)
			{
				_functionHandlers.Add(functionHandler);
				foreach (var expression in PBTNCalcCache.GetAll())
					expression.EvaluateFunction -= functionHandler;
			}
		}

		void OnEnable()
		{
			PBTNCalcCache.Clear();
			RegisterParameterHandler(btVariableParameterHandler);
			RegisterFunctionHandler(conversionFunctionHandler);
			RegisterFunctionHandler(v3FunctionHandler);
		}
#endif

		#region BT Tasks

		[PandaTask]
		void Eval(IVariable vresult, string expression)
		{
#if PANDA_BT_WITH_VISUAL_SCRIPTING
			var exp = GetExpression(expression);
			var result = exp.Evaluate(null);
			vresult.value = result;
			PandaTask.Succeed();
#else
		// throw new Exception("The Eval task requieres VisualScripting (com.unity.visualscripting)");
		PandaTask.Fail();
#endif
		}

		[PandaTask]
		void Eval(string expression)
		{
#if PANDA_BT_WITH_VISUAL_SCRIPTING
			var exp = GetExpression(expression);
			var result = exp.Evaluate(null);
			PandaTask.Complete(result.Equals(true));
#else
		PandaTask.Fail();
#endif
		}

		#endregion

#if PANDA_BT_WITH_VISUAL_SCRIPTING
		void btVariableParameterHandler(Flow f, string parameterName, ParameterArgs args)
		{
			var bindingInfo = PandaBehaviour.current.bindingInfo;
			var binder = bindingInfo.variableBinders.FirstOrDefault(v => v.variable.name == parameterName);

			if (binder == null)
			{
				throw new Exception($"No variable name {parameterName}");
			}

			args.Result = binder.variable.value;

		}

		void conversionFunctionHandler(Flow f, string fn, FunctionArgs args)
		{
			if (fn == "f")
			{
				var value = args.Parameters[0].Evaluate(f);
				var type = value.GetType();

				if (type == typeof(double))
					args.Result = System.Convert.ToSingle((double)value);

				if (type == typeof(int))
					args.Result = System.Convert.ToSingle((int)value);

				if (type == typeof(float))
					args.Result = (float)value;

			}

		}

		void v3FunctionHandler(Flow f, string fn, FunctionArgs args)
		{
			if (fn == "v3")
			{
				var x = (float)args.Parameters[0].Evaluate(f);
				var y = (float)args.Parameters[1].Evaluate(f);
				var z = (float)args.Parameters[2].Evaluate(f);
				args.Result = new Vector3(x, y, z);
			}

			if (fn == "v3x") args.Result = ((Vector3)args.Parameters[0].Evaluate(f)).x;
			if (fn == "v3y") args.Result = ((Vector3)args.Parameters[0].Evaluate(f)).y;
			if (fn == "v3z") args.Result = ((Vector3)args.Parameters[0].Evaluate(f)).z;
		}


		string PrepareExpression(string strExpression)
		{
			string prepared = null;
			string pattern = @"([0-9]*\.[0-9]*)";
			string replacement = @"f($1)";
			prepared = Regex.Replace(strExpression, pattern, replacement);
			return prepared;
		}

		Expression GetExpression(string strExpression)
		{

			var exp = PBTNCalcCache.Get(strExpression);

			if (exp != null)
				return exp;


			var prepared = PrepareExpression(strExpression);
			exp = new Expression(prepared);

			foreach (var parameterHandler in _parameterHandlers)
				exp.EvaluateParameter += parameterHandler;

			foreach (var functionHandler in _functionHandlers)
				exp.EvaluateFunction += functionHandler;

			PBTNCalcCache.Cache(exp, strExpression);

			return exp;
		}

#endif

	}
}
