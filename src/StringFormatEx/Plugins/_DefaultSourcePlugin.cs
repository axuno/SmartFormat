using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx.Plugins
{
    public class _DefaultSourcePlugin : IStringFormatterPlugin
    {

        public IEnumerable<EventHandler<ExtendSourceEventArgs>> GetSourceExtensions()
        {
            return new EventHandler<ExtendSourceEventArgs>[]
                { _DefaultSourcePlugin._GetDefaultSource };
        }

        public IEnumerable<EventHandler<ExtendFormatEventArgs>> GetFormatExtensions()
        {
            return new EventHandler<ExtendFormatEventArgs>[] {};
        }


        /// <summary>
        /// This is the Default method for evaluating the Source.
        /// 
        /// 
        /// If this is the first selector and the selector is an integer, then it returns the (global) indexed argument (just like String.Format).
        /// If the Current item is a Dictionary that contains the Selector, it returns the dictionary item.
        /// Otherwise, Reflection will be used to determine if the Selector is a Property, Field, or Method of the Current item.
        /// </summary>
        [CustomFormatPriority(CustomFormatPriorities.Low)]
        private static void _GetDefaultSource(object source, ExtendSourceEventArgs e) 
        {
            ICustomSourceInfo info = e.SourceInfo;

            //  If it wasn't handled, let's evaluate the source on our own:
            //  We will see if it's an argument index, dictionary key, or a property/field/method.
            //  Maybe source is the global index of our arguments? 
            int argIndex;
            if (info.SelectorIndex == 0 && int.TryParse(info.Selector, out argIndex)) {
                if (argIndex < info.Arguments.Length) {
                    info.Current = info.Arguments[argIndex];
                }
                else {
                    //  The index is out-of-range!
                }
                return;
            }

            //  Maybe source is a Dictionary?
            if (info.Current is IDictionary && ((IDictionary)info.Current).Contains(info.Selector)) {
                info.Current = ((IDictionary)info.Current)[info.Selector];
                return;
            }


            // REFLECTION:
            // Let's see if the argSelector is a Property/Field/Method:
            var sourceType = info.Current.GetType();
            MemberInfo[] members = sourceType.GetMember(info.Selector);
            foreach (MemberInfo member in members) {
                switch (member.MemberType) {
                    case MemberTypes.Field:
                        //  Selector is a Field; retrieve the value:
                        FieldInfo field = member as FieldInfo;
                        info.Current = field.GetValue(info.Current);
                        return;
                    case MemberTypes.Property:
                    case MemberTypes.Method:
                        MethodInfo method;
                        if (member.MemberType == MemberTypes.Property) {
                            //  Selector is a Property
                            PropertyInfo prop = member as PropertyInfo;
                            //  Make sure the property is not WriteOnly:
                            if (prop.CanRead) {
                                method = prop.GetGetMethod();
                            }
                            else {
                                continue;
                            }
                        }
                        else {
                            //  Selector is a Method
                            method = member as MethodInfo;
                        }

                        //  Check that this method is valid -- it needs to be a Function (return a value) and has to be parameterless:
                        //  We are only looking for a parameterless Property/Method:
                        if ((method.GetParameters().Length > 0)) {
                            continue;
                        }

                        //  Make sure that this method is not a Sub!  It has to be a Function!
                        if ((method.ReturnType == typeof(void))) {
                            continue;
                        }

                        //  Retrieve the Property/Method value:
                        info.Current = method.Invoke(info.Current, new object[0]);
                        return;
                }
            }
            //  If we haven't returned yet, then the item must be invalid.
        }

    }
}