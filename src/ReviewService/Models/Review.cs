using ReviewService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models
{
    public class Review : IDocumentBase
    {
        public Review()
        {
            Votes = new List<Vote>();
            DateCreated = DateTime.Now;
        }

        public string Id { get; set; }
        public string Type => nameof(Review);
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
    
        [BsonRepresentation(BsonType.Double)]
        public decimal Rating { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<Vote> Votes { get; set; }
        public bool Approved { get; set; }
        public DateTime DateCreated { get; set; }

        public int RelevancyFactor
        {
            get { return IsRelevantCount - IsNotRelevantCount; }
            set { }
        }

        public int IsRelevantCount
        {
            get { return Votes.Count(x => x.IsRelevant != null && x.IsRelevant.Value); }
            set { }
        }
        public int IsNotRelevantCount
        {
            get { return Votes.Count(x => x.IsRelevant != null && !x.IsRelevant.Value); }
            set { }
        }
    }

    public class Vote
    {
        public string AuthorId { get; set; }
        public bool? IsRelevant { get; set; }
    }
}
