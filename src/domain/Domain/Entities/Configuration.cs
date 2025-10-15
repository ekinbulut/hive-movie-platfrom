using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("configurations")]
public partial class Configuration
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [Column("settings", TypeName = "jsonb")]
    public Settings Settings { get; set; } = new Settings();
}

public class Settings
{
    public string JellyFinServer { get; set; }
    public string JellyFinApiKey { get; set; }
    public string MediaFolder { get; set; }
}