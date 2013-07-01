using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace PeakCalculater
{
    class HttpLib
    {
        public static List<string> GetHttpRespsonse(string webURL)
        {
            List<string> lstResponse = new List<string>();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(webURL);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                    {
                        string strTmp = sr.ReadLine();

                        while (strTmp != null)
                        {
                            lstResponse.Add(strTmp);
                            strTmp = sr.ReadLine();
                        }

                        sr.Close();
                    }

                    response.Close();
                }
            }
            catch (Exception e)
            {
                lstResponse = new List<string>();
            }
           
             return lstResponse; 
        }
    }
}
