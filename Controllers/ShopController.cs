using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ShopController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ApplicationDbContext _dbContext;

    public ShopController(ProductService productService, IRabbitMQService rabbitMQService, ApplicationDbContext dbContext)
    {
        _productService = productService;
        _rabbitMQService = rabbitMQService;
        _dbContext = dbContext;
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        return Ok(_productService.GetAllProducts());
    }

    [HttpPost("order")]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        var product = _productService.GetProduct(request.ProductId);
        if (product == null)
            return NotFound("Product not found");

        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than 0");
        }

        if (!_productService.UpdateStock(request.ProductId, request.Quantity))
            return BadRequest("Insufficient stock");

        var order = new Order
        {
            Id = new Random().Next(1000, 9999),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UserEmail = request.UserEmail,
            TotalPrice = product.Price * request.Quantity,
            OrderDate = DateTime.UtcNow
        };

        _rabbitMQService.PublishMessage("order_processing", order);

        return Ok(new { Message = "Order created successfully", OrderId = order.Id });
    }

    [HttpPost("payment")]
    public IActionResult ProcessPayment([FromBody] PaymentRequest request)
    {
        // Step 1: Send payment to Payment Queue
        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Status = "Pending"
        };

        // Save payment to database
        _dbContext.Payments.Add(payment);
        _dbContext.SaveChanges();

        // Publish to Payment Queue
        _rabbitMQService.PublishMessage("payment_queue", payment);

        return Ok(new { Message = "Payment processing started", PaymentId = payment.Id });
    }

    // Step 2: Process Payment and Notify
    [HttpPost("notify")]
    public IActionResult NotifyPayment([FromBody] NotificationRequest request)
    {
        // Step 3: Send notification to Notification Queue
        var notification = new Notification
        {
            OrderId = request.OrderId,
            UserEmail = request.UserEmail,
            Message = "Your payment has been processed."
        };

        // Save notification to database
        _dbContext.Notifications.Add(notification);
        _dbContext.SaveChanges();

        // Publish to Notification Queue
        _rabbitMQService.PublishMessage("notification_queue", notification);

        return Ok(new { Message = "Notification sent", NotificationId = notification.Id });
    }
}

public class OrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string UserEmail { get; set; }
} 