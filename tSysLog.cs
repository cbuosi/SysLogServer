using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

[DataContract] [Table("tSysLog")]
public class tSysLog
{

    public enum eSimNao
    { 
        DESCONHECIDO = 0,
        SIM = 1,
        NAO = 2
    }

    public enum eSysLogFacility
    {
        DESCONHECIDO = -1,
        //--------------------------
        KERN = 0,
        USER = 1,
        MAIL = 2,
        DAEMON = 3,
        AUTH = 4,
        SYSLOG = 5,
        LPR = 6,
        NEWS = 7,
        UUCP = 8,
        CRON = 9,
        AUTHPRIV = 10,
        FTP = 11,
        NTP = 12,
        SECURITY = 13,
        CONSOLE = 14,
        SOLARIS = 15,
        LOCAL0 = 16,
        LOCAL1 = 17,
        LOCAL2 = 18,
        LOCAL3 = 19,
        LOCAL4 = 20,
        LOCAL5 = 21,
        LOCAL6 = 22,
        LOCAL7 = 23
    }

    public enum eSysLogSeverity
    {
        DESCONHECIDO = -1,
        //--------------------------
        EMERGENCY = 0,
        ALERT = 1,
        CRITICAL = 2,
        ERROR = 3,
        WARNING = 4,
        NOTICE = 5,
        INFORMATIONAL = 6,
        DEBUG = 7
    }


    [DataMember][Key] public int ID { get; set; }
    [DataMember] public eSimNao bProc { get; set; }
    [DataMember] public string Time { get; set; }
    [DataMember] public eSysLogFacility cFacility { get; set; }
    [DataMember] public string rFacility { get; set; }
    [DataMember] public eSysLogSeverity cSeverity { get; set; }
    [DataMember] public string rSeverity { get; set; }
    [DataMember] public string RawMessage { get; set; }
    [DataMember] public string Host { get; set; }
    [DataMember] public string IP { get; set; }
    [DataMember] public string AddressFamily { get; set; }
    [DataMember] public string Ident { get; set; }
    [DataMember] public string Pid { get; set; }
    [DataMember] public string Message { get; set; }
    [DataMember] public string MessageProc { get; set; }

    public tSysLog(string _RawMessage, IPEndPoint oIPEndPoint)
    {

        ID = 0;
        bProc = eSimNao.NAO;
        cFacility = eSysLogFacility.DESCONHECIDO;
        rFacility = ((int)cFacility).ToString() + " - " + cFacility.ToString();
        cSeverity = eSysLogSeverity.DESCONHECIDO;
        rSeverity = ((int)cSeverity).ToString() + " - " + cSeverity.ToString();
        RawMessage = _RawMessage;
        Host = "";
        IP = oIPEndPoint.Address.ToString();
        AddressFamily = oIPEndPoint.Address.AddressFamily.ToString();
        Ident = "";
        Time = "";
        Pid = ""; //Opcional
        Message = "";
        MessageProc = "";

        if (ProcessaSysLog(RawMessage) == true) 
        {
            bProc = eSimNao.SIM;
        }
        else
        {
            bProc = eSimNao.NAO;
        }

        rFacility = ((int)cFacility).ToString() + " - " + cFacility.ToString();
        rSeverity = ((int)cSeverity).ToString() + " - " + cSeverity.ToString();

    }

    private bool ProcessaSysLog(string _RawMessage)
    {
        // ---------------------------------
        string[] strSplit;
        string strTemp;
        // ---------------------------------
        int ini = 0;
        int fin = 0;
        // ---------------------------------
        int i1 = 0;
        int i2 = 0;
        // ---------------------------------
        //int ModoParse = 0;

        int CorteCabeca = 0;

        string PRI = "";

        try
        {


            //<6>Feb 28 12:00:00 192.168.0.1 fluentd[11111]: [error] Syslog test


            if (_RawMessage.StartsWith("<") == false)
            {
                MessageProc = "Erro ao processar msg (1)";
                return false;
            }

            if (_RawMessage.Contains(">") == false)
            {
                MessageProc = "Erro ao processar msg (1)";
                return false;
            }

            strSplit = _RawMessage.Split(new char[] { ' ' });
            if (strSplit.Length <= 3)
            {
                MessageProc = "Erro ao processar msg (1)";
                return false;
            }

            //Message = strSplit[3].Trim();

            //<6>Feb 28 12
            //strTemp = strSplit[0].Trim();
            //<6>Feb 28 12 => //6>Feb 28 12
            strSplit = _RawMessage.Split(">");
            if (strSplit.Length < 2)
            {
                MessageProc = "Erro ao processar msg (1)";
                return false;
            }

            strTemp = strSplit[0].Replace("<", "");
            strTemp = strTemp.Trim();

            //clsSyslog.eSysLogFacility.FTP 11
            //eSysLogSeverity.ERROR 3

            i2 = int.Parse(strTemp);
            i2 = i2 % 8;
            cSeverity = ((eSysLogSeverity)i2);

            //i1 = int.Parse( Math.Round(Decimal.Parse(strTemp) / 8).ToString());
            i1 = int.Parse(strTemp) / 8;
            cFacility = ((eSysLogFacility)i1);

            if (_RawMessage.Contains(":") == false)
            {
                MessageProc = "Erro ao processar msg (3)";
                return false;
            }

            CorteCabeca = EncontrarPosicaoOcorrencia(_RawMessage, ':', 3);

            if (CorteCabeca <= 0)
            {
                MessageProc = "Erro ao processar msg (3)";
                return false;
            }
            //msg
            Message = _RawMessage.Substring(CorteCabeca + 1).Trim();
            PRI = _RawMessage.Substring(0, CorteCabeca).Trim();

            //                   Host          Ident
            //<5>Dec 05 21:58:11 PEREBA_CBuosi PROGRAMA_TESTE
            strSplit = PRI.Split(new char[] { ' ' });

            if (strSplit.Length <= 3)
            {
                MessageProc = "Erro ao processar msg (4)";
                return false;
            }

            Host = strSplit[strSplit.Length - 2];
            Ident = strSplit[strSplit.Length - 1];

            if (Ident.Contains("[") && Ident.Contains("]"))
            {
                ini = Ident.IndexOf("[");
                fin = Ident.IndexOf("]");
                Pid = Ident.Substring(ini + 1, fin - ini - 1).Trim();
            }

            Time = $"{strSplit[0]} {strSplit[1]} {strSplit[2]}";

            Time = Time.Substring(Time.IndexOf('>') + 1);

            //Log($"<{strTemp}> {i1} - {i2} {cFacility.ToString()} - {cSeverity.ToString()} ");
            MessageProc = "Processado OK!";
            return true;

        }
        catch (Exception)
        {
            //Log("Erro: " + ex);
            return false;
        }


    }
    public int EncontrarPosicaoOcorrencia(string input, char caractereProcurado, int ocorrenciaDesejada)
    {

        int ocorrenciasEncontradas = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == caractereProcurado)
            {
                ocorrenciasEncontradas++;

                if (ocorrenciasEncontradas == ocorrenciaDesejada)
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
