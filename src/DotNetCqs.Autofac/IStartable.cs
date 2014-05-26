using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Autofac
{
    /// <summary>
    /// Allows you to create services which should be started when the application starts and be shut down when your app is shut down.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is intended for services which are stored in the main container (i.e. SingleInstance).
    /// </para>
    /// </remarks>
    public interface IStartable
    {
        /// <summary>
        /// Start service
        /// </summary>
        void Start();

        /// <summary>
        /// Stop service
        /// </summary>
        void Stop();
    }
}
