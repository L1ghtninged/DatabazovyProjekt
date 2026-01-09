namespace DatabazovyProjekt.DTO
{
    public class AdminStatisticsDto
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; } = "";
        public string Email { get; set; } = "";
        public int TotalRequests { get; set; }
        public int FinishedRequests { get; set; }
        public double? AvgProcessingMinutes { get; set; }
    }

}
