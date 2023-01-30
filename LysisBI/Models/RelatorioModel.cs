using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerBI.Api.V2.Models;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.Rest;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Runtime.InteropServices;
using LysisBI.Repository;
using System.Net.Http.Headers;
using System.Net;

namespace LysisBI.Models
{
    /// <summary>
    /// Classe Model com as propriedades e os métodos para geração das visões
    /// </summary>
    public class RelatorioModel
    {


        /// -- INICIO: Seção de propriedades de configuração do powerBI colhidas do Web.config e Cloud.config. 

        /// <summary>
        /// Usuário da conta do Azure que tem o devido registro da aplicação no Active Directory
        /// </summary>
        private static readonly string PBIUsername = ConfigurationManager.AppSettings["pbiUsername"];

        /// <summary>
        /// Senha da conta do Azure que tem o devido registro da aplicação no Active Directory
        /// </summary>
        private static readonly string Password = ConfigurationManager.AppSettings["pbiPassword"];

        /// <summary>
        /// Url de autorização para acesso aos ambientes azure e power bi. Propriedade colhida do Clouds.config
        /// </summary>
        private static readonly string AuthorityUrl = ConfigurationManager.AppSettings["authorityUrl"];

        /// <summary>
        /// Url para acesso aos recursos azure e power bi. Propriedade colhida do Clouds.config
        /// </summary>
        private static readonly string ResourceUrl = ConfigurationManager.AppSettings["resourceUrl"];

        /// <summary>
        /// Id da aplicação que está disponível no powerBI service (nuvem).
        /// </summary>
        private static readonly string ApplicationId = ConfigurationManager.AppSettings["ApplicationId"];

        /// <summary>
        /// Url da API do power bi. Propriedade colhida do Clouds.config.
        /// </summary>
        private static readonly string ApiUrl = ConfigurationManager.AppSettings["apiUrl"];

        /// <summary>
        /// Id do workspace do powerBI, que por sua vez, é o espaço de trabalho onde upamos nossas visões.
        /// </summary>
        private static readonly string WorkspaceId = ConfigurationManager.AppSettings["workspaceId"];

        /// -- FIM: Seção de propriedades de configuração do powerBI colhidas do web.config.



        /// -- INICIO: Seção de propriedades do report  - atributos necessário para geração das visões.

        /// <summary>
        /// Recebe o Id da visão para uma posterior geração dessa visão.
        /// </summary>
        public string Id { get; set; }
          
        /// <summary>
        /// Reponsável por levar a Url com os dados necessários para gerar a visão incorporada na página
        /// </summary>
        public string EmbedUrl { get; set; }

        /// <summary>
        /// Responsável pelo token de acesso
        /// </summary>
        public EmbedToken EmbedToken { get; set; }

        /// <summary>
        /// Responsável por receber a resposta sobre as funções que tal usuario tem acesso (ver, editar).
        /// </summary>
        public bool? IsEffectiveIdentityRolesRequired { get; set; }

        /// <summary>
        /// Responsável por receber a resposta sobre a identidade do usuario.
        /// </summary>
        public bool? IsEffectiveIdentityRequired { get; set; }

        /// <summary>
        /// Usuário que está tentando ver o relatório.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Permissões que tal usuário tem em relação aos relatórios (ver, editar).
        /// </summary>
        public string Roles { get; set; }

        /// <summary>
        /// Mensagem de erro ocorrida ao gerar o relatório.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// -- FIM: Seção de propriedades do report  - atributos necessário para geração das visões.



        /// -- INICIO: Seção de propriedades que são utilizadas em interação com a base de dados.
        
        /// <summary>
        /// Utilizado para passar o identificador de determinado Report par utilizar em consultas
        /// </summary>
        public int Identificador { get; set; }

        /// <summary>
        /// Utilizado para passar o nome de uma tabela, parâmetro ou dataset criado com a consulta
        /// </summary>
        public string Tabela { get; set; }

        ///<summary>
        /// Utilizado para passar o valor da coluna de uma tabela ou em um parâmetro criado
        /// </summary>
        public string Coluna { get; set; }


        /// <summary>
        /// Utilizado para passar o operador logico da expressao de consulta, sendo: 'eq' para '='
        /// </summary>
        public string OperadorLogico { get; set; }

