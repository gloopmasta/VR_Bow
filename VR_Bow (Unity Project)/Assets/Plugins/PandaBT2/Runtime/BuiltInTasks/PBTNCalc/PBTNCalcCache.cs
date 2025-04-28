using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PANDA_BT_WITH_VISUAL_SCRIPTING
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
#endif

namespace PandaBT.BTEditor
{
	public static class PBTNCalcCache
	{

#if PANDA_BT_WITH_VISUAL_SCRIPTING
		static Dictionary<string, CachedExpression> _cache = new Dictionary<string, CachedExpression>();

		public static Expression Get(string key)
		{
			Expression expression = null;
			if (_cache.ContainsKey(key))
			{
				var cachedExpression = _cache[key];
				expression = cachedExpression.expression;
			}

			return expression;
		}

		public static IEnumerable<Expression> GetAll()
		{
			var keys = _cache.Keys;
			return keys.Select(key => _cache[key].expression);
		}

		public static void Cache(Expression expression, string key)
		{
			if (_cache.ContainsKey(key))
			{
				throw new System.Exception($"Expression `{key}` is already cached");
			}

			_cache[key] = new CachedExpression()
			{
				expression = expression,
				frame = Time.frameCount
			};
		}

		public static void Clear()
		{
			_cache.Clear();
		}

		public class CachedExpression
		{
			public Expression expression;
			public int frame;
		}
#endif
	}
}
