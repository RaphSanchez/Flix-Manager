using System;

namespace BlazorMovies.Shared.CustomAttributes
{
    /// <summary>
    /// Custom attribute to decorate any root entity
    /// that needs to be part of the <em>auditing</em>
    /// mechanism.
    /// </summary>
    /// <remarks>
    /// It has an <strong>IsDeletable</strong>
    /// <dfn>named parameter</dfn> (optional) with a
    /// default value of true. This means that unless
    /// otherwise instructed, the entity will also
    /// be assigned an IsDeletable shadow property
    /// in the data model created by the
    /// Application/DataStore project.
    /// <para>
    /// For more info go to Udemy course: "Complete Guide
    /// to ASP.Net Core RESTful API with Blazor WASM"
    /// episodes 29 and 30 by Frank Liu.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct)]
    public class IsAuditableAttribute : Attribute
    {
        /// <summary>
        /// Named parameter to determine if the entity should
        /// be included in the <em>soft delete</em> mechanism.
        /// If true, then the entity will be assigned an
        /// <strong>IsDeleted</strong> shadow property.
        /// </summary>
        public bool IsDeletable { get; set; }

        public IsAuditableAttribute()
        {
            IsDeletable = true;
        }
    }
}


