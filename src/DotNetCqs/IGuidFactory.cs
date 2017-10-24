using System;

namespace DotNetCqs
{
    /// <summary>
    ///     GUIDs are the preferred way of identifying items. But as GUIDs can hurt data performance we've using this interface
    ///     to allow you to specialize the GUID creation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         To get better performance you can use COMBs instead of GUIDs. They look the same but give better performance.
    ///         Another name for them is sequential GUIDs.
    ///     </para>
    /// </remarks>
    public interface IGuidFactory
    {
        /// <summary>
        ///     Create a new GUID.
        /// </summary>
        /// <returns>Created GUID.</returns>
        Guid Create();
    }
}