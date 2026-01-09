using DatabazovyProjekt.DAO;
using DatabazovyProjekt.Entities;
using DatabazovyProjekt;
using System.Data.SqlClient;

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
        ChangeRequestState(p, State.ResiSe);
    }

    public static void FinishRequest(int requestId,int adminId,string responseText)
    {
        using var conn = DatabaseFactory.CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            string updateProcessingSql = @"
            update requestprocessing
            set ended_date = getdate(),
                response_text = @response
            where request_id = @requestId
              and admin_id = @adminId
              and ended_date is null;
        ";

            using (var cmd = new SqlCommand(updateProcessingSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@requestId", requestId);
                cmd.Parameters.AddWithValue("@adminId", adminId);
                cmd.Parameters.AddWithValue("@response", responseText);

                int affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                    throw new InvalidOperationException("Požadavek není přiřazen tomuto adminovi.");
            }

            string updateRequestSql = @"
            update servicerequest
            set status_id = @statusId
            where id = @requestId;
        ";

            using (var cmd = new SqlCommand(updateRequestSql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@requestId", requestId);
                cmd.Parameters.AddWithValue("@statusId", (int)State.Uzavreny);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public static void CancelRequest(Request p)
    {
        if (p.Stav == State.Uzavreny)
            throw new InvalidOperationException("Uzavřený požadavek nelze stornovat");

        ChangeRequestState(p, State.Storno);
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
