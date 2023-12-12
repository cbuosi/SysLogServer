using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Globalization;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using System.Web;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Drawing;

public class SysLogServer
{
    const string VERSAO = "2.01";
    static public string PORTA = "";

    public static int Main(String[] args)
    {

        string LOG_ARQUIVO = "";
        string LOG_BANCO = "";
        string SERVIDOR = "";
        string BANCO = "";
        string USUARIO = "";
        string SENHA = "";

        try
        {

            DesenhaLogo();

            PORTA = clsUtil.ObterConfig("PORTA");

            LOG_ARQUIVO = clsUtil.ObterConfig("LOG_ARQUIVO");

            LOG_BANCO = clsUtil.ObterConfig("LOG_BANCO");
            SERVIDOR = clsUtil.ObterConfig("SERVIDOR");
            BANCO = clsUtil.ObterConfig("BANCO");
            USUARIO = clsUtil.ObterConfig("USUARIO");
            SENHA = clsUtil.ObterConfig("SENHA");

            clsUtil.Log($"Porta UDP..............: [{PORTA}]");
            clsUtil.Log($"--------------------------------------------------------");
            clsUtil.Log($"LOG_ARQUIVO............: [{LOG_ARQUIVO}]");
            clsUtil.Log($"--------------------------------------------------------");
            clsUtil.Log($"LOG_BANCO..............: [{LOG_BANCO}]");
            clsUtil.Log($"SERVIDOR...............: [{SERVIDOR}]");
            clsUtil.Log($"BANCO..................: [{BANCO}]");
            clsUtil.Log($"USUARIO................: [{USUARIO}]");
            clsUtil.Log($"SENHA..................: [{SENHA}]");
            clsUtil.Log($"--------------------------------------------------------");

           /*

    RFC-3164 Log
    This incoming event:
    <6>Feb 28 12:00:00 192.168.0.1 fluentd[11111]: [error] Syslog test
    is parsed as:
    time:
    1362020400 (Feb 28 12:00:00)

    record:
    {
      "pri"    : 6,
      cFacility - KERN = 0,USER = 1,MAIL = 2,DAEMON = 3, etc
      rFacility - KERN = 0,USER = 1,MAIL = 2,DAEMON = 3, etc
      cSeverity - EMERGENCY = 0, ALERT = 1, CRITICAL = 2, ERROR = 3, etc
      rSeverity - "EMERGENCY", "ALERT", "CRITICAL" = 2, ERROR = 3, etc
      MsgOriginal   
      "host"   : "192.168.0.1:1234",
      "ident"  : "fluentd",
      "pid"    : "11111",
      "message": "[error] Syslog test"
    }
             */

#if false
            #region "TESTE_CORES"
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("[   Priority  = Emergency     ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write("[   Priority  = Alert         ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("[   Priority  = Critico       ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write("[   Priority  = Erro          ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.Write("[   Priority  = Warning       ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write("[   Priority  = Debug         ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("[   Priority  = Notice        ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write("[   Priority  = Info          ]");
            Console.ResetColor(); Console.WriteLine("");


            Console.Title = $"SysLogServer v.{VERSAO}";


            #endregion

            int larguraConsole = Console.WindowWidth;
            int alturaConsole = Console.WindowHeight;

            Console.WriteLine($"Largura do Console (X): {larguraConsole.ToString("0000")} caracteres");
            Console.WriteLine($"Altura do Console  (Y): {alturaConsole.ToString("0000")} linhas");
#endif



            //Console.WriteLine("\x1b[32mEste texto está em verde.\x1b[0maaaaa");
            //Console.WriteLine("\x1b[33;44m  Este texto está em amarelo com fundo azul.\x1b[0maaaaaa");
            //Console.WriteLine("Cores disponíveis em ConsoleColor:");
            //foreach (ConsoleColor color in Enum.GetValues(typeof(ConsoleColor)))
            //{
            //    Console.ForegroundColor = color;
            //    Console.WriteLine($"[{color}] Texto na cor {color}");
            //}
            //
            //// Restaura as cores padrão
            //Console.ResetColor();
            //
            //// Mantém a janela do console aberta
            //Console.ReadLine();

            UdpServer udpServer = new UdpServer(int.Parse(PORTA), LOG_ARQUIVO);

            udpServer.ProcessarMensagensSysLog();

            // Mantém o programa em execução
            Console.ReadLine();

            // Encerra o servidor quando o usuário pressiona Enter
            udpServer.Stop();

            return 0;

        }
        catch (Exception ex)
        {
            clsUtil.Log("Erro: " + ex.Message);
            return -1;
        }

    }

    //private static void Log(string _strLog)
    //{
    //    System.Diagnostics.Debug.Print(_strLog);
    //    Console.WriteLine(_strLog);
    //}


    private static void DesenhaLogo()
    {

        int seed = DateTime.Now.Second;
        Random random = new Random(seed);

        // Gerando números aleatórios usando a instância com a semente
        int numeroAleatorio1 = random.Next(1, 1);
        Console.Title = $"SysLogServer {VERSAO}";

        //numeroAleatorio1 = 99; teste

        if (numeroAleatorio1 == 1)
        {

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            clsUtil.Log($@"================================================================");
            clsUtil.Log($@"   __              __             __                            ");
            clsUtil.Log($@"  / _\_   _ ___   / /  ___   __ _/ _\ ___ _ ____   _____ _ __   ");
            clsUtil.Log($@"  \ \| | | / __| / /  / _ \ / _` \ \ / _ \ '__\ \ / / _ \ '__|  ");
            clsUtil.Log($@"  _\ \ |_| \__ \/ /__| (_) | (_| |\ \  __/ |   \ V /  __/ |     ");
            clsUtil.Log($@"  \__/\__, |___/\____/\___/ \__, \__/\___|_|    \_/ \___|_|     ");
            clsUtil.Log($@"      |___/                 |___/                       v.{VERSAO}  ");
            clsUtil.Log($@"================================================================");
            Console.ResetColor();

            return;
        }

        clsUtil.Log($"===================");
        clsUtil.Log($"SysLogServer {VERSAO}");
        clsUtil.Log($"===================");

    }


}
