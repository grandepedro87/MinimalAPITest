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
    public Administrador? Login(MinimalAPITest.Dominio.DTOs.Login.LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return adm;
    }
}