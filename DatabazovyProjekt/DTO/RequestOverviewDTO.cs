namespace DatabazovyProjekt.DTO
{
    public class RequestOverviewDto
    {
        public int RequestId { get; set; }
        public string ContactName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public int ProcessingCount { get; set; }
    }

}
