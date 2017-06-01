using System;

namespace MasterApi.Core.Models
{
    public abstract class AuditableEntity : BaseObjectState
    {
        ///// <summary>
        ///// Create datetime.
        ///// </summary>
        public DateTimeOffset Created { get; set; }
    }
}
