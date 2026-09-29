using Microsoft.AspNetCore.Identity;

namespace ProjetFullstack.Models;

public class ProductionLine
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<Machine> Machines { get; set; } = new List<Machine>();
    public List<Product> Products { get; set; } = new List<Product>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

}
