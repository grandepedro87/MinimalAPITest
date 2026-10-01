using System.Data.Common;
using MinimalAPITest.Dominio.Interfaces;
using MinimalAPITest.Infraestrutura.DB;
using MinimalAPITest.Dominio.DTOs;
using MinimalAPITest.Dominio.Entidades;

namespace MinimalAPITest.Dominio.Servicos;

public class AdmServico : IAdmServico
{
    private readonly DBcontexto _contexto;
    public AdmServico(DBcontexto contexto)
    {
        _contexto = contexto;
    }

    public Administrador? BuscaPorId(int id)
    {
        return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
    }

    public Administrador? Incluir(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();

        return administrador;
    }

    public Administrador? Login(MinimalAPITest.Dominio.DTOs.Login.LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return adm;
    }

    public List<Administrador> Todos(int pagina)
    {
         IQueryable<Administrador> query = _contexto.Administradores.AsQueryable();

        int itensPorPagina = 10;

        if (pagina > 0)
        {
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);  
        }

        return query.ToList();
    }
}