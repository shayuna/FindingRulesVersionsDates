using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace FindingRulesVersionsDates
{
    class Program
    {
        private static Mutex mutex;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        static void Main(string[] args)
        {
            bool bCreateNew;
            const string sAppName = "FindingRulesVersionsDates";
            mutex = new Mutex(true, sAppName, out bCreateNew);
            if (!bCreateNew) Environment.Exit(0);

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_MINIMIZE);

            bool bTest = false;

            string sDataSrc = "192.168.200.4";
            string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
            string sSql = "";
            if (bTest)
            {
                sSql = "select top 100 c,hokc from hok_previousversions_legislationdates where isprocessed=1 and dtindoc='01/01/1900' order by hokc,c";
            }
            else
            {
                sSql = "select top 100 c,hokc from hok_previousversions_legislationdates where isprocessed=0 order by hokc,c";
            }
            SqlConnection connRead = new SqlConnection(sConnStr);
            try
            {
                connRead.Open();
                SqlCommand cmdRead = new SqlCommand(sSql, connRead);
                SqlDataReader dataReader = cmdRead.ExecuteReader();
                string sPath = "";
                while (dataReader.Read())
                {
                    if (bTest)
                    {
                        sPath = "https://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + dataReader.GetValue(1) + "_" + dataReader.GetValue(0) + "&v=1";
                    }
                    else
                    {
                        sPath = "d://inetpub//wwwroot//upload//hok//" + dataReader.GetValue(1) + "_" + dataReader.GetValue(0) + ".htm";
                        if (!File.Exists(sPath)) sPath = "d://inetpub//wwwroot//upload//hok//" + dataReader.GetValue(1) + "_" + dataReader.GetValue(0) + ".html";
                    }

                    DateTime oRuleDt = new DateTime(1900, 1, 1);
                    ICollection<HtmlNode> arNodes;
                    if (File.Exists(sPath) || bTest)
                    {
                        if (bTest)
                        {
                            arNodes = Helper.GetAllHtmlClausesInHtmlDocument(Helper.GetHtmlDocFromUrl(sPath));
                        }
                        else
                        {
                            arNodes = Helper.GetAllHtmlClausesInHtmlDocument(Helper.GetHtmlDocFromDisk(sPath));
                        }
                        foreach (HtmlNode eNode in arNodes)
                        {
                            string sText = eNode.InnerText;
                            //                        string sPattern ="(ס\"ח|ק\"ת)"+"\\s+\\d+"+"[\\s,]*[א-ת\"]+"+"[\\s,]*[()]"+"([.0-9]+)"+"[()]";
                            string sPattern = "(ס\"ח|ק\"ת)" + "\\s+\\d+" + "[\\s,]*[א-ת\"]+" + "[\\s,]*[()]" + "([.,\\/0-9]+)" + "[()]";
                            Regex re = new Regex(sPattern);
                            MatchCollection arMatches = re.Matches(sText);
                            foreach (Match mt in arMatches)
                            {
                                string sDate = mt.Groups[2].Value;
                                DateTime oDt;
                                if (DateTime.TryParse(sDate, new CultureInfo("he"), DateTimeStyles.AdjustToUniversal, out oDt))
                                {
                                    if (oRuleDt == null || DateTime.Compare(oDt, oRuleDt) > 0) oRuleDt = oDt;
                                }
                            }
                        }
                    }
                    Helper.WriteToDB(Convert.ToInt32(dataReader.GetValue(0)), oRuleDt.ToString("dd/MM/yyyy"));
                }
                dataReader.Close();
                cmdRead.Dispose();
                connRead.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("can't open connection to db. error is " + ex.Message);
                Console.ReadKey();
            }
        }
    }
}
