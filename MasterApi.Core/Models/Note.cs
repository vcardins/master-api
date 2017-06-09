using System;
using MasterApi.Core.Filters;
using MasterApi.Core.Account.Models;

namespace MasterApi.Core.Models
{
    public class Note : AuditableEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Meta { get; set; }
        public bool Starred { get; set; }
        public int SortOrder { get; set; }
        public int UserId { get; set; }
        public UserAccount UserAccount { get; set; }
        [AutoPopulate]
        public DateTimeOffset? Updated { get; set; }
    }

}