        /// <summary>
        /// Utilizado para passar o nome de uma tabela, parâmetro ou dataset criado com a consulta
        /// </summary>
        public string TabelaAux { get; set; }

        ///<summary>
        /// Utilizado para passar o valor da coluna de uma tabela ou em um parâmetro criado
        /// </summary>
        public string ColunaAux { get; set; }


        /// <summary>
        /// Utilizado para passar o operador logico da expressao de consulta, sendo: 'eq' para '='
        /// </summary>
        public string OperadorLogicoAux { get; set; }


        /// <summary>
        /// Utilizado para passar o valor a ser consultado em uma tabela ou em um parâmetro criado
        /// </summary>
        public int ValorParametro { get; set; }

        /// <summary>
        /// Identificador do relatorio dentro do ambiente do powerBI. Esse será passado via URL para montagem da visão.
        /// </summary>
        public string CodRelatorio { get; set; }

        public List<int> ParametroFiltroClientes { get; set; }

        public bool realizaFiltroCliente { get; set; }

        /// -- FIM: Seção de propriedades que são utilizadas em interação com a base de dados.



        /// -- INICIO: Seção de métodos.

        /// <summary>
        /// Método responsável por receber os reports disponiveis na base
        /// </summary>
        public void ObterDadosReport([Optional]int isn_relatorio)
        {
            if (isn_relatorio > 0)
            {
                //Caso seja passado o isn_relatorio, pega o dado correspondente
               
                Entities db = new Entities();
                var relatorioBase = db.RBI_RELATORIO_BI.Where(rbi => rbi.ISN_RELATORIO_BI == isn_relatorio).FirstOrDefault();
                
                Coluna = relatorioBase.COL_RELATORIO_BI;
                OperadorLogico = relatorioBase.OP_LOGICO_RELATORIO_BI;
                Tabela = relatorioBase.TAB_RELATORIO_BI;

                ColunaAux = relatorioBase.COL_RELATORIO_BI_AUX;
                OperadorLogicoAux = relatorioBase.OP_LOGICO_RELATORIO_BI_AUX;
                TabelaAux = relatorioBase.TAB_RELATORIO_BI_AUX;
                //Caso a consulta seja por cliente (ParametroFiltroClientes != null) e Exista relatorio auxiliar
                // (relatorioBase.TIP_RELATORIO_AUX ==1) pegar o relatorio auxiliar, caso não, pegar o relatorio original
                if (ParametroFiltroClientes != null && ParametroFiltroClientes.Count > 0 && relatorioBase.TIP_RELATORIO_AUX ==1)
                {

                    CodRelatorio = relatorioBase.COD_RELATORIO_BI_AUX;
                }
                else
                {

                    CodRelatorio = relatorioBase.COD_RELATORIO_BI;
                }



                }
            else
            {
                //Caso não seja passado o isn_relatorio, pega o primeiro registro
             
                Entities db = new Entities();
                var relatorioBase = db.RBI_RELATORIO_BI.FirstOrDefault();
                Tabela = relatorioBase.TAB_RELATORIO_BI;
                Coluna = relatorioBase.COL_RELATORIO_BI;
                CodRelatorio = relatorioBase.COD_RELATORIO_BI;
                OperadorLogico = relatorioBase.OP_LOGICO_RELATORIO_BI;
            }


        }


        /// <summary>
        /// Método responsável por preencher uma lista com os reports disponíveis na base.
        /// </summary>
        public List<KeyValuePair<int, String>> ListDropdownReports()
        {

            Entities db = new Entities();
            var listFromDataBase = db.RBI_RELATORIO_BI;
            var listToDropdown = new List<KeyValuePair<int, string>>();
            foreach (var currentItem in listFromDataBase)
            {
                listToDropdown.Add(new KeyValuePair<int, string>(currentItem.ISN_RELATORIO_BI, currentItem.NOM_RELATORIO_BI));

            }
            return listToDropdown;
        }

       


        /// <summary>
        ///Método responsável pela geração da visão
        /// </summary>
        public async Task<Object> GerarReport([Optional] int isn_report)

