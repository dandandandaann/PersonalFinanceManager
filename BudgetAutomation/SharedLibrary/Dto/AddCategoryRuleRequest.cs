namespace SharedLibrary.Dto
{
    public class AddCategoryRuleRequest
    {
        public string SpreadsheetId { get; set; }
        public string Category { get; set; }
        public string DescriptionPattern { get; set; }
    }
} 