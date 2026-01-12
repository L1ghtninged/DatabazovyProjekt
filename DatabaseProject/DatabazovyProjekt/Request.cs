using DatabazovyProjekt.Entities;
using DatabazovyProjekt.DAO;

namespace DatabazovyProjekt
{
    public class Request
    {
        public int Id { get; set; }
        public string Jmeno { get; set; } = "";
        public string Prijmeni { get; set; } = "";
        public string Email { get; set; } = "";
        public string TextZpravy { get; set; } = "";
        public DateTime Vytvoren { get; set; }

        public State Stav { get; set; }
        public Contact? Contact { get; set; }
        public Administrator? Administrator{ get; set; }
        public RequestStatus? RequestStatus{ get; set; }
        public ServiceRequest? ServiceRequest { get; set; }
        public RequestProcessing? RequestProcessing { get; set; }
    }
    public enum State
    {
        Novy = 1,
        ResiSe = 2,
        Uzavreny = 3,
        Storno = 4
    }
    public class ZmenaStavuDto
    {
        public State State { get; set; }

        public static bool JePlatnaZmena(State aktualni, State novy)
        {
            return (aktualni == State.Novy && novy == State.ResiSe)
                || (aktualni == State.ResiSe && novy == State.Uzavreny);
        }

    }

}