using System.Data.Common;
using MinimalAPITest.Dominio.Interfaces;
using MinimalAPITest.Infraestrutura.DB;
using MinimalAPITest.Dominio.DTOs;
using MinimalAPITest.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace MinimalAPITest.Dominio.Servicos;

public class VeiculoServico : IVeiculoServico
{
    private readonly DBcontexto _contexto;
    public VeiculoServico(DBcontexto contexto)
    {
        _contexto = contexto;
    }
    
    public void Apagar(Veiculo veiculo)
    {
        _contexto.Veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    public void Atualizar(Veiculo veiculo)
    {
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscaPorId(int id)
    {
        return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Incluir(Veiculo veiculo)
    {
        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
    {
        IQueryable<Veiculo> query = _contexto.Veiculos.AsQueryable();
        if(!string.IsNullOrEmpty(nome))
        {
            query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
        }

        int itensPorPagina = 10;

        if(pagina != null)
        {
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);  
        }

        return query.ToList();
    }
}