using Microsoft.AspNetCore.Identity;

namespace ProjetFullstack.Models;

public class ProductionLine
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<Machine> Machines { get; set; }
    public Product Product { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

}
