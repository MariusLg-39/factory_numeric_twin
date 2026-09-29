using Microsoft.AspNetCore.Identity;

namespace ProjetFullstack.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ProductionLineId { get; set; }
}
