using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FindingRulesVersionsDates
{
    class Program
    {
        static void Main(string[] args)
        {

            string sDataSrc = "192.168.200.4";
            string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
            string sSql = "select c,hokc from hok_previousversions_legislationdates where isprocessed=0 order by hokc,c";
            SqlConnection connRead = new SqlConnection(sConnStr);
            try
            {
                connRead.Open();
                SqlCommand cmdRead = new SqlCommand(sSql, connRead);
                SqlDataReader dataReader = cmdRead.ExecuteReader();
                while (dataReader.Read())
                {
                    string sUrl = "https://www.lawdata.co.il/lawdata_face_lift_test/gethok.asp?flnm=" + dataReader.GetValue(1) + "_" + dataReader.GetValue(0) + "&v=1";
                    ICollection<HtmlNode> arNodes = Helper.GetAllHtmlClausesInFileLoadedFromWeb(sUrl);
                    DateTime oRuleDt = new DateTime(1900,1,1);
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
                            DateTime oDt= Convert.ToDateTime(sDate, new CultureInfo("he"));
                            if (oRuleDt == null || DateTime.Compare(oDt, oRuleDt) > 0) oRuleDt = oDt;
                        }
                    }
                    if (DateTime.Compare(oRuleDt, new DateTime(1900, 1, 1)) > 0)
                    {
                        Helper.WriteToDB(Convert.ToInt32(dataReader.GetValue(0)), oRuleDt.ToString("dd/MM/yyyy"));
                    }
                }
                dataReader.Close();
                cmdRead.Dispose();
                connRead.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("can't open connection to db. error is " + ex.Message);
            }
        }
    }
}
