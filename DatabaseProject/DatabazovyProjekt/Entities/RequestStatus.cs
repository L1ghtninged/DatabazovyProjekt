namespace DatabazovyProjekt.Entities
{
    public class RequestStatus
    {
        public int Id { get; set; }
        public string StatusText { get; set; }

        public RequestStatus(int id, string statusText)
        {
            Id = id;
            StatusText = statusText;
        }

        public RequestStatus(string statusText)
        {
            StatusText = statusText;
        }
    }
}
