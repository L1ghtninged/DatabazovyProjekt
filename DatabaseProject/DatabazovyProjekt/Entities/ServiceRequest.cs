namespace DatabazovyProjekt.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public int StatusId { get; set; }
        public string RequestText { get; set; }
        public DateTime CreatedDate { get; set; }

        public ServiceRequest(
            int contactId,
            int statusId,
            string requestText,
            DateTime createdDate)
        {
            ContactId = contactId;
            StatusId = statusId;
            RequestText = requestText;
            CreatedDate = createdDate;
        }
        public ServiceRequest(
            int id,
            int contactId,
            int statusId,
            string requestText,
            DateTime createdDate)
        {
            Id = id;
            ContactId = contactId;
            StatusId = statusId;
            RequestText = requestText;
            CreatedDate = createdDate;
        }
        public ServiceRequest(
            int contactId,
            int statusId,
            string requestText)
        {
            ContactId = contactId;
            StatusId = statusId;
            RequestText = requestText;
        }
    }
}
