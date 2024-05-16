﻿using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using RabbitMQ.Client;
using Stripe;
using Stripe.Checkout;
using PaymentService3.Models;
using System;

namespace PaymentService3.Controllers
{
    /// <summary>
    /// Controlleren er ansvarlig for håndtering af betalingsrelaterede operationer.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly StripeSettings _stripeConfig;
        public PaymentController(IConfiguration configuration)
        {
            _stripeConfig = configuration.GetSection("StripeSettings").Get<StripeSettings>();
            StripeConfiguration.ApiKey = _stripeConfig.SecretKey;
        }

        [HttpPost]
        [Route("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            try
            {
                int customerId = request.CustomerId;
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = request.ProductName,
                                },
                                UnitAmount = request.ProductPrice,
                            },
                            Quantity = request.Quantity,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = "https://example.com/success",
                    CancelUrl = "https://example.com/cancel",
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // Send customerId to RabbitMQ
                SendCustomerIdToQueue(customerId.ToString());

                return Ok(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private void SendCustomerIdToQueue(string customerId)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" }; // Update to your RabbitMQ server address

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "paymentQueue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(customerId);

                channel.BasicPublish(exchange: "",
                    routingKey: "paymentQueue",
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent {0}", customerId);
            }
        }
    }
}
