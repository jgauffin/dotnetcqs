using System;

namespace DotNetCqs.AspNet
{
    /// <summary>
    ///     Used to authorize users
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeCqsAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizeCqsAttribute" /> class.
        /// </summary>
        /// <param name="grantedRoleNames">Roles that are granted access.</param>
        public AuthorizeCqsAttribute(params string[] grantedRoleNames)
        {
            GrantedRoleNames = grantedRoleNames;
        }

        /// <summary>
        ///     Roles that may access the current resource
        /// </summary>
        public string[] GrantedRoleNames { get; private set; }

        /// <summary>
        ///     Allow anonymous access
        /// </summary>
        /// <value>
        ///     Default is <c>false</c>.
        /// </value>
        public bool AllowAnonymous { get; set; }
    }
}