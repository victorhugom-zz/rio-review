using ReviewService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.Models
{
    public class Review : IDocumentBase
    {
        public string Id { get; set; }
        public string Type => nameof(Review);
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public decimal Rating { get; set; }
        public string Text { get; set; }
        public List<Vote> Votes { get; set; }
        public bool Approved { get; set; }

        public int GetRelevance()
        {
            return Votes.Count(x => x.IsRelevant);
        }
    }

    public class Vote
    {
        public string AuthorId { get; set; }
        public bool IsRelevant { get; set; }
    }
}
