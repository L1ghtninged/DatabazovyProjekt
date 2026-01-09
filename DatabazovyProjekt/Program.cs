using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using DatabazovyProjekt.DAO;
using DatabazovyProjekt.Entities;
using DatabazovyProjekt.DTO;

namespace DatabazovyProjekt
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var pozadavky = new List<Request>();
            var admins = new List<Administrator>();
            
            int nextId = 1;
            var builder = WebApplication.CreateBuilder(args);

            DatabaseFactory.Init(builder.Configuration);
            builder.WebHost.UseUrls("http://localhost:8080");
            var app = builder.Build();
            
            app.MapGet("/", () => "API běží");
            app.MapPost("/api/requests", (CreateRequestDto dto) =>
            {
                APIController.PostRequest(dto);
            });
            app.MapPost("/api/auth/login", (Admin dto) =>
            {
                APIController.PostRequest(dto);
            });

            app.MapGet("/api/pozadavky", () =>
            {

                return pozadavky;
            });
            app.MapGet("/api/pozadavky/{id}", (int id) =>
            {
                var p = pozadavky.FirstOrDefault(x => x.Id == id);
                return p is null ? Results.NotFound() : Results.Ok(p);
            });
            app.MapPut("/api/pozadavky/{id}/stav", (int id, ZmenaStavuDto dto) =>
            {
                var p = pozadavky.FirstOrDefault(x => x.Id == id);
                if (p == null)
                    return Results.NotFound();

                if (!Enum.IsDefined(typeof(State), dto.State))
                    return Results.BadRequest("Neplatný stav");

                if (!ZmenaStavuDto.JePlatnaZmena(p.Stav, dto.State))
                    return Results.BadRequest($"Nelze změnit stav z {p.Stav} na {dto.State}");

                p.Stav = dto.State;
                return Results.Ok(p);
            });

            app.MapDelete("/api/pozadavky/{id}", (int id) =>
            {
                var p = pozadavky.FirstOrDefault(x => x.Id == id);
                if (p == null)
                    return Results.NotFound();

                pozadavky.Remove(p);
                return Results.NoContent();
            });

            app.Run();
        }
        static void TryCreateTestAdmin()
        {
            try
            {
                using var conn = DatabaseFactory.CreateConnection();
                conn.Open();
                Console.WriteLine("Připojení k DB úspěšné!");

                var adminDAO = new AdministratorDAO();
                var admin = new Administrator("Dan", "Oujeský", "email@dc.cz");
                adminDAO.Delete(1);
                Console.WriteLine("Testovací admin vložen.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chyba při testu DB: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }
        }
    }

}