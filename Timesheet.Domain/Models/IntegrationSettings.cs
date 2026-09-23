namespace Timesheet.Domain.Models;

public enum DataSourceType { Csv, Api }

public class DataSourceSettings
{
    public DataSourceType SourceType { get; set; } = DataSourceType.Csv;
    public string ApiUrl { get; set; } = "";
    public string ApiToken { get; set; } = "";
}

public class IntegrationSettings
{
    public DataSourceSettings Employee { get; set; } = new();
    public DataSourceSettings Swipe { get; set; } = new();
    public DataSourceSettings Leave { get; set; } = new();
    public DataSourceSettings Holiday { get; set; } = new();
}
