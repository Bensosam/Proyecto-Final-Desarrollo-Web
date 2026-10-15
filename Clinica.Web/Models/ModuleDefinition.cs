namespace Clinica.Web.Models;

public enum FieldKind
{
    Text,
    TextArea,
    Integer,
    Decimal,
    Date,
    DateTime,
    Time,
    Boolean,
    Password,
    Select
}

public enum ColumnFormat
{
    Text,
    Currency,
    Date,
    DateTime,
    Boolean,
    Status,
    Number
}

public sealed class ColumnDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public ColumnFormat Format { get; init; } = ColumnFormat.Text;
}

public sealed class FieldDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public FieldKind Kind { get; init; } = FieldKind.Text;
    public bool Required { get; init; }
    public string? Placeholder { get; init; }
    public string? DefaultValue { get; init; }
    public string? OptionsEndpoint { get; init; }
    public string? OptionValueKey { get; init; }
    public string? OptionTextKey { get; init; }
    public string? OptionTextKey2 { get; init; }
    public bool HiddenOnCreate { get; init; }
    public bool HiddenOnEdit { get; init; }
    public bool ReadOnlyOnEdit { get; init; }
}

public sealed class ModuleDefinition
{
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Singular { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string IdField { get; init; } = string.Empty;
    public string Icon { get; init; } = "grid";
    public bool CanCreate { get; init; } = true;
    public bool CanEdit { get; init; } = true;
    public bool CanDelete { get; init; } = true;
    public List<ColumnDefinition> Columns { get; init; } = [];
    public List<FieldDefinition> Fields { get; init; } = [];
}

public sealed class SelectOption
{
    public string Value { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
