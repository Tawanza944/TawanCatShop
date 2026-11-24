using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

[Table("admin_activity")]
public class AdminActivity
{
    public long Id { get; set; }
    [MaxLength(255)] public string Actor { get; set; } = "";
    [MaxLength(255)] public string Description { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}
