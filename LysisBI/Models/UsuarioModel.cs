using LysisBI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Techway;

namespace LysisBI.Models
{
    /// <summary>
    /// Classe Model com as propriedades dos usuário e seus métodos.
    /// </summary>
    public class UsuarioModel

    {

        /// <summary>
        /// Instancia de Util da Techway.dll para utilizar o método EncriptaSenha
        /// </summary>
        private Util util = new Util();

        /// <summary>
        /// Usuário que acessa a aplicação
        /// </summary>
        public string UsuarioAcesso { get; set; }

        /// <summary>
        /// Senha do usuário que acessa a aplicação
        /// </summary>
        public string Senha { get; set; }

        /// <summary>
        /// Identificador da empresa do usário que acessa a aplicação
        /// </summary>
        public int Isn_Empresa { get; set; }

        /// <summary>
        /// Nome do usuário que está acessando a aplicação. Colhido da base de dados.
        /// </summary>
        public string Nome_Pessoa { get; set; }

        /// <summary>
        /// Nome da empresa do usário que acessa a aplicação. Colhido da base de dados.
        /// </summary>
        public string Dsc_Empresa { get; set; }

        /// <summary>
        /// Variavel responsavel por armazenar a resposta de: Usuário é admin?.
        /// </summary>
        public string UsuarioAdmin { get; set; }

        /// <summary>
        /// Valor booleano se checkbox é marcado ou não.
        /// </summary>
        public bool Selected { get; set; }



        /// <summary>
        /// Lista com as empresas a serem exibidos no popup.
        /// </summary>
        public List<ChecklistModel> ListaDeEmpresa { get; set; }

        /// <summary>
        /// Lista com os usuarios a serem exibidos no popup.
        /// </summary>
        public List<ChecklistModel> ListaDeUsuarios { get; set; }


        /// <summary>
        /// Mensagem de erro a ser exibida em tela ao usuário.
        /// </summary>
        public string MensagemErro { get; set; }

        public  int[] clientes {get; set;}

        /// <summary>
        /// Define se o sistema deve ou não exibir informação adicional
        /// </summary>
        public bool exibeInfoAdicional { get; set; }

        /// <summary>
        /// Método responsável por válidar e efetivar o login do usuário.
        /// </summary>
        /// <returns></returns>
        public bool validaAcesso()
        {


            var senhaEncriptada = util.EncriptaSenha(UsuarioAcesso, Senha);
            Entities db = new Entities();
            //Consulta usando os dados digitados pelo usuário para acesso ao sistema.
            var usuarioBase = db.PES_PESSOA.Where(m => m.DSC_LOGIN == UsuarioAcesso && m.DSC_SENHA == senhaEncriptada && m.TIP_STATUS == 1).FirstOrDefault();
           
            try
            {
                //Verificando se o usuário existe
                if (usuarioBase != null)
                {

                    //Verificando se o usuário logado tem acesso ao Analytics
                    var acessaAnalytics = db.EMP_EMPRESA.Where(t => t.ISN_EMPRESA == usuarioBase.ISN_EMPRESA && t.TIP_ANALYTICS == 1).FirstOrDefault();
                    if (acessaAnalytics != null)
                    {

                        Isn_Empresa = usuarioBase.ISN_EMPRESA.Value;
                        Nome_Pessoa = usuarioBase.DSC_LOGIN;

                        //Chamando método responsável por verificar se o usuário tem privilégios de ADMIN
                        UsuarioAdmin = validaUsuarioAdmin(usuarioBase.DSC_LOGIN);

                        //Verificando se a empresa deve exibir o dashboard de informações adicionais
                        exibeInfoAdicional = habilitaInformacaoAdicional(usuarioBase.ISN_EMPRESA.Value);

                        //Recuperando a descrição da empresa a partir do Isn_Empresa do usuário logado.
                        var dadosEmpresa = db.PES_PESSOA.Where(m => m.ISN_PESSOA == usuarioBase.ISN_EMPRESA).FirstOrDefault();
                        Dsc_Empresa = dadosEmpresa.NOM_PESSOA;

                        //Caso o usuário logado exista e tenha acesso ao Analytics - return true;
                        return true;

                    }
                    //Caso o usuário logado exista, porém, não tenha acesso ao Analytics - return false;
                    return false;
                    MensagemErro = "Usuário e/ou Senha incorretos.";
                }
                else
                {
                    MensagemErro = "Usuário e/ou Senha incorretos.";
                    return false;

                }

            } catch (Exception ex)
            {
                MensagemErro = "Usuário e/ou Senha incorretos.";
                return false;
            }
           


        }

