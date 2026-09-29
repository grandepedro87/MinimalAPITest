using Microsoft.EntityFrameworkCore;
using MinimalAPITest.Dominio.DTOs;
using MinimalAPITest.Infraestrutura.DB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBcontexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (MinimalAPITest.Dominio.DTOs.Login.LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "123456")
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});


app.Run();

