using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

static class clsUtil
{
    const string ARQUIVO_DATABASE_CONFIG = "Config.xml";

    public static void Log(string _strLog)
    {
        System.Diagnostics.Debug.Print(_strLog);
        Console.WriteLine(_strLog);
    }


    internal static string ObterConfig(string chave)
    {
        XmlDocument xmlConfig = new XmlDocument();
        string strPath;

        try
        {

            strPath = ""; // HttpContext.Current.Server.MapPath("~");

            xmlConfig = new XmlDocument();
            xmlConfig.Load(Path.Combine(strPath, ARQUIVO_DATABASE_CONFIG));

#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
            return xmlConfig.DocumentElement.SelectSingleNode("//CONFIG").SelectSingleNode("//" + chave).Attributes["valor"].InnerText.ToString();
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.

        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex.Message);
            return "";
        }
    }

}