        public bool habilitaInformacaoAdicional(int isn_Empresa)
        {
            Entities db = new Entities();
             
            try
            {
                var habilitaInformacaoAdicional = db.EMP_EMPRESA.Where(t => t.ISN_EMPRESA == isn_Empresa && t.TIP_INF_ADICIONAL_ANALYTICS == 1).FirstOrDefault();
                if (habilitaInformacaoAdicional != null)
                {
                    return true;

                } else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                MensagemErro = "Não foi possível realizar consulta de verificação TIP_INFO_ADICIONAL_ANALYTICS. Causa: " + ex;
                return false;
            }
        }

        /// <summary>
        /// Método responsável por verificar se o usuário tem privilégios de ADMIN
        /// </summary>
        /// <returns>Valor boolean que indica se o usuário é admin ou não</returns>
        public string validaUsuarioAdmin(string dsc_login)
        {
                      if (dsc_login == "alfredo")
            {
                return "admin";
            }    else
            {
                return "";
            }

                
        }
        
        public List<ChecklistModel> listaEmpresa()
        {
            Entities db = new Entities();
            var consultaEmpresas = from EMP in db.EMP_EMPRESA


                                   join PES in db.PES_PESSOA
                                   on EMP.ISN_EMPRESA equals PES.ISN_PESSOA

                                   orderby PES.NOM_PESSOA descending
                                   select new
                                   {

                                       PES.ISN_PESSOA,
                                       PES.NOM_PESSOA
                                   };

            List<ChecklistModel> lst = new List<ChecklistModel>();
            foreach (var currentRow in consultaEmpresas.Distinct().OrderBy(c => c.NOM_PESSOA))
            {
                ChecklistModel chk = new ChecklistModel();
                chk.Isn_Pessoa = currentRow.ISN_PESSOA;
                chk.Nome = currentRow.NOM_PESSOA;
                lst.Add(chk);


            }

            return lst;
        }


        public List<ChecklistModel> listaUsuarios(int Isn_Empresa_Ativacao_Perfil)
        {
            Entities db = new Entities();
            var consultaUsuarios = from PES in db.PES_PESSOA
                                   
                                   where PES.ISN_EMPRESA == Isn_Empresa_Ativacao_Perfil 
                                   && PES.DSC_LOGIN != null
                                   orderby PES.NOM_PESSOA descending
                                   select new
                                   {

                                       PES.ISN_PESSOA,
                                       PES.NOM_PESSOA,
                                       PES.DSC_LOGIN
                                   };

            var dadosEmpresa = db.PES_PESSOA.Where(m => m.ISN_PESSOA == Isn_Empresa_Ativacao_Perfil).FirstOrDefault();
            var Dsc_Empresa = dadosEmpresa.NOM_PESSOA;

            List<ChecklistModel> lst = new List<ChecklistModel>();
            foreach (var currentRow in consultaUsuarios.Distinct().OrderBy(c => c.NOM_PESSOA))
            {
                ChecklistModel chk = new ChecklistModel();
                chk.Isn_Pessoa = currentRow.ISN_PESSOA;
                chk.Nome = currentRow.NOM_PESSOA;
                chk.Dsc_Login = currentRow.DSC_LOGIN;
                chk.Dsc_Empresa = Dsc_Empresa;
                lst.Add(chk);


            }

            return lst;
        }

        

    }
}