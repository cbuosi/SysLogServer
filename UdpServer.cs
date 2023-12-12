using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static tSysLog;
using System.Text.Json.Serialization;
using System.Runtime.Serialization.Json;
using System.Data;
using System.Data.SqlClient;
using Dapper.Contrib.Extensions;
using MySqlConnector;

class UdpServer
{

    enum eTipoLog
    {
        DESCONHECIDO = 0,
        Arquivo = 1,
        MSSQL = 2,
        MySQL = 3
    }


#if DEBUG
    public bool bLoga = false;
#else
    public bool bLoga = false;
#endif

    private int PortaUDP = 0;
    //private eTipoLog cTipoLog = eTipoLog.DESCONHECIDO;

    public string LOG_ARQUIVO { get; set; }
    public string LOG_BANCO { get; set; }
    public string SERVIDOR { get; set; }
    public string BANCO { get; set; }
    public string USUARIO { get; set; }
    public string SENHA { get; set; }


    private UdpClient? udpServer;
    private Thread? listenerThread;

    public UdpServer(string _LOG_ARQUIVO)
    {
        PortaUDP = 514;

        LOG_ARQUIVO = _LOG_ARQUIVO;
        LOG_BANCO = "";
        SERVIDOR = "";
        BANCO = "";
        USUARIO = "";
        SENHA = "";

        Log($"Construtor UdpServer. Porta Padrão 512");
    }

    public UdpServer(int _PortaUDP, string _LOG_ARQUIVO)
    {
        PortaUDP = _PortaUDP;
        LOG_ARQUIVO = _LOG_ARQUIVO;
        LOG_BANCO = "";
        SERVIDOR = "";
        BANCO = "";
        USUARIO = "";
        SENHA = "";
        Log($"Construtor UdpServer. Porta: {_PortaUDP.ToString()}");
    }

    public int ProcessarMensagensSysLog()
    {
        try
        {
            //setando porta x
            Log($"P1 setando porta {PortaUDP}");
            udpServer = new UdpClient(PortaUDP);
            // configurando Threads
            Log("P2 configurando Threads");
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            //iniciando leitura
            Log("P3 iniciando leitura");
            listenerThread.Start();
            return 0;
        }
        catch (SocketException exx)
        {
            //erro 1 (porta?)
            Log("Erro de socket: " + exx.Message);
            return -1;
        }
        catch (Exception ex)
        {
            //erro 2 (generico)
            Log("Erro: " + ex.Message);
            return -2;
        }

    }

    private void ListenForClients()
    {
        try
        {
            //iniciando leitura
            Log("L1 - iniciando leitura 2");
            while (true)
            {
                if (udpServer == null)
                {
                    Log("erro 4");
                    return;
                }

                Log("L2 - Monitorando todos endereços...");
                IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
                Log("L3 - recebendo...");
                byte[] receivedBytes = udpServer.Receive(ref remoteIp);

                // Criar uma nova thread para processar a conexão
                Log("L4 - Criar uma nova thread para processar a conexão");
                Thread clientThread = new Thread(() => HandleClient(remoteIp, receivedBytes));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Log(ex.Message);
        }
    }

    private void HandleClient(IPEndPoint clientEndPoint, byte[] data)
    {
        // Processar os dados recebidos aqui
        Log($"H1 - Processar os dados recebidos aqui");
        string receivedMessage = System.Text.Encoding.ASCII.GetString(data);
        Log($"H2 - Recebido de {clientEndPoint}: {receivedMessage}");

        tSysLog oSysLog = new tSysLog(receivedMessage, clientEndPoint);

        //aqui loga..

        if (LOG_ARQUIVO.Trim() != "")
        {
            //
            LogArquivoTXT(oSysLog);
            //
        }

        //
        string CONNECTION_MS_SQL = "Server=192.168.1.120;Database=SysLogServer;User Id=sa;Password=123@qweasd;";
        string CONNECTION_MY_SQL = "Server=192.168.1.93; User ID=root; Password=123qweasd; Database=SysLogServer";
        //
        using (IDbConnection db = new SqlConnection(CONNECTION_MS_SQL))
        {
            db.Insert<tSysLog>(oSysLog);
        }

        using (IDbConnection db = new MySqlConnection(CONNECTION_MY_SQL))
        {
            db.Insert<tSysLog>(oSysLog);
        }


        if (oSysLog.bProc == eSimNao.SIM)
        {
            /* Time    IP  Host    Facility    Priority    Tag MEssage  */
            Log($"{oSysLog.Time} {oSysLog.AddressFamily} {oSysLog.Host} {oSysLog.rFacility} {oSysLog.rSeverity} {oSysLog.Ident} {oSysLog.Message}");

            Log("oSysLog.ID.................: " + oSysLog.ID.ToString());
            Log("oSysLog.bProc..............: " + oSysLog.bProc.ToString());
            Log("oSysLog.Time...............: " + oSysLog.Time.ToString());
            Log("oSysLog.cFacility..........: " + oSysLog.cFacility.ToString());
            Log("oSysLog.rFacility..........: " + oSysLog.rFacility.ToString());
            Log("oSysLog.cSeverity..........: " + oSysLog.cSeverity.ToString());
            Log("oSysLog.rSeverity..........: " + oSysLog.rSeverity.ToString());
            Log("oSysLog.RawMessage.........: " + oSysLog.RawMessage.ToString());
            Log("oSysLog.Host...............: " + oSysLog.Host.ToString());
            Log("oSysLog.IP.................: " + oSysLog.IP.ToString());
            Log("oSysLog.AddressFamily......: " + oSysLog.AddressFamily.ToString());
            Log("oSysLog.Ident..............: " + oSysLog.Ident.ToString());
            Log("oSysLog.Pid................: " + oSysLog.Pid.ToString());
            Log("oSysLog.Message............: " + oSysLog.Message.ToString());
            Log("oSysLog.MessageProc........: " + oSysLog.MessageProc.ToString());

            ExibeConsole(oSysLog);

        }
        else
        {
            Log("Erro ao processar msg");
        }
        //Log($"Recebido de {clientEndPoint}: {receivedMessage}");
    }

    private void LogArquivoTXT(string _txt)
    {
        //
        try
        {
            using (StreamWriter writer = new StreamWriter(LOG_ARQUIVO, true))
            {
                writer.WriteLine(_txt);
            }
            //
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex.ToString());
        }
        //
    }

