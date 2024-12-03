using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ShopController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly IRabbitMQService _rabbitMQService;

    public ShopController(ProductService productService, IRabbitMQService rabbitMQService)
    {
        _productService = productService;
        _rabbitMQService = rabbitMQService;
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
}

public class OrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string UserEmail { get; set; }
} 