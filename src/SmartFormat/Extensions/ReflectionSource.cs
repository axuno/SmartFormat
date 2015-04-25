﻿using System.Reflection;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
	public class ReflectionSource : ISource
	{
		public ReflectionSource(SmartFormatter formatter)
		{
			// Add some special info to the parser:
			formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
			formatter.Parser.AddAdditionalSelectorChars("_");
			formatter.Parser.AddOperators(".");
		}

		public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
		{
			var current = selectorInfo.CurrentValue;
			var selector = selectorInfo.SelectorText;

			if (current == null)
			{
				return false;
			}

			// REFLECTION:
			// Let's see if the argSelector is a Selectors/Field/ParseFormat:
			var sourceType = current.GetType();

			var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
			bindingFlags |= selectorInfo.FormatDetails.Settings.GetCaseSensitivityBindingFlag();

			var members = sourceType.GetMember(selector, bindingFlags);
			foreach (var member in members)
			{
				if (member is FieldInfo)
				{
					//  Selector is a Field; retrieve the value:
					var field = (FieldInfo)member;
					selectorInfo.Result = field.GetValue(current);
					return true;
				}

				var propertyInfo = member as PropertyInfo;
				MethodInfo methodInfo;

				if (propertyInfo != null)
				{
					if (propertyInfo.CanRead)
					{
						methodInfo = propertyInfo.GetGetMethod();
					}
					else
					{
						continue;
					}
				}
				else
				{
					methodInfo = member as MethodInfo;
				}

				if (methodInfo != null)
				{
					//  Check that this method is valid -- it needs to return a value and has to be parameterless:
					//  We are only looking for a parameterless Function/Property:
					if (methodInfo.GetParameters().Length > 0)
					{
						continue;
					}

					//  Make sure that this method is not void!  It has to be a Function!
					if (methodInfo.ReturnType == typeof(void))
					{
						continue;
					}

					//  Retrieve the Selectors/ParseFormat value:
					selectorInfo.Result = methodInfo.Invoke(current, new object[0]);
					return true;
				}
			}

			return false;
		}
	}
}
