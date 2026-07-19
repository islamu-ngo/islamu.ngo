// ABOUTME: Model for editing a single custom property value in the Blazor UI.
// ABOUTME: Keeps generic value editor state without binding to generated transport DTOs.

using System.ComponentModel.DataAnnotations;

namespace Explore.Blazor.Client.Models.CustomProperties;

public class CustomPropertyValueModel
{
    public Guid? Id { get; set; }
    public Guid DefinitionId { get; set; }
    public Guid ParentId { get; set; }
    public int Ordinal { get; set; }

    public string? TextValue { get; set; }
    public double? NumberValue { get; set; }
    public bool? BooleanValue { get; set; }
    public DateTimeOffset? DateTimeValue { get; set; }
    public Guid? OptionId { get; set; }
}
