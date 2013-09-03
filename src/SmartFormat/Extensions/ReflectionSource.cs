using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using System.Reflection;

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

        public void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
        {
            if (current == null)
            {
                return;
            }

            // REFLECTION:
            // Let's see if the argSelector is a Selectors/Field/ParseFormat:
            var sourceType = current.GetType();

	        var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
	        bindingFlags |= Smart.Settings.GetCaseSensitivityBindingFlag();

			var members = sourceType.GetMember(selector.Text, bindingFlags);
            foreach (var member in members)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        //  Selector is a Field; retrieve the value:
                        var field = (FieldInfo) member;
                        result = field.GetValue(current);
                        handled = true;
                        return;
                    case MemberTypes.Property:
                    case MemberTypes.Method:
                        MethodInfo method;
                        if (member.MemberType == MemberTypes.Property)
                        {
                            //  Selector is a Property
                            var prop = (PropertyInfo) member;
                            //  Make sure the property is not WriteOnly:
                            if (prop.CanRead)
                            {
                                method = prop.GetGetMethod();
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //  Selector is a method
                            method = (MethodInfo) member;
                        }

                        //  Check that this method is valid -- it needs to return a value and has to be parameterless:
                        //  We are only looking for a parameterless Function/Property:
                        if (method.GetParameters().Length > 0)
                        {
                            continue;
                        }

                        //  Make sure that this method is not void!  It has to be a Function!
                        if (method.ReturnType == typeof(void))
                        {
                            continue;
                        }

                        //  Retrieve the Selectors/ParseFormat value:
                        result = method.Invoke(current, new object[0]);
                        handled = true;
                        return;
                        
                }
            }

        }
    }
}
