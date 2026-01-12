using DatabazovyProjekt.DAO;
using DatabazovyProjekt.DTO;
using DatabazovyProjekt.Entities;

namespace DatabazovyProjekt.Controllers
{
    public static class APIController
    {
        /// <summary>
        /// POST api/requests - Creates a new service request.
        /// </summary>
        public static IResult PostRequest(CreateRequestDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName)) return Results.BadRequest("First name is required");
                if (string.IsNullOrWhiteSpace(dto.LastName)) return Results.BadRequest("Last name is required");
                if (string.IsNullOrWhiteSpace(dto.Email)) return Results.BadRequest("Email is required");
                if (string.IsNullOrWhiteSpace(dto.Message)) return Results.BadRequest("Message is required");

                Request request = new()
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
                    new RequestCreatedDTO
                    {
                        RequestId = request.ServiceRequest.Id,
                        Status = "novy",
                        CreatedDate = request.ServiceRequest.CreatedDate
                    }
                );
            }
            catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// POST api/admin/register - Registers a new administrator.
        /// </summary>
        public static IResult RegisterAdmin(AdminCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName)) return Results.BadRequest("First name is required");
            if (string.IsNullOrWhiteSpace(dto.LastName)) return Results.BadRequest("Last name is required");
            if (string.IsNullOrWhiteSpace(dto.Email)) return Results.BadRequest("Email is required");

            try
            {
                bool existed = AdminHandling.CheckAdminExists(dto.Email);
                Administrator admin = AdminHandling.RegisterAdmin(dto);

                var response = new AdminResponseDTO
                {
                    Id = admin.Id,
                    FullName = $"{admin.FirstName} {admin.LastName}",
                    Email = admin.Email
                };

                return existed ? Results.Ok(response) : Results.Created($"/api/admin/{admin.Id}", response);
            }
            catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// POST api/admin/login - Logs in an administrator by email.
        /// </summary>
        public static IResult LoginAdmin(AdminLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email)) return Results.BadRequest("Email is required");

            try
            {
                Administrator admin = AdminHandling.LoginAdmin(dto.Email);

                return Results.Ok(new
                {
                    admin.Id,
                    admin.FirstName,
                    admin.LastName,
                    admin.Email
                });
            }
            catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// PUT api/admin/{id} - Updates administrator info.
        /// </summary>
        public static IResult UpdateAdmin(int id, AdminUpdateDTO dto)
        {
            if (id <= 0) return Results.BadRequest("Invalid admin id");
            if (string.IsNullOrWhiteSpace(dto.Email)) return Results.BadRequest("Email is required");

            try
            {
                AdminHandling.UpdateAdmin(id, dto);
                return Results.Ok(new { message = "Admin updated" });
            }
            catch (InvalidOperationException ex) { return Results.NotFound(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// DELETE api/admin/{id} - Deletes an administrator and releases their requests.
        /// </summary>
        public static IResult DeleteAdmin(int id)
        {
            if (id <= 0) return Results.BadRequest("Invalid admin id");

            try
            {
                AdminHandling.RemoveAdmin(id);
                return Results.Ok(new { message = "Admin deleted" });
            }
            catch (InvalidOperationException ex) { return Results.NotFound(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// GET api/admin/{id}/requests - Returns active requests assigned to the admin.
        /// </summary>
        public static IResult GetAdminRequests(int id)
        {
            if (id <= 0) return Results.BadRequest("Invalid admin id");

            try
            {
                RequestProcessingDAO dao = new();
                var requests = dao.GetActiveRequestsByAdmin(id);

                return Results.Ok(requests.Select(r => new
                {
                    r.Id,
                    r.CreatedDate,
                    r.StatusId,
                    r.ContactId
                }));
            }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// POST api/requests/{id}/assign - Assigns a request to an admin.
        /// </summary>
        public static IResult AssignRequest(int id, AssignRequestDTO dto)
        {
            if (id <= 0 || dto.AdminId <= 0) return Results.BadRequest("Invalid id");

            try
            {
                ServiceRequestDAO srDao = new();
                AdministratorDAO adminDao = new();

                ServiceRequest? sr = srDao.GetById(id);
                if (sr == null) return Results.NotFound("Request not found");

                Administrator? admin = adminDao.GetById(dto.AdminId);
                if (admin == null) return Results.NotFound("Admin not found");

                Request p = new() { ServiceRequest = sr, Stav = State.Novy };
                RequestHandling.AssignAdminToRequest(p, admin);

                return Results.Ok(new { message = "Request assigned", requestId = id, adminId = dto.AdminId });
            }
            catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// POST api/requests/{id}/finish - Marks a request as finished by admin.
        /// </summary>
        public static IResult FinishRequest(int id, FinishRequestDTO dto)
        {
            if (id <= 0 || dto.AdminId <= 0) return Results.BadRequest("Invalid id");
            if (string.IsNullOrWhiteSpace(dto.ResponseText)) return Results.BadRequest("Response text is required");

            try
            {
                RequestHandling.FinishRequest(id, dto.AdminId, dto.ResponseText);
                return Results.Ok(new { message = "Request finished", requestId = id });
            }
            catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// POST api/requests/{id}/cancel - Cancels a request by admin.
        /// </summary>
        public static IResult CancelRequest(int id, CancelRequestDTO dto)
        {
            if (id <= 0 || dto.AdminId <= 0) return Results.BadRequest("Invalid id");

            try
            {
                RequestHandling.CancelRequest(id, dto.AdminId);
                return Results.Ok(new { message = "Request cancelled", requestId = id });
            }
            catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            catch { return Results.Problem("Internal server error"); }
        }

        /// <summary>
        /// GET api/requests/overview - Returns all requests.
        /// </summary>
        public static IResult GetRequestsOverview()
        {
            try
            {
                RequestOverviewDAO dao = new();
                var requests = dao.GetAllRequests();
                return Results.Ok(requests);
            }
            catch (Exception ex) { return Results.Problem($"Internal server error: {ex.Message}"); }
        }

        /// <summary>
        /// GET api/requests/overview/{status} - Returns requests filtered by status.
        /// </summary>
        public static IResult GetRequestsByStatus(string status)
        {
            try
            {
                RequestOverviewDAO dao = new();
                var requests = dao.GetRequestsByStatus(status);
                return Results.Ok(requests);
            }
            catch (Exception ex) { return Results.Problem($"Internal server error: {ex.Message}"); }
        }

        /// <summary>
        /// GET api/admin/statistics - Returns statistics for all admins.
        /// </summary>
        public static IResult GetAdminStatistics()
        {
            try
            {
                AdminStatisticsDAO dao = new();
                var stats = dao.GetAllStatistics();
                return Results.Ok(stats);
            }
            catch (Exception ex) { return Results.Problem($"Internal server error: {ex.Message}"); }
        }

        /// <summary>
        /// GET api/admin/{id}/statistics - Returns statistics for a specific admin.
        /// </summary>
        public static IResult GetAdminStatisticsById(int id)
        {
            if (id <= 0) return Results.BadRequest("Invalid admin id");

            try
            {
                AdminStatisticsDAO dao = new();
                var stats = dao.GetStatisticsByAdminId(id);
                if (stats == null) return Results.NotFound($"Statistics not found for admin id: {id}");
                return Results.Ok(stats);
            }
            catch (Exception ex) { return Results.Problem($"Internal server error: {ex.Message}"); }
        }
    }
}
