using Microsoft.EntityFrameworkCore;
using MinimalAPITest.Dominio.Entidades;

namespace MinimalAPITest.Infraestrutura.DB;

public class DBcontexto : DbContext
{
    private readonly IConfiguration _configuracaoAppSettings;
    public DBcontexto(IConfiguration configuracaoAppSettings)
    {
        _configuracaoAppSettings = configuracaoAppSettings;
    }
    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador {
                Id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexão = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
            if(!string.IsNullOrEmpty(stringConexão))
            {
                optionsBuilder.UseMySql(stringConexão, 
                ServerVersion.AutoDetect(stringConexão));
            }
        }

    }
}