
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PaymentService3.Controllers;
using PaymentService3.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TestPayment
    {
        [TestClass]
        public class PaymentControlerTest
        {
            [TestMethod]
            public async Task CreateCheckoutSession_ReturnsOk()
            {
                // Arrange
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                    { "StripeSettings:SecretKey", "sk_test_51PECtbRpjT8p9PLVgi8k4gbyDTSNSs8m5lzT1kFCbl3vbyZfhi4JpQNBxOsycliOChEtEMstO4HK5KnEAEtTkFAt00jykFmX3J" }
                    })
                    .Build();

                var controller = new PaymentController(configuration);
                var request = new CreateCheckoutSessionRequest
                {
                    CustomerId = "customer123",
                    ProductName = "Product 1",
                    ProductPrice = 250,
                    Quantity = 1
                };

                // Act
                var result = await controller.CreateCheckoutSession(request);

                // Assert
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var okResult = (OkObjectResult)result;
                Assert.IsNotNull(okResult.Value);
                Assert.AreEqual(200, okResult.StatusCode);
            }
        }
    }


