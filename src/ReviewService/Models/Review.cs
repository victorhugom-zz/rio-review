using ReviewService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public decimal Rating { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<Vote> Votes { get; set; }
        public bool Approved { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class Vote
    {
        public string AuthorId { get; set; }
        public bool? IsRelevant { get; set; }
    }

    public static class ReviewExtensions
    {
        public static IEnumerable<Review> OrderedByRelevance(this IQueryable<Review> reviews)
        {
            return reviews.ToList().OrderByDescending(x => x.Votes.Count(v => v.IsRelevant ?? false) - x.Votes.Count(v => !v.IsRelevant ?? false));
        }

        public static IEnumerable<Review> OrderedByDate(this IQueryable<Review> reviews)
        {
            return reviews.OrderByDescending(x => x.DateCreated);
        }

        public static decimal GetAverageRating(this IQueryable<Review> reviews)
        {
            return reviews.ToList().Average(x => x.Rating);
        }

        public static Dictionary<string, int> GetStarsSummary(this IQueryable<Review> reviews)
        {
            return reviews.ToList().Aggregate(new Dictionary<int, int>()
            {
                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
            }, (acc, curr) => {
                var stars = (int)Math.Floor(curr.Rating);

                if (!acc.Keys.Contains(stars))
                {
                    return acc;
                }

                acc[stars]++;

                return acc;
            }).ToDictionary(x => x.Key == 1 ? "oneStar" :
                x.Key == 2 ? "twoStars" :
                x.Key == 3 ? "threeStars" :
                x.Key == 4 ? "fourStars" :
                x.Key == 5 ? "fiveStars" : "", x => x.Value);
        }
    }
}
