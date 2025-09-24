using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApiTestApp.ApiModels;

public sealed class PagingQuery : IValidatableObject
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    [Range(1, int.MaxValue)]
    public int? Page { get; set; }

    [Range(1, MaxPageSize)]
    public int? PageSize { get; set; }

    [JsonIgnore]
    internal int? EffectivePage => Page;
    [JsonIgnore]
    internal int? EffectivePageSize => Page.HasValue ? (PageSize ?? DefaultPageSize) : null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Page.HasValue && PageSize is 0)
            yield return new ValidationResult("PageSize must be greater than 0", [nameof(PageSize)]);
    }
}
