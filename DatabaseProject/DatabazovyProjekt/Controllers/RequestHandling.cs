using DatabazovyProjekt.DAO;
using DatabazovyProjekt.Entities;
using DatabazovyProjekt;
using System.Data.SqlClient;

/// <summary>
/// Zpracování požadavků: ukládání kontaktů, přiřazování adminů, dokončení a storno requestů.
/// </summary>
public class RequestHandling
{
    /// <summary>
    /// Uloží kontakt do DB, pokud neexistuje, a vrátí instanci Contact.
    /// </summary>
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
        Console.WriteLine($"CONTACT ID: {contact.Id}");
        return contact;
    }

    /// <summary>
    /// Vytvoří záznam ServiceRequest v DB pro daný Request.
    /// </summary>
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

    /// <summary>
    /// Přiřadí admina k požadavku a změní jeho stav na 'Řeší se'.
    /// </summary>
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

    /// <summary>
    /// Dokončí request s odpovědí admina.
    /// </summary>
    public static void FinishRequest(int requestId, int adminId, string responseText)
    {
        using var conn = DatabaseFactory.CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            // ověření přiřazení
            string checkAssignment = @"
                    select count(*) from requestprocessing
                    where request_id = @requestId
                      and admin_id = @adminId
                      and ended_date is null
                ";

            using var checkCmd = new SqlCommand(checkAssignment, conn, transaction);
            checkCmd.Parameters.AddWithValue("@requestId", requestId);
            checkCmd.Parameters.AddWithValue("@adminId", adminId);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
                throw new InvalidOperationException("Požadavek není přiřazen tomuto adminovi nebo je již uzavřen.");

            // update RequestProcessing
            string updateProcessingSql = @"
                    update requestprocessing
                    set ended_date = getdate(),
                        response_text = @response
                    where request_id = @requestId
                      and admin_id = @adminId
                      and ended_date is null
                ";

            using var cmd = new SqlCommand(updateProcessingSql, conn, transaction);
            cmd.Parameters.AddWithValue("@requestId", requestId);
            cmd.Parameters.AddWithValue("@adminId", adminId);
            cmd.Parameters.AddWithValue("@response", responseText);

            int affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new InvalidOperationException("Nelze dokončit požadavek.");

            // update ServiceRequest stav
            string updateRequestSql = @"
                    update servicerequest
                    set status_id = @statusId
                    where id = @requestId
                ";

            using var cmd2 = new SqlCommand(updateRequestSql, conn, transaction);
            cmd2.Parameters.AddWithValue("@requestId", requestId);
            cmd2.Parameters.AddWithValue("@statusId", (int)State.Uzavreny);
            cmd2.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Zruší request, nastaví stav na 'Storno' a ukončí RequestProcessing.
    /// </summary>
    public static void CancelRequest(int requestId, int adminId)
    {
        using var conn = DatabaseFactory.CreateConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        try
        {
            string checkAssignment = @"
                    select count(*) from requestprocessing
                    where request_id = @requestId
                      and admin_id = @adminId
                      and ended_date is null
                ";

            using var checkCmd = new SqlCommand(checkAssignment, conn, transaction);
            checkCmd.Parameters.AddWithValue("@requestId", requestId);
            checkCmd.Parameters.AddWithValue("@adminId", adminId);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
                throw new InvalidOperationException("Požadavek není přiřazen tomuto adminovi nebo je již uzavřen.");

            string updateProcessing = @"
                    update requestprocessing
                    set ended_date = getdate()
                    where request_id = @requestId
                      and admin_id = @adminId
                      and ended_date is null
                ";

            using var cmd = new SqlCommand(updateProcessing, conn, transaction);
            cmd.Parameters.AddWithValue("@requestId", requestId);
            cmd.Parameters.AddWithValue("@adminId", adminId);

            int affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new InvalidOperationException("Nelze zrušit požadavek.");

            string updateRequest = @"
                    update servicerequest
                    set status_id = @statusId
                    where id = @requestId
                ";

            using var cmd2 = new SqlCommand(updateRequest, conn, transaction);
            cmd2.Parameters.AddWithValue("@requestId", requestId);
            cmd2.Parameters.AddWithValue("@statusId", (int)State.Storno);
            cmd2.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Změní stav requestu v DB a v objektu Request.
    /// </summary>
    public static void ChangeRequestState(Request p, State newState)
    {
        if (p.ServiceRequest == null)
            throw new InvalidOperationException("Request není uložen v DB");

        if (!ZmenaStavuDto.JePlatnaZmena(p.Stav, newState))
            throw new InvalidOperationException($"Neplatná změna stavu: {p.Stav} → {newState}");

        ServiceRequestDAO dao = new();
        dao.UpdateStatus(p.ServiceRequest.Id, newState);

        p.Stav = newState;
    }
}
