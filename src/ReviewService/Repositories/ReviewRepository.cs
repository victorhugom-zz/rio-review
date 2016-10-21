using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using ReviewService.Models;

namespace ReviewService.Repositories
{
    public class ReviewRepository : MongoRepositoryBase<Review>
    {
        public ReviewRepository(MongoClient client) : base(client)
        {

        }

        public IQueryable<Review> GetItems(string itemId, bool onlyApproved = true)
        {
            return GetItems(x => x.ItemId == itemId && (!onlyApproved || x.Approved));
        }

        public IQueryable<Review> GetOrderedByRelevance(string itemId, bool onlyApproved = true)
        {
            return GetItems(itemId, onlyApproved)
                        .OrderByDescending(x => x.RelevancyFactor);
        }

        public IEnumerable<Review> GetOrderedByDate(string itemId, bool onlyApproved = true)
        {
            return GetItems(itemId, onlyApproved)
                        .OrderByDescending(x => x.DateCreated);
        }

        public decimal GetAverageRating(string itemId, bool onlyApproved = true)
        {
            var query = GetItems(x => true).Average(x => x.Rating);
            return query;
        }

        public Dictionary<string, int> GetStarsSummary(string itemId, bool onlyApproved = true)
        {
            return GetItems(itemId, onlyApproved).ToList().Aggregate(new Dictionary<int, int>
            {
                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
            }, (acc, curr) =>
            {
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
