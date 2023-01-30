using LysisBI.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LysisBI.Models
{
    public class ClienteModel
    {
        /// <summary>
        /// Identificador da empresa do usário que acessa a aplicação
        /// </summary>
        public int Isn_Empresa { get; set; }

        /// <summary>
        /// Identificador do cliente
        /// </summary>
        public int Isn_Pessoa { get; set; }

        /// <summary>
        /// Nome do cliente
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Valor booleano se checkbox do cliente é marcado ou não.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Lista com os clientes a serem exibidos no popup.
        /// </summary>
        public List<SelectListItem> ListaDeClientes { get; set; }

        /// <summary>
        /// Recebe uma lista com os ISN dos clientes selecionados para serem utilizados como parametro
        /// nas visões.
        /// </summary>
        public List<int> ParametroFiltroClientes { get; set; }

        /// <summary>
        /// Recebe uma lista com os ISN dos clientes selecionados.
        /// </summary>
        public List<int> IsnClientesSelecionados { get; set; }

        /// <summary>
        /// Recebe o ISN dos grupo selecionado.
        /// </summary>
        public List<int> IsnGruposSelecionados { get; set; }

        /// <summary>
        /// Lista com os grupos a serem exibidos no popup.
        /// </summary>
        public List<SelectListItem> ListaDeGrupos { get; set; }

        /// <summary>
        /// Armazena os ISNs dos clientes selecionados.
        /// </summary>
        public List<int> SelectedClientes { get; set; }


        /// <summary>
        /// Armazena os ISNs dos grupos selecionados.
        /// </summary>
        public List<int> SelectedGrupos { get; set; }

        /// <summary>
        /// Método responsável por preencher uma lista com os clientes da empresa do usuário logado.
        /// </summary>
        /// /// <returns>Uma lista com o ISN dos clientes para uso no filtro dos reports</returns>
        public List<SelectListItem> ListarClientes()
        {

            Entities db = new Entities();
            var consultaClientes = from VPP in db.VPP_PARTE_PROCESSO

                                   join PES in db.PES_PESSOA
                                   on VPP.ISN_PESSOA equals PES.ISN_PESSOA

                                   join PRO in db.PRO_PROCESSO
                                   on VPP.ISN_PROCESSO equals PRO.ISN_PROCESSO

                                   where PRO.ISN_EMPRESA == Isn_Empresa && VPP.TIP_CLIENTE == 1
                                   orderby PES.NOM_PESSOA descending
                                   select new
                                   {

                                       PES.ISN_PESSOA,
                                       PES.NOM_PESSOA
                                   };

            List<SelectListItem> lst = new List<SelectListItem>();
            foreach (var currentRow in consultaClientes.Distinct().OrderBy(c => c.NOM_PESSOA))
            {
                SelectListItem item = new SelectListItem();
                item.Value = currentRow.ISN_PESSOA.ToString();
                item.Text = currentRow.NOM_PESSOA;


                //Utilizado para manter selecionado os clientes que já foram previamente selecionados

                if (IsnClientesSelecionados != null && IsnClientesSelecionados.Count() > 0)
                {
                    foreach (var currentRowSelected in IsnClientesSelecionados)
                    {
                        if (item.Value == currentRowSelected.ToString())
                        {
                            item.Selected = true;

                        }
                    }
                }



                lst.Add(item);


            }

            return lst;
        }

        /// <summary>
        ///Realiza uma consulta por isn dos clientes e recebe o nome dos mesmo, para exibição.
        /// </summary>
        /// <param name="lst"></param>
        /// <returns>Nome dos clientes selecionados</returns>
        public string ObtemClientesSelecionados(List<int> lst)
        {
            string clientesSelecionados = "Todos";

            Entities db = new Entities();
            var consultaClientes = from VPP in db.VPP_PARTE_PROCESSO

                                   join PES in db.PES_PESSOA
                                   on VPP.ISN_PESSOA equals PES.ISN_PESSOA

                                   join PRO in db.PRO_PROCESSO
                                   on VPP.ISN_PROCESSO equals PRO.ISN_PROCESSO

                                   where lst.Contains(PES.ISN_PESSOA) && VPP.TIP_CLIENTE == 1
                                   select new
                                   {
                                       PES.NOM_PESSOA
                                   };

            if (lst != null && lst.Count() > 0)
            {
                clientesSelecionados = "";
                foreach (var currentCliente in consultaClientes.Distinct().OrderBy(c => c.NOM_PESSOA))
                {
                    clientesSelecionados = clientesSelecionados + currentCliente.NOM_PESSOA + " | ";
                }

            }
            //Caso apenas um cliente seja selecionado ou o grupo tenha apenas um componente, remover o separador " | ".
            if (lst.Count() == 1)
            {
                clientesSelecionados = clientesSelecionados.Replace("|", "").Trim();
            }


            return clientesSelecionados;
        }

        /// <summary>
        /// Método responsável por listar os grupos de pessoa pelo isn_empresa.
        /// </summary>
        /// <param name="Isn_Empresa"></param>
        /// <returns>Lista do tipo SelectListItem com: Isn do grupo e descrição do grupo.</returns>
        public List<SelectListItem> listaGrupos(int Isn_Empresa)
        {
            Entities db = new Entities();
            var consultaGrupos = from GPE in db.GPE_GRUPO_PESSOA

                                 where GPE.ISN_EMPRESA == Isn_Empresa
                                 orderby GPE.NOM_GRUPO_PESSOA descending
                                 select new
                                 {

                                     GPE.ISN_GRUPO_PESSOA,
                                     GPE.NOM_GRUPO_PESSOA
                                 };


            List<SelectListItem> lst = new List<SelectListItem>();
            foreach (var currentRow in consultaGrupos.Distinct().OrderBy(c => c.NOM_GRUPO_PESSOA))
            {
                SelectListItem item = new SelectListItem();
                item.Value = currentRow.ISN_GRUPO_PESSOA.ToString();
                item.Text = currentRow.NOM_GRUPO_PESSOA;
                lst.Add(item);

                //Utilizado para manter o grupo selecionado.

                if (IsnGruposSelecionados != null)
                {
                    foreach(var currentRowSelected in IsnGruposSelecionados)
                    {
                        if (item.Value == currentRowSelected.ToString())
                        {
                            item.Selected = true;

                        }
                    }
                }


            }

            return lst;
        }

        /// <summary>
        /// Responsável por obter os integrantes dos grupos selecionados (apenas quem é cliente).
        /// </summary>
        /// <param name="isn_grupo_selecionado"></param>
        /// <returns>Uma lista com o isn dos clientes dos grupos selecionados</returns>
        public List<int> ObtemClientePeloGrupo(List<int> gruposSelecionados)
        {


            Entities db = new Entities();
            var clientes = from VPP in db.VPP_PARTE_PROCESSO

                           join PRO in db.PRO_PROCESSO
                           on VPP.ISN_PROCESSO equals PRO.ISN_PROCESSO

                           join PES in db.PES_PESSOA
                           on VPP.ISN_PESSOA equals PES.ISN_PESSOA

                           join GPE in db.GPE_GRUPO_PESSOA
                           on PES.ISN_GRUPO_PESSOA equals GPE.ISN_GRUPO_PESSOA

                           where  gruposSelecionados.Contains(GPE.ISN_GRUPO_PESSOA) && VPP.TIP_CLIENTE == 1
                           select new
                           {
                               PES.ISN_PESSOA
                           };


            List<int> listaDeIsnClientes = new List<int>();
            foreach (var currentCliente in clientes.Distinct())
            {

                listaDeIsnClientes.Add(currentCliente.ISN_PESSOA);

            }


            return listaDeIsnClientes;
        }

    }

}