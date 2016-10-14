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
        public MongoRepositoryBase<Review> reviewRepo => new MongoRepositoryBase<Review>(new MongoClient(""));
        
        // GET api/reviews
        //[HttpGet]
        //public IEnumerable<Review> Get()
        //{
        //    return reviewRepo.GetItems(x => true);
        //}

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
        public async Task Post([FromBody]Review review)
        {
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
        [HttpGet("item/{id}/rating")]
        public decimal GetItemRating(string id)
        {
            return reviewRepo.GetItems(x => x.Approved && x.ItemId == id)
                .Average(x => x.Rating);
        }

        // GET api/reviews/item/5
        [HttpGet("item/{id}")]
        public IEnumerable<Review> GetReviewsByItems(string id)
        {
            return reviewRepo.GetItems(x => x.Approved && x.ItemId == id)
                .OrderByDescending(x => x.GetRelevance());
        }

        // PUT api/values/5
        //[HttpPut("{id}")]
        //public async Task Put(string id, [FromBody]Review review)
        //{
        //    await reviewRepo.Update(id, review);
        //}

        // DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
