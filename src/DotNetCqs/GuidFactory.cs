using System;

namespace DotNetCqs
{
    /// <summary>
    /// Class uses to generate GUIDs. Read the class documentation of <see cref="IGuidFactory"/> for more information.
    /// </summary>
    /// <remarks>
    /// <para>Uses <see cref="DotNetGuidFactory"/> per default.</para>
    /// </remarks>
    public class GuidFactory
    {
        private static IGuidFactory _instance = new DotNetGuidFactory();

        /// <summary>
        /// Assign a new factory implementation
        /// </summary>
        /// <param name="factory">factory to use</param>
        /// <exception cref="System.ArgumentNullException">factory</exception>
        public static void Assign(IGuidFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _instance = factory;
        }

        /// <summary>
        /// Creates a new GUID.
        /// </summary>
        /// <returns>Generated GUID.</returns>
        public static Guid Create()
        {
            return _instance.Create();
        }
    }
}