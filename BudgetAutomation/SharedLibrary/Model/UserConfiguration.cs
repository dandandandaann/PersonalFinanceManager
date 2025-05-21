using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace SharedLibrary.Model;

public class UserConfiguration
{
    [DynamoDBProperty("spreadsheetId")]
    public string SpreadsheetId { get; set; }
}