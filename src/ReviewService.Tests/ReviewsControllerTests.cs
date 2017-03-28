using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ReviewService.Controllers;
using Shouldly;
using Microsoft.Extensions.Options;

namespace ReviewService.Tests
{
    public class ReviewsControllerTests
    {
        [Fact]
        public async Task Reviews_Post()
        {
            // arrange
            var review = ReviewInputFixture;

            var options =  new OptionsManager<AppSettings>(new List<ConfigureOptions<AppSettings>>());

            options.Value.DbHost = "localhost";
            options.Value.DbPort = 27017;
            var controller = new ReviewsController(options);

            // act
            var result = await controller.Post(review);

            // Assert
            var viewResult = result.ShouldBeAssignableTo<OkObjectResult>();
            var reviewResult = viewResult.Value.ShouldBeAssignableTo<ReviewsController.ReviewResult>();

            reviewResult.Text.ShouldBe(review.Text);
            reviewResult.AuthorName.ShouldBe(review.AuthorName);
            reviewResult.DateCreated.ShouldBeGreaterThan(DateTime.Now.AddMinutes(-1)); ;
            reviewResult.Id.ShouldNotBeNullOrEmpty();
            reviewResult.Rating.ShouldBe(review.Rating);
            reviewResult.Title.ShouldBe(review.Title);
        }

        private static ReviewsController.ReviewInput ReviewInputFixture => new ReviewsController.ReviewInput
        {
            ItemId = "ItemId" + Guid.NewGuid(),
            Rating = new Random().Next(1, 6),
            Text = "Mock Text" + Guid.NewGuid(),
            AuthorName = "AuthorName" + Guid.NewGuid(),
            Title = "Meh" + Guid.NewGuid(),
            AuthorId = "Author -" + Guid.NewGuid(),
            ItemName = "Item Name" + Guid.NewGuid()
        };

        [Fact]
        public async Task Review_GetReviewsByItems()
        {
            var itemId = Guid.NewGuid().ToString();
            // arrange
            var review1 = ReviewInputFixture;
            review1.ItemId = itemId;

            var review2 = ReviewInputFixture;
            review2.ItemId = itemId;

            var review3 = ReviewInputFixture;
            review3.ItemId = itemId;

            var review4 = ReviewInputFixture;
            review4.ItemId = itemId;

            var review5 = ReviewInputFixture;
            review5.ItemId = itemId;

            var options = new OptionsManager<AppSettings>(new List<ConfigureOptions<AppSettings>>());
            options.Value.DbHost = "localhost";
            options.Value.DbPort = 27017;
            var controller = new ReviewsController(options);
            
            // act
            var reviewPostResult1 = await controller.Post(review1);
            var reviewResult1 = GetValueFromPostReview(reviewPostResult1);

            var reviewPostResult2 = await controller.Post(review2);
            var reviewResult2 = GetValueFromPostReview(reviewPostResult2);

            var reviewPostResult3 = await controller.Post(review3);
            var reviewResult3 = GetValueFromPostReview(reviewPostResult3);

            var reviewPostResult4 = await controller.Post(review4);
            var reviewResult4 = GetValueFromPostReview(reviewPostResult4);

            var reviewPostResult5 = await controller.Post(review5);
            var reviewResult5 = GetValueFromPostReview(reviewPostResult5);

            var reviewsByItemsResult = controller.GetReviewsByItems(review1.ItemId, "", "", onlyApproved:false);

            // assert - get all
            var getAllReviewsResult = reviewsByItemsResult.ShouldBeAssignableTo<OkObjectResult>();
            var getAllReviewsData = getAllReviewsResult.Value.ShouldBeAssignableTo<IEnumerable<ReviewsController.ReviewResult>>();

            getAllReviewsData.Count().ShouldBe(5);

            // act 2 - get only approved

            var getApprovedReviewsRequest = controller.GetReviewsByItems(review1.ItemId, "", "", onlyApproved: true);

            // assert
            var getApprovedReviewsResult = getApprovedReviewsRequest.ShouldBeAssignableTo<OkObjectResult>();
            var getApprovedReviewsData = getApprovedReviewsResult.Value.ShouldBeAssignableTo<IEnumerable<ReviewsController.ReviewResult>>();

            getApprovedReviewsData.Count().ShouldBe(0);

            // act 3 get by author
            var getReviewByAuthorRequest = controller.GetReviewsByItemsAndAuthor(review1.ItemId, review1.AuthorId);

            // assert
            var getReviewByAuthorResult = getReviewByAuthorRequest.ShouldBeAssignableTo<OkObjectResult>();
            var getReviewByAuthorData = getReviewByAuthorResult.Value.ShouldBeAssignableTo<ReviewsController.ReviewResult>();

            getReviewByAuthorData.ShouldNotBeNull();

            // act 4 upvote
            var upvoteMineRequest = controller.PostVote(reviewResult1.Id, true, review1.AuthorId);
            var upvoteOtherRequest = controller.PostVote(reviewResult5.Id, true, review2.AuthorId);

            // assert
            upvoteMineRequest.ShouldBeAssignableTo<BadRequestObjectResult>();
            upvoteOtherRequest.ShouldBeAssignableTo<OkObjectResult>();

            // assert 2 - reviews bytitem and date
            var getReviewsBySortDateRequest = controller.GetReviewsByItems(review1.ItemId, "", "date", onlyApproved: false);

            var getReviewsBySortDateResult = getReviewsBySortDateRequest.ShouldBeAssignableTo<OkObjectResult>();
            var getReviewsBySortDateData = getReviewsBySortDateResult.Value.ShouldBeAssignableTo<IEnumerable<ReviewsController.ReviewResult>>();

            var resultAsList = getReviewsBySortDateData.ToList();
            resultAsList.Count.ShouldBe(5);
            resultAsList.First().Id.ShouldBe(reviewResult5.Id);
            resultAsList.Last().Id.ShouldBe(reviewResult1.Id);

            var getReviewsByRelevanceRequest = controller.GetReviewsByItems(review1.ItemId, "", "", onlyApproved: false);

            var getReviewsByRelevanceResult = getReviewsByRelevanceRequest.ShouldBeAssignableTo<OkObjectResult>();
            var getReviewsByRelevanceData = getReviewsByRelevanceResult.Value.ShouldBeAssignableTo<IEnumerable<ReviewsController.ReviewResult>>();

            getReviewsByRelevanceData.First().AuthorName.ShouldBe(review5.AuthorName);

            // act 5 - get rating avg

            var avgRating = (review1.Rating +
                             review2.Rating +
                             review3.Rating +
                             review4.Rating +
                             review5.Rating)/5;

            var getRatingRequest = controller.GetItemRating(itemId, false);

            // assert
            var getRatingResult = getRatingRequest.ShouldBeAssignableTo<OkObjectResult>();
            var getRatingData = getRatingResult.Value.ShouldBeAssignableTo<decimal>();
            
            getRatingData.ShouldBe(avgRating);

            // act 6 - get review summary
        }

        private static ReviewsController.ReviewResult GetValueFromPostReview(IActionResult reviewResult1)
        {
            var reviewViewResult = reviewResult1.ShouldBeAssignableTo<OkObjectResult>();
            return  reviewViewResult.Value.ShouldBeAssignableTo<ReviewsController.ReviewResult>();
        }
    }
}