        {

            var result = this;

            try
            {

                var error = GetWebConfigErrors();
                if (error != null)
                {
                    result.ErrorMessage = error;

                    return result;
                }

                // Criando as credencias do usuário.
                var credential = new UserPasswordCredential(PBIUsername, Password);

                // Autenticação utilizando as credenciais criadas.
                var authenticationContext = new AuthenticationContext(AuthorityUrl);
                var authenticationResult = await authenticationContext.AcquireTokenAsync(ResourceUrl, ApplicationId, credential);

                if (authenticationResult == null)
                {
                    result.ErrorMessage = "Falha de autenticação.";
                    return result;
                }

                var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

                // Criando um objeto do tipo PowerBI para realizar as chamadas
                using (var client = new PowerBIClient(new Uri(ApiUrl), tokenCredentials))
                {
                    // Pegando os reports existentes no workspace.
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    
                    var reports = await client.Reports.GetReportsInGroupAsync(WorkspaceId);

                    // Validando se existem report no workspace.
                    if (reports.Value.Count() == 0)
                    {
                        result.ErrorMessage = "Não foram encontrados reports no Workspace";
                        return result;
                    }

                    Report report;

                    //Recuperando os dados do report da base de dados e preenchendo o atributo CodRelatorio
                    ObterDadosReport(isn_report);

                    report = reports.Value.FirstOrDefault(r => r.Id == CodRelatorio); //setar o reportID recuperado da base


                    if (report == null)
                    {
                        result.ErrorMessage = "Não foi encontrado nenhum report com o ID passado.";
                        return result;
                    }

                    var datasets = await client.Datasets.GetDatasetByIdInGroupAsync(WorkspaceId, report.DatasetId);
                    result.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
                    result.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;
                    GenerateTokenRequest generateTokenRequestParameters;
                    // Criando um token com as efetivas credenciais setadas no webconfig
                    if (!string.IsNullOrWhiteSpace(Username))
                    {
                        var rls = new EffectiveIdentity(Username, new List<string> { report.DatasetId });
                        if (!string.IsNullOrWhiteSpace(Roles))
                        {
                            var rolesList = new List<string>();
                            rolesList.AddRange(Roles.Split(','));
                            rls.Roles = rolesList;
                        }
                        // Gerando token com seu nível de acesso.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view", identities: new List<EffectiveIdentity> { rls });
                    }
                    else
                    {
                        // Gerando token com seu nível de acesso apenas para view.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    }



                    var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(WorkspaceId, report.Id, generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        result.ErrorMessage = "Falha na geração do token de credenciais.";
                        return result;
                    }





                    // Finalmente, gerando a view com o report requisitado.
                    result.EmbedToken = tokenResponse;
                    result.EmbedUrl = report.EmbedUrl;
                    result.Id = report.Id;


                    return result;
                }
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }

            return result;
        }

        /// <summary>
        /// Método responsável por checar se o webconfig tem os valores válidos. 
        /// </summary>
        /// <returns>Null se  os parametros web.config são válidos, caso contrário retorna o erro específico.</returns>
        private string GetWebConfigErrors()
        {
            // Application Id deve ter valor.
            if (string.IsNullOrWhiteSpace(ApplicationId))
            {
                return "ApplicationId não tem valor atribuido. Favor, registrar a aplicção de modo 'nativo' em  https://dev.powerbi.com/apps e setar o clientID no web.config.";
            }

            // Application Id deve ter valor.
            Guid result;
            if (!Guid.TryParse(ApplicationId, out result))
            {
                return "ApplicationId está vazio. Favor, registrar a aplicação de modo 'nativo' em https://dev.powerbi.com/apps e setar o applcationID no web.config.";
            }

            // Workspace Id deve ter valor.
            if (string.IsNullOrWhiteSpace(WorkspaceId))
            {
                return "WorkspaceId está vazio. Favor, inserir tais dados no web.config";
            }

            // Workspace Id deve ter valor.
            if (!Guid.TryParse(WorkspaceId, out result))
            {
                return "WorkspaceId must be a Guid object. Favor, setar o Id do seu Workspace de preferência no web.config";
            }

            // Username deve ter valor.
            if (string.IsNullOrWhiteSpace(PBIUsername))
            {
                return "Username está vazio. Favor, setar Power BI username no web.config";
            }

            // Password deve ter valor.
            if (string.IsNullOrWhiteSpace(Password))
            {
                return "Password está vazio. Favor, setar Power BI password no web.config";
            }

            return null;
        }

        /// -- FIM: Seção da seção de métodos.

    }
}