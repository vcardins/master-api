using MasterApi.Core.Data.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MasterApi.Core.Models
{
    public abstract class BaseObjectState : ValidatableModel, IObjectState
    {
        [NotMapped]
        [IgnoreDataMember]
        public ObjectState ObjectState { get; set; }
    }
}
