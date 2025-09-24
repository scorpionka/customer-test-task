using System.ComponentModel.DataAnnotations;

namespace AppBL.Configuration;

public sealed class RedisCacheOptions
{
    [Range(1, 86400)]
    public int CacheDurationSeconds { get; set; } = 300;
    [Required]
    public string Configuration { get; set; } = string.Empty;
}