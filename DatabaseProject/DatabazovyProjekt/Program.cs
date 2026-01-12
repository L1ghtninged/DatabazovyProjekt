using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using DatabazovyProjekt.DAO;
using DatabazovyProjekt.Entities;
using DatabazovyProjekt.DTO;
using DatabazovyProjekt.Controllers;

namespace DatabazovyProjekt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });


            DatabaseFactory.Init(builder.Configuration);
            DatabaseInitializer.InitializeDatabase();
            builder.WebHost.UseUrls("http://localhost:8080");
            var app = builder.Build();
            app.UseCors();

            app.MapGet("/", () => "API běží");
            app.MapPost("/api/requests", (CreateRequestDTO dto) =>
            {
                return APIController.PostRequest(dto);
            });
            app.MapPost("/api/admin/register", (AdminCreateDTO dto) =>
            {
                return APIController.RegisterAdmin(dto);
            });
            app.MapPost("/api/admin/login", (AdminLoginDTO dto) =>
            {
                return APIController.LoginAdmin(dto);
            });
            app.MapPut("/api/admin/{id}", (int id, AdminUpdateDTO dto) =>
            {
                return APIController.UpdateAdmin(id, dto);
            });
            app.MapDelete("/api/admin/{id}", (int id) =>
            {
                return APIController.DeleteAdmin(id);
            });
            app.MapGet("/api/admin/{id}/requests", (int id) =>
            {
                return APIController.GetAdminRequests(id);
            });

            app.MapPost("/api/requests/{id}/assign", (int id, AssignRequestDTO dto) =>
            {
                return APIController.AssignRequest(id, dto);
            });
            app.MapPost("/api/requests/{id}/finish", (int id, FinishRequestDTO dto) =>
            {
                return APIController.FinishRequest(id, dto);
            });
            app.MapPost("/api/requests/{id}/cancel", (int id, CancelRequestDTO dto) =>
            {
                return APIController.CancelRequest(id, dto);
            });
            app.MapGet("/api/requests/overview", () =>
            {
                return APIController.GetRequestsOverview();
            });

            app.MapGet("/api/requests/overview/{status}", (string status) =>
            {
                return APIController.GetRequestsByStatus(status);
            });

            app.MapGet("/api/admin/statistics", () =>
            {
                return APIController.GetAdminStatistics();
            });

            app.MapGet("/api/admin/{id}/statistics", (int id) =>
            {
                return APIController.GetAdminStatisticsById(id);
            });
            app.MapPost("/api/import", async (HttpContext context) => {
                var form = await context.Request.ReadFormAsync();
                var file = form.Files["file"];
                var table = form["table"].ToString();
                var hasHeader = bool.Parse(form["hasHeader"].ToString() ?? "true");

                return await ImportController.ImportCsv(file, table, hasHeader);
            });


            app.Run();
        }
        
    }

}