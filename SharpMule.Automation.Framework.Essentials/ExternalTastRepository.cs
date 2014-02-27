using System;
using System.Xml.Serialization;
using Expedia.Automation.SOA.Serializer;
using Expedia.Automation.SOA.Message;

using Expedia.Automation.ExpSOATemplates.Hotel.LodgingInventoryShoppingService;
using Expedia.Automation.ExpSOATemplates.Hotel.LodgingInventoryShoppingService.Schemas.AvailSummaryV3_0;
using Expedia.Automation.ExpSOATemplates.Hotel.LodgingInventoryShoppingService.Schemas.AvailSummaryV2_0;
using Expedia.Automation.Test.Hotels.LodgingInventoryShoppingService.R4;
using Expedia.Test.Framework;
using Expedia.Automation.Web.Hotels;
using Expedia.Automation.Web.Hotels.LodgingInventoryShoppingService;
using System.Collections.Generic;
using LiquidTechnologies.FastInfoset;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System.Net;
using System.IO;





namespace SharpMule.Automation.Framework.Essentials
{
    public class ExternalTestRepository
    {
        
        public class HttpOperations
        {

            public bool SendFastInfoSet(string endpointurl, string xmldata,string method, int timeout,out string response)
            {
                response = String.Empty; 
                try
                {
                        
                        //Convert Xml to FastInfoSet
                        byte[] fastInfoSet = XmlToFastInfoSet(xmldata);

                        string applicationtype = "application/fastinfoset";
                        HttpWebRequest req = RequestRoutine(endpointurl, fastInfoSet, method, timeout, applicationtype);
                       
                        MemoryStream msRec = new MemoryStream(); 
                        CopyData(req.GetResponse().GetResponseStream(), msRec);

                        // Convert FastInfoSet to XmlString
                        response = FastInfoSetToXML(msRec); 
                      

                }
                catch(WebException ex)
                {
                    try
                    {
                        response = (new StreamReader(ex.Response.GetResponseStream())).ReadToEnd();
                        TestInterfaceEngine.Log.LogError(response);
                    }
                    catch
                    {
                        TestInterfaceEngine.Log.LogError("Server seems to be unreachable. Check the endpoint. "); 
                    }
                    return false; 
                }

                return true; 

            }

            // This will make the HttpRequest and send the data as fast infoset
            private HttpWebRequest RequestRoutine(string requestUriString, byte[] fastinfosetdata,string method, int timeout, string contentType)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.Timeout = timeout;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50215)";
                request.AllowAutoRedirect = true;
                request.Method = method;
                request.Headers.Add("SOAPAction", "\"\"");
                request.ContentType = contentType;
                request.ContentLength = fastinfosetdata.Length;

                using (Stream netStream = request.GetRequestStream())
                {
                    try
                    {
                        netStream.Write(fastinfosetdata, 0, fastinfosetdata.Length);
                    }
                    finally
                    {
                        netStream.Close();
                    }
                }

               
              

                return request;
            }

            //public byte[] XmlToFastInfoSetLiquid(string xmldata)
            //{
            //    try
            //    {
            //        MemoryStream stream = new MemoryStream();
                 
            //        XmlDocument doc = new XmlDocument();
            //        doc.LoadXml(xmldata); 
           
            //        XmlWriter fiWriter = XmlWriter.Create(new FIWriter(stream)); 
            //        doc.WriteTo(fiWriter);
                    
            //        byte[] bytes = stream.ToArray();

            //        return bytes;
            //    }
            //    catch
            //    {
            //        return null;
            //    }

            //}
            //public string FastInfoSetToXmlLiquid(MemoryStream fiStream)
            //{
            //    try
            //    {
            //        Expedia.Test.Framework.Oss.OssWrapper osswrapper = new Expedia.Test.Framework.Oss.OssWrapper();
            //        string text = osswrapper.FastInfoSet2Xml(fiStream.ToArray());
            //        XmlDocument doc = new XmlDocument();
            //        Noemax.FastInfoset.XmlFastInfosetReader rd = new Noemax.FastInfoset.XmlFastInfosetReader();
               
                    
            //        rd.ReadContentAsBase64(fiStream.ToArray(),0, fiStream.ToArray().Length);

                    
            //        XmlReader fiReader = XmlReader.Create(rd,null);
            //        doc.Load(fiReader);
            //        fiReader.Close();

            //        return doc.InnerXml; 
                    
            //    }
            //    catch
            //    {
            //        return String.Empty;
            //    }

            //}
            public byte[] XmlToFastInfoSet(string xmldata)
            {
               
                try
                {
                    Expedia.Test.Framework.Oss.OssWrapper osswrapper = new Expedia.Test.Framework.Oss.OssWrapper();

                    byte[] fastInfoSet = osswrapper.Xml2FastInfoSet(xmldata); //memStr.ToArray();

                    return fastInfoSet;
                }
                catch (Exception e)
                {
                    TestInterfaceEngine.Log.LogDebug("Converting Xml to Fast Info Set has failed. " + e.Message);
                    return null;
                }


            }
            public string FastInfoSetToXML(MemoryStream fiStream)
            {
                try
                {
                    Expedia.Test.Framework.Oss.OssWrapper osswrapper = new Expedia.Test.Framework.Oss.OssWrapper();
                    string xml = osswrapper.FastInfoSet2Xml(fiStream.ToArray()); 

                    return XDocument.Parse(xml).ToString();
                }
                catch (Exception e)
                {
                    TestInterfaceEngine.Log.LogError("Converting Fast Info Set to XML has failed. " + e.Message);
                    return String.Empty;
                }


            }
            // This will convert ResponseStream to MemoryStream
            private void CopyData(Stream FromStream, Stream ToStream)
            {
                int intBytesRead = 0;
                byte[] bytes = new byte[0x1001];
                for (intBytesRead = FromStream.Read(bytes, 0, 0x1000); intBytesRead > 0; intBytesRead = FromStream.Read(bytes, 0, 0x1000))
                {
                    ToStream.Write(bytes, 0, intBytesRead);
                }
            }




        }

       
    }
}
