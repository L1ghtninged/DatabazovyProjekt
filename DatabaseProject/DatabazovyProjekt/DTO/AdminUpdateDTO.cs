namespace DatabazovyProjekt.DTO
{
    public class AdminUpdateDTO
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool ReleaseActiveRequests { get; set; }
    }

}
