using DatabazovyProjekt.DAO;
using DatabazovyProjekt.Entities;
using DatabazovyProjekt;

public class RequestHandling
{
    public static Contact SaveContact(Request p)
    {
        if (string.IsNullOrWhiteSpace(p.Email))
            throw new ArgumentException("Email nesmí být prázdný");

        ContactDAO dao = new();
        Contact? contact = dao.GetByEmail(p.Email);

        if (contact == null)
        {
            contact = new Contact(p.Jmeno, p.Prijmeni, p.Email);
            dao.Create(contact);
        }

        p.Contact = contact;
        return contact;
    }

    public static void CreateRequestDB(Request p)
    {
        if (p.Contact == null || p.Contact.Id <= 0)
            throw new InvalidOperationException("Neplatný kontakt");

        if (string.IsNullOrWhiteSpace(p.TextZpravy))
            throw new ArgumentException("Text požadavku nesmí být prázdný");

        p.RequestStatus = new RequestStatus((int)State.Novy, "novy");

        ServiceRequest sr = new(
            p.Contact.Id,
            p.RequestStatus.Id,
            p.TextZpravy
        );

        ServiceRequestDAO dao = new();
        dao.Create(sr);

        p.ServiceRequest = sr;
        p.Stav = State.Novy;
    }

    public static void AssignAdminToRequest(Request p, Administrator admin)
    {
        if (p.ServiceRequest == null || p.ServiceRequest.Id <= 0)
            throw new InvalidOperationException("Požadavek není uložen");

        if (admin == null || admin.Id <= 0)
            throw new InvalidOperationException("Neplatný admin");

        if (p.Stav != State.Novy)
            throw new InvalidOperationException("Požadavek nelze převzít");

        RequestProcessing rp = new(admin.Id, p.ServiceRequest.Id);
        new RequestProcessingDAO().Create(rp);

        p.Administrator = admin;
        p.RequestProcessing = rp;
        p.Stav = State.ResiSe;
    }

    public static void FinishRequest(Request p, string responseText)
    {
        if (p.RequestProcessing == null || p.RequestProcessing.Id <= 0)
            throw new InvalidOperationException("Požadavek není zpracováván");

        if (!ZmenaStavuDto.JePlatnaZmena(p.Stav, State.Uzavreny))
            throw new InvalidOperationException("Neplatná změna stavu");

        new RequestProcessingDAO().Finish(p.RequestProcessing.Id, responseText);
        ChangeRequestState(p, State.Uzavreny);
    }

    public static void CancelRequest(Request p)
    {
        if (p.Stav == State.Uzavreny)
            throw new InvalidOperationException("Uzavřený požadavek nelze stornovat");

        ChangeRequestState(p, State.Stornovany);
    }
    public static void ChangeRequestState(Request p, State newState)
    {
        if (p.ServiceRequest == null)
            throw new InvalidOperationException("Request není uložen v DB");

        if (!ZmenaStavuDto.JePlatnaZmena(p.Stav, newState))
            throw new InvalidOperationException(
                $"Neplatná změna stavu: {p.Stav} → {newState}"
            );

        ServiceRequestDAO dao = new();
        dao.UpdateStatus(p.ServiceRequest.Id, newState);

        p.Stav = newState;
    }




}
