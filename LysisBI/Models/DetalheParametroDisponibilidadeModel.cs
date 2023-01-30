using LysisBI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LysisBI.Models
{
    public class DetalheParametroDisponibilidadeModel
    {

        public int IsnDetalheParametroDisponibilidade { get; set; }

        public int IsnParametroDisponibilidade { get; set; }

        public int DiaDisponibilidadeAnalytics { get; set; }

        public int HoraPausaDisponibilidade { get; set; }

        Entities db;

        public void ObterHoraPausaDisponibilidade()
        {
            try
            {
                DPA_DETALHE_PARAMETRO_DISPONIBILIDADE_ANALYTICS dpa = new DPA_DETALHE_PARAMETRO_DISPONIBILIDADE_ANALYTICS();

                db = new Entities();

                dpa = db.DPA_DETALHE_PARAMETRO_DISPONIBILIDADE_ANALYTICS.First(r => r.ISN_PARAMETRO_DISPONIBILIDADE_ANALYTICS == this.IsnParametroDisponibilidade);

                

            }
            catch (Exception ex)
            
            {

            }

            
        }

        public bool DiaAtivoAnalytics()
        {
            db = new Entities();
            string DiaDaSemana = "";
            var diasAnalyticsDisponivel = from DPA in db.DPA_DETALHE_PARAMETRO_DISPONIBILIDADE_ANALYTICS

                                   select new
                                   {
                                       DPA.TIP_DIA_DISPONIBILIDADE_ANALYTICS
                                   };

            foreach (var currentRow in diasAnalyticsDisponivel)
            {
                switch((int)currentRow.TIP_DIA_DISPONIBILIDADE_ANALYTICS)
                {
                    case 1:
                        DiaDaSemana = "Sunday";
                        break;
                    case 2:
                        DiaDaSemana = "Monday";
                        break;
                    case 3:
                        DiaDaSemana = "Tuesday";
                        break;
                    case 4:
                        DiaDaSemana = "Wednesday";
                        break;
                    case 5:
                        DiaDaSemana = "Thursday";
                        break;
                    case 6:
                        DiaDaSemana = "Friday";
                        break;
                    case 7:
                        DiaDaSemana = "Saturday";
                        break;
                    default:
                        DiaDaSemana = "";
                        break;
                    
                }

                if  (DateTime.Now.DayOfWeek.ToString() == DiaDaSemana)
                {
                    return true;
                }
            }
            
                return false;
        }


    }
}