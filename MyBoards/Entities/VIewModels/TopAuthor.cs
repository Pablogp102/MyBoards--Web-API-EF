using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MyBoards.Entities.VIewModels
{
    public class TopAuthor
    {
        public string FullName { get; set; }
        public int WorkItemsCreated { get; set; }
    }
}
