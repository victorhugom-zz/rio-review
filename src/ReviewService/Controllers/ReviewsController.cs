using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReviewService.Models;
using ReviewService.Repositories;
using MongoDB.Driver;

namespace ReviewService.Controllers
{
    [Route("api/[controller]")]
    public class ReviewsController : Controller
    {
        public MongoRepositoryBase<Review> reviewRepo => new MongoRepositoryBase<Review>(new MongoClient("mongodb://localhost:27017"));
        
        // GET api/reviews/5
        [HttpGet("{id}")]
        public Review Get(string id)
        {
            return reviewRepo.Get(id);
        }

        // POST api/reviews/5/vote
        [HttpPost("{id}/vote")]
        public void PostVote(string id, [FromBody]Vote vote)
        {
            var review = reviewRepo.Get(id);

            var found = review.Votes.FirstOrDefault(x => x.AuthorId == vote.AuthorId);
            if (found != null)
            {
                found.IsRelevant = vote.IsRelevant;
            } else {
                review.Votes.Add(vote);
            }

            reviewRepo.Update(id, review);
        }

        // POST api/reviews
        [HttpPost]
        public async Task Post([FromBody]ReviewInput input)
        {
            var review = input.ToReview();

            await reviewRepo.Create(review);
        }

        // POST api/reviews/5/approve
        [HttpPost("{id}/approve")]
        public async Task PostApprove(string id, [FromBody]bool approved)
        {
            var review = reviewRepo.Get(id);
            review.Approved = approved;

            await reviewRepo.Update(id, review);
        }

        // GET api/reviews/item/5/rating
        [HttpGet("item/{itemId}/rating")]
        public decimal GetItemRating(string itemId)
        {
            return GetItems(itemId).GetAverageRating();
        }

        // GET api/reviews/item/5
        [HttpGet("item/{itemId}")]
        public IEnumerable<ReviewResult> GetReviewsByItems(string itemId)
        {
            return GetItems(itemId).OrderedByRelevance().Select(x => new ReviewResult(x));
        }

        // GET api/reviews/item/5/rating
        [HttpGet("item/{itemId}/starsSummary")]
        public Dictionary<int, int> GetStarsSummary(string itemId)
        {
            return GetItems(itemId).GetStarsSummary();
        }

        private IQueryable<Review> GetItems(string itemId)
        {
            return reviewRepo.GetItems(x => x.Approved && x.ItemId == itemId);
        }

        public class ReviewResult
        {
            public string Id { get; set; }
            public string AuthorName { get; set; }
            public DateTime DateCreated { get; set; }
            public decimal Rating { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }
            public Dictionary<int, int> Votes { get; set; }

            public ReviewResult(Review review)
            {
                Id = review.Id;
                AuthorName = review.AuthorName;
                DateCreated = review.DateCreated;
                Rating = review.Rating;
                Text = review.Text;
                Title = review.Title;
                Votes = review.Votes.Aggregate(new Dictionary<int, int>() { { +1, 0 }, { -1, 0 } }, (acc, curr) =>
                {
                    int vote;
                    if (curr.IsRelevant == null)
                    {
                        return acc;
                    }
                    else if (curr.IsRelevant == true)
                    {
                        vote = +1;
                    }
                    else
                    {
                        vote = -1;
                    }

                    acc[vote]++;

                    return acc;
                });
            }
        }

        public class ReviewInput
        {
            public string ItemId { get; set; }
            public string ItemName { get; set; } //TODO from db
            public string AuthorId { get; set; }
            public string AuthorName { get; set; } //TODO from db
            public decimal Rating { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }

            public Review ToReview()
            {
                return new Review()
                {
                    Approved = true,
                    DateCreated = DateTime.Now,
                    AuthorId = AuthorId,
                    AuthorName = AuthorName,
                    ItemId = ItemId,
                    ItemName = ItemName,
                    Rating = Rating,
                    Text = Text,
                    Title = Title,
                };
            }
        }
    }
}
