using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Specialized;


namespace uts_rest_api
{
    /// <summary>
    /// library of classes for use in UTS Rest API
    /// </summary>

    /// <summary>
    /// class for UMLS Terminology Services Rest API
    /// (see documentation.uts.nlm.gov/rest/home.html)
    /// </summary>
    internal class UTSRestClass
    {
        private string _baseURILogin = "https://utslogin.nlm.nih.gov";
        private string _baseUriRequests = "https://uts-ws.nlm.nih.gov/rest";
        //from user profile at uts.nlm.nih.gov
        internal string ApiKey = "";
        //keep track because only lasts 8 hrs
        private DateTime _whenLastTGTObtained = DateTime.MinValue;
        /// <summary>
        /// the Ticket Granting Ticket in URI format
        /// </summary>
        private string _tgtUri = null;
        private bool _tgtIsCurrent
        {
            get
            {
                //say ok if within 7 hrs;  should last 8 hrs normally
                return _whenLastTGTObtained.AddHours(7) > DateTime.Now;
            }
        }

        /// <summary>
        /// constuct clas for UTS rest API
        /// </summary>
        /// <param name="apiKey"></param>
        internal UTSRestClass(string apiKey)
        {
            ApiKey = apiKey;
        }
        /// <summary>
        /// retrieve ticket granting ticket or null if fails
        /// (in the form of a uri)
        /// </summary>
        /// <returns></returns>
        public string ObtainTicketGrantingTicketUri()
        {
            string url = _baseURILogin + "/cas/v1/api-key";
            StringBuilder sbData = new StringBuilder();
            sbData.Append("apikey=");
            sbData.Append(ApiKey);
            string response = SendRequest("POST",
                sbData.ToString(),
                null,
                url);
            int start = response.IndexOf("https://utslogin.nlm.nih.gov/cas/v1/api-key/TGT");
            int stop = response.IndexOf("-cas", start);
            if ((start > -1) && (stop > -1))
            {
                _tgtUri = response.Substring(start, (stop + 4 - start));
                _whenLastTGTObtained = DateTime.Now;
                //debug
                //Console.WriteLine(_tgtUri + "  " + DateTime.Now.ToShortTimeString());
            }
            else
            {
                throw new Exception("Failed to obtain TicketGrantingTicket..." +
                    response);
            }
            return _tgtUri;
        }

