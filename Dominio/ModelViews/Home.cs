using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalAPITest.Dominio.ModelViews
{
    public struct Home
    {
        public string Mensagem { get => "Bem vindos ao projeto de API de veiculos";}
        public string Doc { get => "/swagger";}

    }
}