﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using ReviewService.Models;
using ReviewService.Repositories;
using MongoDB.Driver;

namespace ReviewService.Controllers
{
    [Route("api/[controller]")]
    public class ReviewsController : Controller
    {
#if DEBUG
        public MongoRepositoryBase<Review> ReviewRepo => new MongoRepositoryBase<Review>(new MongoClient(new MongoClientSettings() { Server = new MongoServerAddress("localhost", 27017) }));
#else
        public MongoRepositoryBase<Review> ReviewRepo => new MongoRepositoryBase<Review>(new MongoClient(new MongoClientSettings() { Server = new MongoServerAddress("mongo", 27017) }));

#endif
        // GET api/reviews/item/5        
        /// <summary>
        /// Get reviews by item
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="authorId">id of user to return his vote on IsRelevant</param>
        /// <param name="order">pass "date" to order by date or use default behavior by relevance</param>
        /// <param name="skip">skip this many for pagination sake</param>
        /// <param name="take">take this many for pagination sake</param>
        /// <param name="onlyApproved">consider only approved</param>
        /// <returns>list of reviews</returns>
        [HttpGet("item/{itemId}")]
        public IEnumerable<ReviewResult> GetReviewsByItems(string itemId, [FromQuery]string authorId, [FromQuery]string order, [FromQuery]int skip = 0, [FromQuery]int take = 5, [FromQuery]bool onlyApproved = true)
        {
            take = Math.Min(take, 100);
            var items = GetItems(itemId, onlyApproved);

            var sorted = order == "date" ? items.OrderedByDate() : items.OrderedByRelevance();

            return sorted.Skip(skip).Take(take).Select(x => new ReviewResult(x, authorId));
        }

        [HttpGet("item/{itemId}/author/{authorId}")]
        public IActionResult GetReviewsByItemsAndAuthor(string itemId, string authorId)
        {
            var review = ReviewRepo.GetItems(x => x.ItemId == itemId && x.AuthorId == authorId).FirstOrDefault();

            if (review == null)
            {
                return BadRequest($"Review not found: item id {itemId} | author id {authorId}");
            }

            return Ok(new ReviewResult(review, authorId));
        }

        // POST api/reviews
        /// <summary>
        /// Register new review
        /// </summary>
        /// <param name="input">review witch is going to be registered</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ReviewInput input)
        {
            var review = input.ToReview();

            var found = ReviewRepo.GetItems(x => x.AuthorId == input.AuthorId && x.ItemId == review.ItemId).FirstOrDefault();

            if (found == null)
            {
                await ReviewRepo.Create(review);
            }
            else
            {
                found.Text = input.Text;
                found.Title = input.Title;
                found.Rating = input.Rating;
                await ReviewRepo.Update(found.Id, found);
            }

            return Ok(new ReviewResult(review));
        }

        // POST api/reviews/5/vote
        /// <summary>
        /// register new vote to review to adjust its relevance
        /// </summary>
        /// <param name="id">review id</param>
        /// <param name="isRelevant">true = +1, false = -1, null = 0</param>
        /// <param name="authorId">who is voting</param>
        /// <returns>returns the updated review</returns>
        [HttpPost("{id}/vote")]
        public IActionResult PostVote(string id, [FromBody]bool? isRelevant, [FromQuery]string authorId)
        {
            var review = ReviewRepo.Get(id);

            if (string.IsNullOrEmpty(authorId))
                return BadRequest("Author cannot be null or empty");

            if (review == null)
                return this.BadRequest($"Review not found: {id}");

            var found = review.Votes.FirstOrDefault(x => x.AuthorId == authorId);
            if (found != null)
            {
                found.IsRelevant = isRelevant;
            }
            else
            {
                review.Votes.Add(new Vote
                {
                    AuthorId = authorId,
                    IsRelevant = isRelevant
                });
            }

            ReviewRepo.Update(id, review);

            return this.Ok(new ReviewResult(review, authorId));
        }

        // GET api/reviews/item/5/rating
        /// <summary>
        /// get the average rating of an item
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="onlyApproved">consider only approved</param>
        /// <returns>average rating</returns>
        [HttpGet("item/{itemId}/rating")]
        public decimal GetItemRating(string itemId, [FromQuery]bool onlyApproved = true)
        {
            return GetItems(itemId, onlyApproved).GetAverageRating();
        }

        // GET api/reviews/item/5/rating
        /// <summary>
        /// returns a descrete representation of the ratings of an item
        /// </summary>
        /// <param name="itemId">item id</param>
        /// <param name="onlyApproved">consider only approved</param>
        /// <returns>
        /// {
        /// "oneStar": 0,
        /// "twoStars": 0,
        /// "threeStars": 0,
        /// "fourStars": 0,
        /// "fiveStars": 1
        /// }
        /// </returns>
        [HttpGet("item/{itemId}/starsSummary")]
        public Dictionary<string, int> GetStarsSummary(string itemId, [FromQuery]bool onlyApproved = true)
        {
            return GetItems(itemId, onlyApproved).GetStarsSummary();
        }

        // POST api/reviews/5/approve
        /// <summary>
        /// approve review to be displayed
        /// </summary>
        /// <param name="id">review id</param>
        /// <param name="approved">is approved</param>
        /// <returns></returns>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> PostApprove(string id, [FromBody]bool approved)
        {
            var review = ReviewRepo.Get(id);

            if (review == null)
            {
                return BadRequest($"Review for item not found: ${id}");
            }

            review.Approved = approved;

            await ReviewRepo.Update(id, review);

            return this.Ok();
        }

        private IQueryable<Review> GetItems(string itemId, bool onlyApproved = true)
        {
            return ReviewRepo.GetItems(x => x.ItemId == itemId && (!onlyApproved || x.Approved));
        }

        public class ReviewResult
        {
            public string Id { get; set; }
            public string AuthorName { get; set; }
            public DateTime DateCreated { get; set; }
            public decimal Rating { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }
            public bool? IsRelevant { get; set; }
            public Dictionary<int, int> Votes { get; set; }

            public ReviewResult(Review review, string authorId = null)
            {
                Id = review.Id;
                AuthorName = review.AuthorName;
                DateCreated = review.DateCreated;
                Rating = review.Rating;
                Text = review.Text;
                Title = review.Title;
                IsRelevant = review.Votes.FirstOrDefault(x => x.AuthorId == authorId)?.IsRelevant;
                Votes = review.Votes.Aggregate(new Dictionary<int, int>() { { +1, 0 }, { -1, 0 } }, (acc, curr) =>
                {
                    int vote;
                    switch (curr.IsRelevant)
                    {
                        case null:
                            return acc;
                        case true:
                            vote = +1;
                            break;
                        default:
                            vote = -1;
                            break;
                    }

                    acc[vote]++;

                    return acc;
                });
            }
        }

        public class ReviewInput
        {
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public string AuthorId { get; set; }
            public string AuthorName { get; set; }
            public decimal Rating { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }

            public Review ToReview()
            {
                return new Review()
                {
                    //Approved = true,
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