        /// <summary>
        /// uri like https://utslogin.nlm.nih.gov/cas/v1/api-key/TGT-25469-aI4dlqaFWqA0lcT6ltUcJid11OPPiACcxCDzPTDqNPu9MIDoIu-cas
        /// </summary>
        /// <param name="tgtUri"></param>
        /// <returns></returns>
        public string ObtainSingleUseTicket(string tgtUri)
        {
            string body = "service=http://umlsks.nlm.nih.gov";
            string response = SendRequest("POST",
                body, null, tgtUri);
            //debug
            //Console.WriteLine(response + "  " + DateTime.Now.ToShortTimeString());
            return response;

        }
        /// <summary>
        /// obtain single use ticket, obtaining
        /// ticket granting ticket first if needed
        /// </summary>
        /// <returns></returns>
        public string ObtainSingleUseTicket()
        {
            if (!_tgtIsCurrent)
            {
                ObtainTicketGrantingTicketUri();
            }
            string ticket = ObtainSingleUseTicket(_tgtUri);
            if (ticket == null)
            {
                throw new Exception("Filed to obtain single use ticket.");
            }
            return ticket;
        }
        /// <summary>
        /// post request and return the text of the webresponse
        /// </summary>
        /// <param name="postOrGet">should be POST or GET</param>
        /// <param name="url"></param>
        /// <param name="bodyString">string of body of post if any</param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public string SendRequest(string postOrGet,
            string bodyString,
            NameValueCollection headers,
            string url)
        {
            if (url.StartsWith("http://") ||
                url.StartsWith("https://"))
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                if ((headers != null) &&
                    (headers.Count > 0))
                {
                    myHttpWebRequest.Headers.Add(headers);
                }


                //post data
                myHttpWebRequest.Method = postOrGet;
                ////// Set the ContentType property of the WebRequest.  This seems 
                //to be required; anything else says bad media type
                //Usually the content type is application/x-www-form-urlencoded, so the request body uses
                //the same format as the query string:
                //parameter = value & also = another
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                //media type
                //myHttpWebRequest.MediaType = "application/json";
                if (!string.IsNullOrEmpty(bodyString))
                {

                    // Create POST data and convert it to a byte array.
                    if (bodyString == null)
                        bodyString = string.Empty;
                    byte[] byteArray = Encoding.UTF8.GetBytes(bodyString);

                    // Set the ContentLength property of the WebRequest.
                    myHttpWebRequest.ContentLength = byteArray.Length;

                    // Get the request stream.
                    using (Stream postDataStream = myHttpWebRequest.GetRequestStream())
                    {
                        // Write the data to the request stream.
                        postDataStream.Write(byteArray, 0, byteArray.Length);
                        postDataStream.Flush();
                    }//postDataStream closes here
                }//from if not null post string


                string result = string.Empty;
                StringBuilder sb = new StringBuilder();
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    Console.WriteLine("\nThe HttpHeaders are \n\n\tName\t\tValue\n{0}", myHttpWebRequest.Headers);
                    // Print the HTML contents of the page to the string. 
                    using (Stream streamResponse = myHttpWebResponse.GetResponseStream())
                    {
                        using (StreamReader streamRead = new StreamReader(streamResponse))
                        {
                            Char[] readBuff = new Char[256];
                            int count = streamRead.Read(readBuff, 0, 256);
                            while (count > 0)
                            {
                                String outputData = new String(readBuff, 0, count);
                                sb.Append(outputData);
                                count = streamRead.Read(readBuff, 0, 256);
                            }
                        }//from using streamRead
                    }//from using streamResponse
                }//from using myHttpWebResponse
                result = sb.ToString();
                return result;
            }
            else
            {
                throw new Exception(" Error posting request:  " +
                    "address must start with http(s)");
            }
        }

        
        /// <summary>
        /// returns the raw text of the search and also
        /// assigns result to a list of SnomedConcepts
        /// </summary>
        /// <param name="maxPages">maximum number of pages to return e.g. 100</param>
        /// <param name="pageSize">maximum results per page e.g.100</param>
        /// <param name="result">a list of snomed concepts</param>
        /// <returns></returns>
        public string SearchSnomed(string searchString,
            int maxPages,
            int pageSize,
            out List<SnomedConcept> result)
        {
            List<SnomedConcept> snomedConcepts = new List<SnomedConcept>();
            SnomedConcept c = null;
            bool isAwaitingName = false;
            StringBuilder sbResult = new StringBuilder();
            string response; //for each page
            int pageCount = 0;
            for (int i = 0; i < maxPages; i++)
            {
                //debug:
                //Console.Write(i.ToString() + ";");

                string ticket = ObtainSingleUseTicket();
                StringBuilder sbUri = new StringBuilder(_baseUriRequests);
                sbUri.Append("/search/current?string=");
                sbUri.Append(System.Uri.EscapeDataString(searchString));

                sbUri.Append("&sabs=");
                sbUri.Append("SNOMEDCT_US");
                //sbUri.Append("&returnIdType=sourceDescriptor");// don't use - returns nothing
                //sbUri.Append("&returnIdType=sourceConcept"); //seems to give the same info
                sbUri.Append("&returnIdType=code"); //code returns UMLS CUI or source Ui depending on &sabs
                                                    //sbUri.Append("&returnIdType=sourceUi"); //sourceUi returns ui source uses

                sbUri.Append("&pageSize=");
                sbUri.Append(pageSize);
                sbUri.Append("&pageNumber=");
                sbUri.Append((i + 1).ToString());

                sbUri.Append("&ticket=");
                sbUri.Append(ticket);
                response = SendRequest("GET",
                    null, null, sbUri.ToString());
                if (JsonPageIsNORESULTS(response))
                {
                    pageCount = i;
                    break;
                }

                else
                {
                    sbResult.Append(response);
                    sbResult.Append(Environment.NewLine);
                    if (i == maxPages - 1)
                    {
                        throw new Exception("Truncated at " +
                            (i + 1).ToString() +
                            " pages maximum.");
                    }
                    //add to list
                    using (Newtonsoft.Json.JsonTextReader reader =
                        new JsonTextReader(new StringReader(response)))
                    {
                        while (reader.Read())
                        {
                            if (reader.Depth == 4)
                            {
                                if (isAwaitingName)
                                {
                                    //then if we get the name before we get end of object
                                    // we'll add it to the list
                                    //if end of object..
                                    if (reader.TokenType == JsonToken.EndObject)
                                    {
                                        //oops got to end before found name
                                        //reset
                                        isAwaitingName = false;
                                        c = null;
                                    }
                                    else if ((reader.TokenType == JsonToken.PropertyName) &&
                                        ((string)reader.Value == "name"))
                                    {
                                        //read value of name and save to list
                                        if (reader.Read())
                                        {
                                            if (reader.TokenType == JsonToken.String)
                                            {
                                                c.Name = (string)reader.Value;
                                                snomedConcepts.Add(c);
                                                isAwaitingName = false;
                                                c = null;
                                            }
                                        }
                                    }
                                }//from if awaing name
                                else if ((reader.TokenType == JsonToken.PropertyName) &&
                                 ((string)reader.Value == "ui"))
                                {
                                    //read value of ui
                                    if (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.String)
                                        {
                                            c = new SnomedConcept();
                                            // the ui value can be umls Concept Unique Id, or the snomed CID depending on query returnIdType
                                            c.ConceptID = (string)reader.Value;
                                            //c.CUI = (string)reader.Value;
                                            isAwaitingName = true;
                                        }
                                    }
                                }
                            }//from if depth 4
                        }//from while reader read
                    }//from using json reader
                }//from if not NORESULTS
            }//from for each page
            result = snomedConcepts;
            return sbResult.ToString();
        }


        /// <summary>
        /// returns the raw text of the search for only one page
        /// </summary>
        /// <param name="pageSize">maximum results per page e.g.100</param>
        /// <returns></returns>
        public string SearchUMLS(string searchString,
            int pageSize)
        {
            StringBuilder sbResult = new StringBuilder();
            string response; //for one page
            string ticket = ObtainSingleUseTicket();
            StringBuilder sbUri = new StringBuilder(_baseUriRequests);
            sbUri.Append("/search/current?string=");
            sbUri.Append(System.Uri.EscapeDataString(searchString));                             
            sbUri.Append("&pageSize=");
            sbUri.Append(pageSize);
            sbUri.Append("&ticket=");
            sbUri.Append(ticket);
            response = SendRequest("GET",
                null, null, sbUri.ToString());
            sbResult.Append(response);
            sbResult.Append(Environment.NewLine);
            return sbResult.ToString();
        }


        
        /// <summary>
        /// returns response string and assigns SnomedConcept to req.Result
        /// </summary>
        /// <param name="req">SearchString should be ID per Source
        /// and SourceAbbreviations should be the source Abbreviation</param>
        public string RetrieveSNOMEDContent(string code)
        {
            //StringBuilder sbResult = new StringBuilder();
            string response; //for each page
            string ticket = ObtainSingleUseTicket();
            StringBuilder sbUri = new StringBuilder(_baseUriRequests);
            sbUri.Append("/content/current/source/");// SNOMEDCT_US/");// 9468002/parents 
                                                     // / search/current?string=");
            sbUri.Append("SNOMEDCT_US"); //needs to be only one source
            sbUri.Append("/");
            sbUri.Append(code);
            // or sbUri.Append(System.Uri.EscapeDataString(code));
            //sbUri.Append("/atoms");
            sbUri.Append("?ticket=");
            sbUri.Append(ticket);
            response = SendRequest("GET",
                null, null, sbUri.ToString());
            return response;
        }

        
        /// <summary>
        /// retrieve SNOMED children or parents
        /// </summary>
        /// <param name="isForChildren">otherwise for parents</param>
        /// <param name="searchID">SNOMED ID</param>
        /// <param name="pageSize">e.g. 100</param>
        /// <param name="maxPages">e.g. 100</param>
        /// <param name="snomedConcepts">results</param>
        /// <returns></returns>
        public string RetrieveChildrenOrParents(string searchID, 
            bool isForChildren,
            int pageSize,
            int maxPages,
            out List<SnomedConcept> snomedConcepts)
        {
            ///content/{version}/source/{source}/{id}/children 
            snomedConcepts = new List<SnomedConcept>();
            SnomedConcept c = null;
            bool isAwaitingName = false;
            bool expectMoreOnNextPage = true;
            StringBuilder sbResult = new StringBuilder();
            string response = null; //for each page
            for (int i = 0; i < maxPages; i++)
            {
                //debug:
                //Console.Write(i.ToString() + ";");
                expectMoreOnNextPage = false; //unless we reach max for this page below...
                string ticket = ObtainSingleUseTicket();
                StringBuilder sbUri = new StringBuilder(_baseUriRequests);
                sbUri.Append("/content/");
                sbUri.Append("current");//unless you want to specify one
                sbUri.Append("/source/");
                sbUri.Append("SNOMEDCT_US");
                sbUri.Append("/");
                sbUri.Append(System.Uri.EscapeDataString(searchID));
                sbUri.Append(isForChildren ? "/children" : "/parents");
                sbUri.Append("?pageSize=");
                sbUri.Append(pageSize);
                sbUri.Append("&pageNumber=");
                sbUri.Append((i + 1).ToString());
                //if (!string.IsNullOrEmpty(req.SearchType))
                //{
                //    sbUri.Append("&searchType=");
                //    sbUri.Append(req.SearchType);
                //}
                sbUri.Append("&ticket=");
                sbUri.Append(ticket);
                //trap not found error which happens when results
                // count is multiple of pageSize
                try
                {
                    response = SendRequest("GET",
                        null, null, sbUri.ToString());
                }
                catch (Exception er)
                {
                    if ((i > 0) &&
                        (er is System.Net.WebException) &&
                    (er.ToString().Contains("(404)")))
                    {
                        //ignore the error, just quit
                        break;
                    }
                    else

                    {
                        //rethrow
                        throw er;
                    }
                }

                if (true)
                {
                    sbResult.Append(response);
                    sbResult.Append(Environment.NewLine);
                    if (i == maxPages - 1)
                    {
                        throw new Exception("Truncated at " +
                            (i + 1).ToString() +
                            " pages maximum.");
                    }
                    //add to list
                    using (Newtonsoft.Json.JsonTextReader reader =
                        new JsonTextReader(new StringReader(response)))
                    {
                        StringBuilder sb = new StringBuilder();
                        int numConceptsFound = 0;
                        while (reader.Read())
                        {
                            sb.Append("depth=");
                            sb.Append(reader.Depth);
                            sb.Append("; tokenType=");
                            sb.Append(reader.TokenType);
                            sb.Append("; value=");
                            sb.Append(reader.Value);

                            sb.Append("\r\n");
                            if (reader.Depth == 3)
                            {
                                if (isAwaitingName)
                                {
                                    //then if we get the name before we get end of object
                                    // we'll add it to the list
                                    //if end of object..
                                    if (reader.TokenType == JsonToken.EndObject)
                                    {
                                        //oops got to end before found name
                                        //reset
                                        isAwaitingName = false;
                                        c = null;
                                    }
                                    else if ((reader.TokenType == JsonToken.PropertyName) &&
                                        ((string)reader.Value == "name"))
                                    {
                                        //read value of name and save to list
                                        if (reader.Read())
                                        {
                                            if (reader.TokenType == JsonToken.String)
                                            {
                                                c.Name = (string)reader.Value;
                                                numConceptsFound++;
                                                if (numConceptsFound >= pageSize)
                                                {
                                                    expectMoreOnNextPage = true;
                                                }
                                                snomedConcepts.Add(c);
                                                isAwaitingName = false;
                                                c = null;
                                            }
                                        }
                                    }
                                }//from if awaing name
                                else if ((reader.TokenType == JsonToken.PropertyName) &&
                                 ((string)reader.Value == "ui"))
                                {
                                    //read value of ui
                                    if (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.String)
                                        {
                                            c = new SnomedConcept();
                                            //hopefully ui is the Snomed Concept ID
                                            c.ConceptID = (string)reader.Value;
                                            isAwaitingName = true;
                                        }
                                    }
                                }
                            }//from if depth 3
                        }//from while reader read
                    }//from using json reader
                }//from if not NORESULTS -> from if true
                 //notice they don't tell us when end of list, so the
                 // only way we know is if list is smaller than pageSize
                if (!expectMoreOnNextPage)
                {
                    break;
                }
            }//from for each page
            return sbResult.ToString();
        }

        /// <summary>
        /// sends a GET request to the given uri and 
        /// returns the response, so user can edit and tweak examples
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="makeItAPost">if true send POST ; otherwise GET</param>
        /// <returns></returns>
        public string GetCustomRequest (string uri,
            bool makeItAPost)
        {
            string response = SendRequest(makeItAPost?"POST":"GET",
                        null, null, uri);
            return response;
        }

        public void UseJson()
        {
            Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(new StringReader("s"));
        }
        /// <summary>
        /// returns true only if find
        /// result.results[0].name with value "NO RESULTS";
        /// otherwise defaults to false
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool JsonPageIsNORESULTS(string json)
        {
            bool result = false;
            using (Newtonsoft.Json.JsonTextReader reader =
                new JsonTextReader(new StringReader(json)))
            {
                while (reader.Read())
                {
                    if ((reader.Path == "result.results[0].name") &&
                        (reader.TokenType == JsonToken.String))
                    {
                        result = ((string)reader.Value == "NO RESULTS");
                        //string path = reader.Path;
                        //JsonToken tokenType = reader.TokenType;
                        //object value = reader.Value;
                        break;
                    }
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Snomed concept name and ui
    /// </summary>
    public class SnomedConcept
    {
        /// <summary>
        /// text name 
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Snomed Unique Identifier
        /// </summary>
        public string ConceptID = null;
        /// <summary>
        /// UMLS Concept Unique Identifier, if known
        /// </summary>
        public string CUI = null;
        /// <summary>
        /// creaete empty Snomed Concept - needs explicit 
        /// member definitions later
        /// </summary>
        public SnomedConcept()
        {

        }
        /// <summary>
        /// create Snomed Concept by name and Snomed UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cid">snomed concept id which umls returns as UI</param>
        public SnomedConcept(string name, string cid)
        {
            Name = name;
            ConceptID = cid;
        }
        /// <summary>
        /// returns name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Name);
            sb.Append("   ");
            sb.Append(ConceptID);
            return sb.ToString();
        }
    }

}
