using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using System;
using System.Linq;
using LysisBI.Models;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace LysisBI.Controllers
{
    /// <summary>
    /// Classe controladora das operação de acesso e geração das visões
    /// </summary>
    public class AnalyticsController : Controller
    {

        /// <summary>
        /// Variável global que recebe o valor selecionado no dropdown das visões
        /// </summary>
        private static int valorDrop = 1;


        /// <summary>
        /// Responsável por gerar a tela de login
        /// </summary>
        /// <returns>Tela de login</returns>
        /// 

       // private Agendador agendador = new Agendador();

        /// <summary>
        /// Path para geração de arquivo de log
        /// </summary>
        private static readonly string PathLogs = ConfigurationManager.AppSettings["pathLogs"];

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Responsável por receber os dados do usuario e realizar o acesso a aplicação.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>Retorna a ação para redirecionamento à view Dashboard em caso de êxito no acesso do usuário</returns>
        [HttpPost]
        public async Task<ActionResult> Login(UsuarioModel usuario)
        {
            Object result;

            if (!usuario.validaAcesso())
            {
                ViewBag.msgErro = usuario.MensagemErro;
                return View(usuario);

            }
            else

                Session["Usuario"] = usuario.Nome_Pessoa;
            Session["Isn_Empresa"] = usuario.Isn_Empresa;
            Session["Dsc_Empresa"] = usuario.Dsc_Empresa;
            Session["Habilita_Admin"] = usuario.UsuarioAdmin;
            Session["Habilita_Inf_Adicional"] = usuario.exibeInfoAdicional;

            //Após êxito no Login, verifica o status da capacidade dedicada

            CapacidadeDedicadaModel CapacidadeDedicada = new CapacidadeDedicadaModel();

            await CapacidadeDedicada.ObterDetalhesCapacidadeDedicada();

            //Capacidade Dedicada Pausada


            if (CapacidadeDedicada.Status == "Paused")
            {
                //if (CapacidadeDedicada.Status == "Succeeded") {

                //Verificando os parâmetros de inicialização (hora de suspensão da capacidade dedicada e dias da semana)
                ParametroDisponibilidadeModel parametroDisponibilidade = new ParametroDisponibilidadeModel();
                DetalheParametroDisponibilidadeModel detalheParametroDisponibilidade = new DetalheParametroDisponibilidadeModel();

                parametroDisponibilidade.IsnParametroDisponibilidade = 1;
                parametroDisponibilidade.ObterHoraPausaDisponibilidade();

                detalheParametroDisponibilidade.IsnParametroDisponibilidade = parametroDisponibilidade.IsnParametroDisponibilidade;

                DateTime HoraParametrizadaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Int32.Parse(parametroDisponibilidade.HoraInicioDisponibilidade), 0, 0);
                DateTime HoraParametrizadaFim = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Int32.Parse(parametroDisponibilidade.HoraPausaDisponibilidade), 0, 0);

                //Verificando se o dia atual da semana é um dia permitido para acesso
                if (detalheParametroDisponibilidade.DiaAtivoAnalytics())
                {
                    //Verificando se a hora atual é igual ou inferior a hora em que o Analytics deve estar
                    //indisponível e sua capacidade dedicada pausada
                    if (DateTime.Compare(HoraParametrizadaInicio, DateTime.Now) < 0 && DateTime.Compare(DateTime.Now, HoraParametrizadaFim) < 0)
                    {
                        StreamWriter arquivoLog = null;

                        try
                        {

                            ////Colocando o caminho fisico e o nome do arquivo a ser criado
                            //finalizando com .txt
                            string CaminhoNome = PathLogs + "LogInicioServico_" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt";

                            //utilizando o metodo para criar um arquivo texto
                            //e associando o caminho e nome ao metodo
                            arquivoLog = System.IO.File.CreateText(CaminhoNome);

                            //aqui, exemplo de escrever no arquivo texto
                            //como se fossemos criar um recibo de pagamento 

                            //escrevendo o titulo
                            arquivoLog.WriteLine("Inicio da operação");
                            //pulando linha sem escrita
                            arquivoLog.WriteLine();
                            arquivoLog.WriteLine();
                            //escrevendo conteúdo do recibo
                            arquivoLog.WriteLine("Data/hora atual: " + DateTime.Now.ToString());
                            arquivoLog.WriteLine("Intervalo parametrizado para atividade do serviço: " + HoraParametrizadaInicio + " à " + HoraParametrizadaFim);
                            arquivoLog.WriteLine("Acesso realizado pelo usuário: " + Session["Usuario"].ToString() + " da empresa " + Session["Dsc_Empresa"].ToString());


                            arquivoLog.WriteLine();

                            CapacidadeDedicada.Iniciar();



                        }
                        catch (Exception ex)
                        {
                            arquivoLog.WriteLine("Ocorreu um erro ao iniciar capacidade dedicada. Erro: " + ex.Message + " - Stack Trace: " + ex.StackTrace);
                        }
                        finally
                        {
                            //fechando o arquivo texto com o método .Close()
                            arquivoLog.WriteLine("Fim da operação");
                            arquivoLog.WriteLine("__________________________________________________");
                            arquivoLog.Close();
                        }


                    }
                }

                CapacidadeDedicada.Esperar(60);
                return RedirectToAction("Dashboard");


            }
            else
            {
                return RedirectToAction("Dashboard");
            }





        }

        /// <summary>
        /// Responsável por responder a requisição de logout da aplicação.
        /// </summary>
        /// <returns>Ação de redirecionamento à View Login</returns>
        [HttpGet]
        public ActionResult Logout()
        {
            Session["Usuario"] = null;
            Session["Isn_Empresa"] = null;
            Session["Dsc_Empresa"] = null;
            Session["Parametros_Cliente"] = null;
            valorDrop = 1;
            return RedirectToAction("Login");

        }


        /// <summary>
        /// Recebe o valor que o usuário selecionou no dropdown 
        /// e passa para a action dashboard gerar a visão correspondente
        /// </summary>
        /// <param name="selectedValue">Valor inteiro que corresponde ao Id do report na base de dados</param>
        /// <returns></returns>
        public ActionResult CarregaReportSelecionado(int selectedValue)
        {
            valorDrop = selectedValue;

            return RedirectToAction("Dashboard");
        }



        /// <summary>
        /// Responsável por receber os dados do relatório que o usuário deseja ver
        /// e passá-los para o RelatorioModel tratá-los
        /// </summary>
        /// <param name="relatorio"></param>
        /// <returns>A visão do relatório solicitado em caso de êxito. Em caso de não êxito, retona o erro correspondente.</returns>
        public async Task<ViewResult> Dashboard(RelatorioModel relatorio)


        {


            if (Session["Isn_Empresa"] != null)
            {
                Object result;
                try
                {


                    if (Session["Parametros_Cliente"] != null)
                    {
                        relatorio.ParametroFiltroClientes = (List<int>)Session["Parametros_Cliente"];
                        result = await relatorio.GerarReport(valorDrop);
                        relatorio.ValorParametro = (int)(Session["Isn_Empresa"]);

                    }
                    else
                    {
                        result = await relatorio.GerarReport(valorDrop);
                        relatorio.ValorParametro = (int)(Session["Isn_Empresa"]);
                        Session["ClientesSelecionados"] = "Todos";
                    }


                    return View(result);
                }
                catch
                {

                }
                return View();

            }
            else
            {
                ViewBag.MsgAcessoRestrito = "É necessário estar logado para obter acesso ao recurso solicitado.";

                return null;

            }

        }
        [AsyncTimeout(3000)]
        /// <summary>
        /// Responsável exibir o popup com os clientes do escritório logado no sistema
        /// </summary>
        /// <returns>A visão do popup com a lista de clientes.</returns>
        public ActionResult ViewPopupClientes()
        {
            if (Session["Isn_Empresa"] != null)
            {
                ClienteModel cliente = new ClienteModel();
                cliente.Isn_Empresa = (int)(Session["Isn_Empresa"]);
                cliente.IsnClientesSelecionados = (List<int>)Session["Parametros_Cliente"];
                cliente.ListaDeClientes = cliente.ListarClientes();

                return View(cliente);
            }
            else
            {

                return null;
            }

        }

        /// <summary>
        /// Responsável atribuir os ISNs dos clientes selecionados no popup às variáveis de sessão
        /// </summary>
        /// <param name="cliente"></param>
        [HttpPost]
        public ActionResult ViewPopupClientes(ClienteModel cliente)
        {


            List<int> list = new List<int>();
            if (cliente.SelectedClientes != null)
            {
                foreach (var currentData in cliente.SelectedClientes)
                {

                    list.Add(currentData);

                }
                Session["Parametros_Cliente"] = list;

                Session["ClientesSelecionados"] = cliente.ObtemClientesSelecionados(list);

            }
            else
            {
                Session["Parametros_Cliente"] = null;

                Session["ClientesSelecionados"] = "Todos";
            }



            return null;
        }


        public ActionResult SelecaoGrupo()
        {
            if (Session["Isn_Empresa"] != null)
            {
                ClienteModel cliente = new ClienteModel();

                if (Session["Grupos_Selecionados"] != null)
                {
                    cliente.IsnGruposSelecionados = (List<int>)Session["Grupos_Selecionados"];
                }

                cliente.ListaDeGrupos = cliente.listaGrupos((int)(Session["Isn_Empresa"]));


                return View(cliente);

            }
            else
            {

                return null;
            }
        }

        [HttpPost]
        public ActionResult SelecaoGrupo(ClienteModel cliente)
        {

            List<int> gruposSelecionados = new List<int>();
            List<int> clientesListadosPorGrupo = new List<int>();

            foreach (var currentData in cliente.SelectedGrupos)
            {
                gruposSelecionados.Add(currentData);
            }


            Session["Grupos_Selecionados"] = gruposSelecionados;

            clientesListadosPorGrupo = cliente.ObtemClientePeloGrupo(gruposSelecionados);

            Session["Parametros_Cliente"] = clientesListadosPorGrupo;

            Session["ClientesSelecionados"] = cliente.ObtemClientesSelecionados(clientesListadosPorGrupo);

            return null;
        }


        /// <summary>
        /// Responsável por exibir um popup com as empresas cadastradas no sistema
        /// visando permitir a alteração de ambiente (semelhante ao "ADMIN" do Lysis)
        /// </summary>
        /// <returns>instancia de usuário com a lista de empresas preenchidas</returns>
        public ActionResult SelecaoEmpresa()
        {
            if (Session["Isn_Empresa"] != null)
            {
                UsuarioModel usuario = new UsuarioModel();
                usuario.ListaDeEmpresa = usuario.listaEmpresa();

                return View(usuario);
            }
            else
            {

                return null;
            }

        }

        /// <summary>
        /// Recebe um objeto usuario com a lista preenchida. Pega o usuário selecionado e se loga no sistema a partir dele.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>Redirecionamento para a view de ativação de perfil</returns>
        [HttpPost]
        public ActionResult SelecaoEmpresa(UsuarioModel usuario)
        {

            foreach (var currentData in usuario.ListaDeEmpresa)
            {
                if (currentData.Selected == true)
                {
                    Session["Isn_Empresa_Ativacao_Perfil"] = currentData.Isn_Pessoa;
                    return RedirectToAction("AtivarPerfil");
                }
            }

            return null;
        }

        /// <summary>
        /// Preenche a lista de usuários da empresa selecionada para ativação de perfil e exibe em um popup
        /// </summary>
        /// <param name="usuario"></param>
        public ActionResult AtivarPerfil()
        {
            if (Session["Isn_Empresa"] != null)
            {
                UsuarioModel usuario = new UsuarioModel();
                usuario.ListaDeUsuarios = usuario.listaUsuarios((int)(Session["Isn_Empresa_Ativacao_Perfil"]));

                return View(usuario);
            }
            else
            {

                return null;
            }

        }


        /// <summary>
        /// Pega o cliente selecionado, limpa as variaveis de sessão, efetua login com o usuário selecionado
        /// </summary>
        /// <param name="usuario"></param>
        [HttpPost]
        public ActionResult AtivarPerfil(UsuarioModel usuario)
        {

            foreach (var currentData in usuario.ListaDeUsuarios)
            {
                if (currentData.Selected == true)
                {


                    Session["Usuario"] = currentData.Dsc_Login;
                    Session["Isn_Empresa"] = (int)(Session["Isn_Empresa_Ativacao_Perfil"]);
                    Session["Dsc_Empresa"] = currentData.Dsc_Empresa;
                    //Validando se o novo cliente é admin ou não.
                    Session["Habilita_Admin"] = usuario.validaUsuarioAdmin(currentData.Dsc_Login);
                    //Verificando se o novo cliente tem acesso as visões de informações adicionais.
                    Session["Habilita_Inf_Adicional"] = usuario.habilitaInformacaoAdicional((int)(Session["Isn_Empresa_Ativacao_Perfil"]));
                    //Limpando os as variaveis de sessão dos parametros de clientes, pois a partir de agora, é uma nova sessão com um novo usuário
                    Session["Parametros_Cliente"] = null;

                    Session["ClientesSelecionados"] = null;
                }
            }

            return null;
        }

        

        [HttpGet]
        public async Task<ViewResult> PausarCapacidade(string TipoOperacao)
        {


            if (TipoOperacao == "PausaFimDoDia")
            {
                CapacidadeDedicadaModel CapacidadeDedicada = new CapacidadeDedicadaModel();

                await CapacidadeDedicada.ObterDetalhesCapacidadeDedicada();

                //Capacidade Dedicada Pausada

                if (CapacidadeDedicada.Status == "Succeeded")
                {


                    StreamWriter arquivoLog = null;

                    try
                    {

                        CapacidadeDedicada.Pausar();

                        ////Colocando o caminho fisico e o nome do arquivo a ser criado
                        //finalizando com .txt
                        string CaminhoNome = PathLogs + "LogPausaAgendadaFimDoDia_" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt";

                        //utilizando o metodo para criar um arquivo texto
                        //e associando o caminho e nome ao metodo
                        arquivoLog = System.IO.File.CreateText(CaminhoNome);

                        //aqui, exemplo de escrever no arquivo texto
                        //como se fossemos criar um recibo de pagamento 

                        //escrevendo o titulo
                        arquivoLog.WriteLine("Inicio da operação");
                        //pulando linha sem escrita
                        arquivoLog.WriteLine();
                        arquivoLog.WriteLine();
                        //escrevendo conteúdo do recibo
                        arquivoLog.WriteLine("Pausa realizada em Data/hora atual: " + DateTime.Now.ToString());



                        arquivoLog.WriteLine();

                    }
                    catch (Exception ex)
                    {
                        arquivoLog.WriteLine("Ocorreu um erro ao pausar capacidade dedicada. Erro: " + ex.Message + " - Stack Trace: " + ex.StackTrace);
                    }
                    finally
                    {
                        //fechando o arquivo texto com o método .Close()
                        arquivoLog.WriteLine("Fim da operação");
                        arquivoLog.WriteLine("__________________________________________________");
                        arquivoLog.Close();
                    }

                    CapacidadeDedicada.Esperar(60);

                }
            } else if (TipoOperacao == "PausaPosAtualizacaoVisoes")
            {
                CapacidadeDedicadaModel CapacidadeDedicada = new CapacidadeDedicadaModel();

                await CapacidadeDedicada.ObterDetalhesCapacidadeDedicada();

                //Capacidade Dedicada Pausada

                if (CapacidadeDedicada.Status == "Succeeded")
                {


                    StreamWriter arquivoLog = null;

                    try
                    {

                        CapacidadeDedicada.Pausar();

                        ////Colocando o caminho fisico e o nome do arquivo a ser criado
                        //finalizando com .txt
                        string CaminhoNome = PathLogs + "LogPausaAgendadaPosAtualizacaoVisoes_" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt";

                        //utilizando o metodo para criar um arquivo texto
                        //e associando o caminho e nome ao metodo
                        arquivoLog = System.IO.File.CreateText(CaminhoNome);

                        //aqui, exemplo de escrever no arquivo texto
                        //como se fossemos criar um recibo de pagamento 

                        //escrevendo o titulo
                        arquivoLog.WriteLine("Inicio da operação");
                        //pulando linha sem escrita
                        arquivoLog.WriteLine();
                        arquivoLog.WriteLine();
                        //escrevendo conteúdo do recibo
                        arquivoLog.WriteLine("Pausa realizada em Data/hora atual: " + DateTime.Now.ToString());



                        arquivoLog.WriteLine();

                    }
                    catch (Exception ex)
                    {
                        arquivoLog.WriteLine("Ocorreu um erro ao pausar capacidade dedicada. Erro: " + ex.Message + " - Stack Trace: " + ex.StackTrace);
                    }
                    finally
                    {
                        //fechando o arquivo texto com o método .Close()
                        arquivoLog.WriteLine("Fim da operação");
                        arquivoLog.WriteLine("__________________________________________________");
                        arquivoLog.Close();
                    }

                    CapacidadeDedicada.Esperar(60);
                }
            }

           
            return null;

        }




        [HttpGet]
        public async Task<ViewResult> IniciarCapacidade()
        {


                CapacidadeDedicadaModel CapacidadeDedicada = new CapacidadeDedicadaModel();

                await CapacidadeDedicada.ObterDetalhesCapacidadeDedicada();

                //Capacidade Dedicada Pausada

                if (CapacidadeDedicada.Status == "Paused")
                {


                    StreamWriter arquivoLog = null;

                    try
                    {

                        CapacidadeDedicada.Iniciar();

                        ////Colocando o caminho fisico e o nome do arquivo a ser criado
                        //finalizando com .txt
                        string CaminhoNome = PathLogs + "LogInicioAgendadoAntesAtualizacao_" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt";

                        //utilizando o metodo para criar um arquivo texto
                        //e associando o caminho e nome ao metodo
                        arquivoLog = System.IO.File.CreateText(CaminhoNome);

                        //aqui, exemplo de escrever no arquivo texto
                        //como se fossemos criar um recibo de pagamento 

                        //escrevendo o titulo
                        arquivoLog.WriteLine("Inicio da operação");
                        //pulando linha sem escrita
                        arquivoLog.WriteLine();
                        arquivoLog.WriteLine();
                        //escrevendo conteúdo do recibo
                        arquivoLog.WriteLine("Inicio realizado em Data/hora atual: " + DateTime.Now.ToString());



                        arquivoLog.WriteLine();

                    }
                    catch (Exception ex)
                    {
                        arquivoLog.WriteLine("Ocorreu um erro ao iniciar capacidade dedicada. Erro: " + ex.Message + " - Stack Trace: " + ex.StackTrace);
                    }
                    finally
                    {
                        //fechando o arquivo texto com o método .Close()
                        arquivoLog.WriteLine("Fim da operação");
                        arquivoLog.WriteLine("__________________________________________________");
                        arquivoLog.Close();
                    }

                    CapacidadeDedicada.Esperar(60);

                }
            


            return null;

        }
    }
}


