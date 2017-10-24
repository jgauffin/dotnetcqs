using System;
using System.Collections.Generic;

namespace DotNetCqs.DependencyInjection
{
    /// <summary>
    ///     Wraps a message handler to be able to manage lifetimes of the handler and it's dependencies.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implementations of this interface MUST disposed/clean up dependencies when this scope is disposed.
    ///     </para>
    /// </remarks>
    public interface IHandlerScope : IDisposable
    {
        /// <summary>
        ///     Find all implementations of the given message handler
        /// </summary>
        /// <param name="messageHandlerServiceType">
        ///     The generic interface for a handler, with the message type specified. For
        ///     example <c><![CDATA[IMessageHandler<SimpleMessage>]]></c>
        /// </param>
        /// <returns>All implementations</returns>
        IEnumerable<object> Create(Type messageHandlerServiceType);

        /// <summary>
        ///     Resolve one of the dependencies from the handler implementations.
        /// </summary>
        /// <typeparam name="T">Type of dependency</typeparam>
        /// <returns></returns>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// queueListener.OnMessageProcessed += (source, args) {
        ///     if (args.Exception != null)
        ///     {
        ///         var uow = args.Scope.ResolveDependency<IUnitOfWork>();
        ///         uow.SaveChanges();
        ///     }
        /// };
        /// ]]>
        /// </code>
        /// </example>
        IEnumerable<T> ResolveDependency<T>();
    }
}