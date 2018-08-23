using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using HtmlAgilityPack;

namespace FindingRulesVersionsDates
{
    class Helper
    {
        public static ICollection<HtmlNode> GetAllHtmlClausesInHtmlDocument(HtmlDocument oDoc)
        {
            
            ICollection<HtmlNode> arNodesInDoc = oDoc.QuerySelectorAll(".hearot");
            if (arNodesInDoc.Count==0) arNodesInDoc = oDoc.QuerySelectorAll("td");
            foreach (HtmlNode oNode in arNodesInDoc)
            {
                oNode.SetAttributeValue("class", oNode.GetAttributeValue("class", "") + " " + "comparableItm");
            }
            ICollection<HtmlNode> arNodesToReturn = oDoc.QuerySelectorAll(".comparableItm");
            foreach (HtmlNode oNode in arNodesToReturn)
            {
                oNode.SetAttributeValue("class", oNode.GetAttributeValue("class", "").Replace(" comparableItm", ""));
            }
            return arNodesToReturn;
        }
        public static HtmlDocument GetHtmlDocFromUrl(string sUrl)
        {
            HtmlWeb oHtmlWeb = new HtmlWeb();
            oHtmlWeb.OverrideEncoding = Encoding.GetEncoding(1255);
            return oHtmlWeb.Load(sUrl);
        }
        public static HtmlDocument GetHtmlDocFromDisk(string sPath)
        {
            HtmlDocument oDoc = new HtmlDocument();
            oDoc.Load(sPath);
            return oDoc;
        }
        public static bool WriteToDB(int iC,string sDate)
        {
            bool bRslt = true;
            try
            {
                string sDataSrc = "192.168.200.4";
                string sConnStr = "Initial Catalog=LawData;User ID=sa;Password=;Data Source=" + sDataSrc;
                SqlConnection connWrite = new SqlConnection(sConnStr);
                connWrite.Open();
                SqlCommand cmdWrite = new SqlCommand();
                cmdWrite.Connection = connWrite;
                cmdWrite.CommandType = System.Data.CommandType.Text;
                cmdWrite.CommandText = "update hok_previousversions set dtindoc=convert(datetime,@dt,103) where c=@c";
                cmdWrite.Parameters.AddWithValue("@c", iC);
                cmdWrite.Parameters.AddWithValue("@dt", sDate);
                cmdWrite.ExecuteNonQuery();
                cmdWrite.Dispose();
                connWrite.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("oops. something happened when trying to write data to db. versionc=" + iC + " exception is - " + ex.Message);
                bRslt = false;
            }
            return bRslt;
        }
    }
}
