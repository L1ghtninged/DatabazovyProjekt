namespace DatabazovyProjekt.DTO
{
    public class AdminStatisticsDTO
    {
        public int AdminId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string AdminRole { get; set; } = "";
        public int NewRequests { get; set; }
        public int AssignedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int CancelledRequests { get; set; }
        public int TotalProcessed { get; set; }
        public int AvgProcessingTimeMinutes { get; set; }
    }
}