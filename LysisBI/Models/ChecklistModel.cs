using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LysisBI.Models
{
    public class ChecklistModel
    {

        public int Isn_Pessoa { get; set; }

        public string Nome { get; set; }

        public bool Selected { get; set; }

        public string Dsc_Login { get; set; }

        public string Dsc_Empresa { get; set; }

        public int Isn_Grupo_Pessoa { get; set; }

        public string Nome_Grupo_Pessoa { get; set; }

    }
}