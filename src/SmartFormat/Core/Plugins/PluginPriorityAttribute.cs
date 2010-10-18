using System;
using System.Linq;

namespace SmartFormat.Core.Plugins
{
    /// <summary>
    /// Allows you to specify the sorting priority of a plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginPriorityAttribute : Attribute
    {
        #region Properties

        public PluginPriority FormatterPriority { get; private set; }
        public PluginPriority SourcePriority { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Applies a sorting priority to a plugin.
        /// </summary>
        /// <param name="newPriority"></param>
        public PluginPriorityAttribute(PluginPriority newPriority)
        {
            this.FormatterPriority = newPriority;
            this.SourcePriority = newPriority;
        }
        /// <summary>
        /// Use this constructor if your plugin implements both ISource and IFormatter.
        /// </summary>
        public PluginPriorityAttribute(PluginPriority sourcePriority, PluginPriority formatterPriority)
        {
            this.SourcePriority = sourcePriority;
            this.FormatterPriority = formatterPriority;
        }

        #endregion

        #region Static Helper Methods

        public static Comparison<ISource> SourceComparer()
        {
            return new Comparison<ISource>((a,b) => 
                GetPluginPriority(a).CompareTo(GetPluginPriority(b))
            );
        }
        public static Comparison<IFormatter> FormatterComparer() 
        {
            return new Comparison<IFormatter>((a, b) =>
                GetPluginPriority(a).CompareTo(GetPluginPriority(b))
            );
        }
        public static PluginPriority GetPluginPriority(IFormatter plugin)
        {
            var pluginAttribute = (PluginPriorityAttribute)plugin.GetType().GetCustomAttributes(typeof(PluginPriorityAttribute), true).FirstOrDefault();
            return (pluginAttribute != null) ? pluginAttribute.FormatterPriority : PluginPriority.Normal;
        }
        public static PluginPriority GetPluginPriority(ISource plugin)
        {
            var pluginAttribute = (PluginPriorityAttribute)plugin.GetType().GetCustomAttributes(typeof(PluginPriorityAttribute), true).FirstOrDefault();
            return (pluginAttribute != null) ? pluginAttribute.SourcePriority : PluginPriority.Normal;
        }

        #endregion
    }
}