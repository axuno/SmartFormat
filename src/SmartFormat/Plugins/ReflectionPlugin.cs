using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core;
using SmartFormat.Core.Plugins;
using SmartFormat.Core.Parsing;
using System.Reflection;

namespace SmartFormat.Plugins
{
    public class ReflectionPlugin : ISourcePlugin
    {
        public ReflectionPlugin(SmartFormatter formatter)
        {
            // Add some special info to the parser:
            formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
            formatter.Parser.AddAdditionalSelectorChars("_");
            formatter.Parser.AddOperators(".");
        }

        public void EvaluateSelector(SmartFormatter formatter, object[] args, object current, Selector selector, ref bool handled, ref object result)
        {
            if (current == null)
            {
                return;
            }

            // REFLECTION:
            // Let's see if the argSelector is a Selectors/Field/ParseFormat:
            var sourceType = current.GetType();
            var members = sourceType.GetMember(selector.Text);
            foreach (var member in members)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        //  Selector is a Field; retrieve the value:
                        var field = member as FieldInfo;
                        result = field.GetValue(current);
                        handled = true;
                        return;
                    case MemberTypes.Property:
                    case MemberTypes.Method:
                        MethodInfo method;
                        if (member.MemberType == MemberTypes.Property)
                        {
                            //  Selector is a Property
                            PropertyInfo prop = member as PropertyInfo;
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
                            method = member as MethodInfo;
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
