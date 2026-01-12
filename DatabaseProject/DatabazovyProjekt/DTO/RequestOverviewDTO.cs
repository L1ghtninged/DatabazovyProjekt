namespace DatabazovyProjekt.DTO
{
    public class RequestOverviewDTO
    {
        public int RequestId { get; set; }
        public string ContactFirstName { get; set; } = "";
        public string ContactLastName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string RequestText { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string? AssignedAdminFirstName { get; set; }
        public string? AssignedAdminLastName { get; set; }
        public string? AssignedAdminEmail { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? EndedDate { get; set; }
        public string? ResponseText { get; set; }
    }
}