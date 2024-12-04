public class ProductService
{
    private readonly List<Product> _products;

    public ProductService()
    {
        _products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 100000 },
            new Product { Id = 2, Name = "Smartphone", Price = 499.99m, Stock = 100000 },
            new Product { Id = 3, Name = "Headphones", Price = 99.99m, Stock = 100000 }
        };
    }

    public List<Product> GetAllProducts() => _products;

    public Product GetProduct(int id) => _products.FirstOrDefault(p => p.Id == id);

    public bool UpdateStock(int productId, int quantity)
    {
        var product = GetProduct(productId);
        if (product == null || product.Stock < quantity) return false;
        
        product.Stock -= quantity;
        return true;
    }
} 