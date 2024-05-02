using API.Errors;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController : BaseAPIController
    {
        // private const string whSecret = "whsec_88e5401350e3fe067bd9856523419d27b429e9a0e32b1cf61be2a6ae37212256";
        private const string whSecret = "";
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if(basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket"));

            return basket;
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], whSecret);
            
            PaymentIntent intent;
            Order order;

            // intent = (PaymentIntent)stripeEvent.Data.Object;
            // _logger.LogInformation("Payment succeeded: {intent.Id}", intent.Id);
            // order = await _paymentService.UpdateOrderPaymentStatus(intent.Id, order );

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                    _logger.LogInformation("Order updated to payment received: {order.Id}", order.Id);
                    break;

                case "payment_intent.payment_failed":
                intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation("Payment failed: {intent.Id}", intent.Id);
                    order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                    _logger.LogInformation("Order updated to payment failed: {order.Id}", order.Id);
                    break;
            }
            return new EmptyResult();
        }
    }
}