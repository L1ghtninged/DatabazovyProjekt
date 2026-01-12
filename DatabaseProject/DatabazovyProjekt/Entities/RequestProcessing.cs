namespace DatabazovyProjekt.Entities
{
    public class RequestProcessing
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public int RequestId { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime? EndedDate { get; set; }
        public string? ResponseText { get; set; }

        public RequestProcessing(
            int adminId,
            int requestId,
            DateTime startedDate,
            DateTime? endedDate,
            string? responseText)
        {
            AdminId = adminId;
            RequestId = requestId;
            StartedDate = startedDate;
            EndedDate = endedDate;
            ResponseText = responseText;
        }
        public RequestProcessing(
            int id,
            int adminId,
            int requestId,
            DateTime startedDate,
            DateTime? endedDate,
            string? responseText)
        {
            Id = id;
            AdminId = adminId;
            RequestId = requestId;
            StartedDate = startedDate;
            EndedDate = endedDate;
            ResponseText = responseText;
        }
        public RequestProcessing(int adminId, int requestId)
        {
            AdminId = adminId;
            RequestId = requestId;
        }
    }
}
