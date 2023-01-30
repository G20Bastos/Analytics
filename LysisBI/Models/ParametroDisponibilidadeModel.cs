using LysisBI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LysisBI.Models
{
    public class ParametroDisponibilidadeModel
    {

        public int IsnParametroDisponibilidade { get; set; }

        public string HoraPausaDisponibilidade { get; set; }

        public string HoraInicioDisponibilidade { get; set; }

        Entities db;

        public void ObterHoraPausaDisponibilidade()
        {
            try
            {
                PDA_PARAMETRO_DISPONIBILIDADE_ANALYTICS pda = new PDA_PARAMETRO_DISPONIBILIDADE_ANALYTICS();

                db = new Entities();

                pda = db.PDA_PARAMETRO_DISPONIBILIDADE_ANALYTICS.First(r => r.ISN_PARAMETRO_DISPONIBILIDADE_ANALYTICS == this.IsnParametroDisponibilidade);

                this.HoraPausaDisponibilidade = pda.HOR_PAUSA_DISPONIBILIDADE;
                this.HoraInicioDisponibilidade = pda.HOR_INICIO_DISPONIBILIDADE;

            }
            catch (Exception ex)
            {

            }

            
        }




    }
}