    private void LogArquivoTXT(tSysLog _oSysLog)
    {
        //
        try
        {
            //
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(tSysLog));
            serializer.WriteObject(stream, _oSysLog);
            // Converter o MemoryStream para uma string
            string json = Encoding.UTF8.GetString(stream.ToArray());
            using (StreamWriter writer = new StreamWriter(LOG_ARQUIVO, true))
            {
                writer.WriteLine(json);
            }
            //
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex.ToString());
        }
        //
    }
    //
    private void ExibeConsole(tSysLog oSysLog)
    {

        ConsoleColor CorFrente = ConsoleColor.Black;
        ConsoleColor CorFundo = ConsoleColor.White;

        if (oSysLog.cSeverity == eSysLogSeverity.EMERGENCY)
        {
            CorFrente = ConsoleColor.White;
            CorFundo = ConsoleColor.DarkRed;
        }


        if (oSysLog.cSeverity == eSysLogSeverity.ALERT)
        {
            CorFrente = ConsoleColor.White;
            CorFundo = ConsoleColor.Red;
        }

        if (oSysLog.cSeverity == eSysLogSeverity.CRITICAL)
        {
            CorFrente = ConsoleColor.Black;
            CorFundo = ConsoleColor.DarkRed;
        }

        if (oSysLog.cSeverity == eSysLogSeverity.ERROR)
        {
            CorFrente = ConsoleColor.Black;
            CorFundo = ConsoleColor.Red;
        }

        if (oSysLog.cSeverity == eSysLogSeverity.WARNING)
        {
            CorFrente = ConsoleColor.Black;
            CorFundo = ConsoleColor.Yellow;
        }

        if (oSysLog.cSeverity == eSysLogSeverity.DEBUG)
        {
            CorFrente = ConsoleColor.Black;
            CorFundo = ConsoleColor.Gray;
        }

        if (oSysLog.cSeverity == eSysLogSeverity.NOTICE)
        {
            CorFrente = ConsoleColor.White;
            CorFundo = ConsoleColor.Black;
        }

        if (oSysLog.cSeverity == eSysLogSeverity.INFORMATIONAL)
        {
            CorFrente = ConsoleColor.Black;
            CorFundo = ConsoleColor.White;
        }

        Console.ForegroundColor = CorFrente;
        Console.BackgroundColor = CorFundo;
        Console.Write($"[ {oSysLog.cFacility} | {oSysLog.cSeverity} | {oSysLog.Message} ]");
        Console.ResetColor();
        Console.WriteLine("");
        return;


    }
    //
    public void Stop()
    {
        if (udpServer == null)
        {
            return;
        }
        Log("S1 - server fechar");
        udpServer.Close();
        //listenerThread.Abort();
    }
    //
    private void Log(string _strLog)
    {
        if (bLoga)
        {
            Console.WriteLine(_strLog);
        }
        System.Diagnostics.Debug.Print(_strLog);
        LogArquivoTXT(_strLog);
    }
    //
}


