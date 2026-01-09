using DatabazovyProjekt.DTO;

namespace DatabazovyProjekt
{
    public static class APIController
    {

        public static IResult PostRequest(CreateRequestDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName))
                    return Results.BadRequest("First name is required");

                if (string.IsNullOrWhiteSpace(dto.LastName))
                    return Results.BadRequest("Last name is required");

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return Results.BadRequest("Email is required");

                if (string.IsNullOrWhiteSpace(dto.Message))
                    return Results.BadRequest("Message is required");

                Request request = new Request
                {
                    Jmeno = dto.FirstName,
                    Prijmeni = dto.LastName,
                    Email = dto.Email,
                    TextZpravy = dto.Message
                };

                RequestHandling.SaveContact(request);
                RequestHandling.CreateRequestDB(request);

                return Results.Created(
                    $"/api/requests/{request.ServiceRequest!.Id}",
                    new RequestCreatedDto
                    {
                        RequestId = request.ServiceRequest.Id,
                        Status = "novy",
                        CreatedDate = request.ServiceRequest.CreatedDate
                    }
                );
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
            catch (Exception)
            {
                return Results.Problem("Internal server error");
            }
        }



    }
}
