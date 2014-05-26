using System;

namespace DotNetCqs
{
    /// <summary>
    /// Uses .NET to generated Guids
    /// </summary>
    public class DotNetGuidFactory : IGuidFactory
    {
        /// <summary>
        /// Create a new GUID.
        /// </summary>
        /// <returns>
        /// Created GUID.
        /// </returns>
        public Guid Create()
        {
            return Guid.NewGuid();
        }
    }
}
