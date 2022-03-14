// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// Initializes an <see cref="ISource"/> or <see cref="IFormatter"/>.
    /// </summary>
    public interface IInitializer
    {
        /// <summary>
        /// Initializes an <see cref="ISource"/> or <see cref="IFormatter"/>.
        /// The method gets called when adding an extension to a <see cref="SmartFormatter"/> instance.
        /// </summary>
        /// <param name="smartFormatter"></param>
        void Initialize(SmartFormatter smartFormatter);
    }
}