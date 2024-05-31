using SharedLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedLibrary.Dto
{
    public class RemoveExpenseResponse : ApiResponse
    {
        [JsonPropertyName("expense")]
        public Expense? expense { get; set; }
    }
}
