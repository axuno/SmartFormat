using System;
using System.Linq;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// Allows you to specify the sorting priority of an extension.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExtensionPriorityAttribute : Attribute
    {
        #region Properties

        public ExtensionPriority FormatterPriority { get; private set; }
        public ExtensionPriority SourcePriority { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Applies a sorting priority to an extension.
        /// </summary>
        /// <param name="newPriority"></param>
        public ExtensionPriorityAttribute(ExtensionPriority newPriority)
        {
            this.FormatterPriority = newPriority;
            this.SourcePriority = newPriority;
        }
        /// <summary>
        /// Use this constructor if your extension implements both ISource and IFormatter.
        /// </summary>
        public ExtensionPriorityAttribute(ExtensionPriority sourcePriority, ExtensionPriority formatterPriority)
        {
            this.SourcePriority = sourcePriority;
            this.FormatterPriority = formatterPriority;
        }

        #endregion

        #region Static Helper Methods

        public static Comparison<ISource> SourceComparer()
        {
            return new Comparison<ISource>((a,b) => 
                GetExtensionPriority(a).CompareTo(GetExtensionPriority(b))
            );
        }
        public static Comparison<IFormatter> FormatterComparer() 
        {
            return new Comparison<IFormatter>((a, b) =>
                GetExtensionPriority(a).CompareTo(GetExtensionPriority(b))
            );
        }
        public static ExtensionPriority GetExtensionPriority(IFormatter extension)
        {
            var extensionAttribute = (ExtensionPriorityAttribute)extension.GetType().GetCustomAttributes(typeof(ExtensionPriorityAttribute), true).FirstOrDefault();
            return (extensionAttribute != null) ? extensionAttribute.FormatterPriority : ExtensionPriority.Normal;
        }
        public static ExtensionPriority GetExtensionPriority(ISource extension)
        {
            var extensionAttribute = (ExtensionPriorityAttribute)extension.GetType().GetCustomAttributes(typeof(ExtensionPriorityAttribute), true).FirstOrDefault();
            return (extensionAttribute != null) ? extensionAttribute.SourcePriority : ExtensionPriority.Normal;
        }

        #endregion
    }
}