using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalAPITest.Dominio.Entidades;
using MinimalAPITest.Dominio.DTOs;

namespace MinimalAPITest.Dominio.Interfaces
{
    public interface IAdmServico
    {
        Administrador? Login(MinimalAPITest.Dominio.DTOs.Login.LoginDTO loginDTO);
    }
}