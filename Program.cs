using Microsoft.EntityFrameworkCore;
using MinimalAPITest.Dominio.DTOs;
using MinimalAPITest.Dominio.Interfaces;
using MinimalAPITest.Infraestrutura.DB;
using MinimalAPITest.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalAPITest.Dominio.ModelViews;
using Microsoft.EntityFrameworkCore.Storage;
using MinimalAPITest.Dominio.Entidades;
using MinimalAPITest.Dominio.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if(string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdmServico, AdmServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Jwt",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta maneira: Bearar {Seu token}"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});


builder.Services.AddDbContext<DBcontexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
    if(string.IsNullOrEmpty(key)) return string.Empty;
    var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfl", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] MinimalAPITest.Dominio.DTOs.Login.LoginDTO loginDTO, IAdmServico admServico) =>
{
    var adm = admServico.Login(loginDTO);
    if (adm != null){
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdmLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdmServico admServico) =>
{
    var adms = new List<AdmModelView>();
    var administradores = admServico.Todos(pagina ?? 1);
    foreach(var adm in administradores)
    {
        adms.Add(new AdmModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Administradores");

app.MapGet("/Administradores/{id}", ([FromRoute]int id, IAdmServico admServico) =>
{
    var administrador = admServico.BuscaPorId(id);

    if(administrador == null) return Results.NotFound();

    return Results.Ok(new AdmModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdmServico admServico) =>
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

if(string.IsNullOrEmpty(administradorDTO.Email))
    validacao.Mensagens.Add("Email não pode ser vazio");
if(string.IsNullOrEmpty(administradorDTO.Senha))
    validacao.Mensagens.Add("Senha não pode ser vazia");
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);


    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    admServico.Incluir(administrador);

    return Results.Created("/administradores", new AdmModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Administradores");
#endregion


#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao{
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio");

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A marca não pode ser vazio");

    if(veiculoDTO.Ano < 1950)
        validacao.Mensagens.Add("Veiculo muito antigo, somente aceito veiculos a partir de 1950");

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);


    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm, Editor"}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);


    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromQuery]int id, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.BuscaPorId(id);

    if(veiculos == null) return Results.NotFound();

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm, Editor"}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromQuery]int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.BuscaPorId(id);
    if(veiculos == null) return Results.NotFound();
    
    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);


    veiculos.Nome = veiculoDTO.Nome;
    veiculos.Marca = veiculoDTO.Marca;
    veiculos.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculos);

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromQuery]int id, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.BuscaPorId(id);
    if(veiculos == null) return Results.NotFound();

    veiculoServico.Apagar(veiculos);

    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
