namespace Infrastructure.Integration.Services.JellyFin.Models;

public class JellyFinSearchResponse
{
    public List<JellyFinSearchHint> SearchHints { get; set; } = new();
    public int TotalRecordCount { get; set; }
}

public class JellyFinSearchHint
{
    public string ItemId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ProductionYear { get; set; }
    public string Type { get; set; } = string.Empty;
}