using System;

namespace MasterApi.Core.ViewModels
{
    public class NoteOutput 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Meta { get; set; }
        public bool Starred { get; set; }
        public int SortOrder { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }

}
