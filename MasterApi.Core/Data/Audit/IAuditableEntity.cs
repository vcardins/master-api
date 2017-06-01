using System;
using MasterApi.Core.Data.Infrastructure;

namespace MasterApi.Core.Data.Audit
{
    public interface IAuditableEntity : IObjectState
    {
        int? CreatedById { get; set; }

        DateTimeOffset Created { get; set; }

        int? UpdatedById { get; set; }

        DateTimeOffset Updated { get; set; }
    }
}
