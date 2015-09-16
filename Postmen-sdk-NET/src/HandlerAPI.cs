using System;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Postmen_sdk_NET
{
    public class HandlerAPI
    {

        private const string version = "v3";
        private string api_key;
        private string endpoint;
        private string proxy;
        private bool retry;
        private bool rate;

        public string Version
        {
            get { return version; }
        }

        public string Api_key
        {
            get { return api_key; }
            set { api_key = value; }
        }

        public string Endpoint
        {
            get { return endpoint; }
            set { endpoint = value; }
        }

        public string Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }

        public bool Retry
        {
            get { return retry; }
            set { retry = value; }
        }

        public bool Rate
        {
            get { return rate; }
            set { rate = value; }
        }

        public HandlerAPI(string api_key, string region = "", string endpoint = "", string proxy = "", bool retry = true, bool rate = true)
        {

            this.api_key = api_key;

            if (String.IsNullOrEmpty(api_key))
            {
                throw new Exception("missed API key");
            }
              

            if (String.IsNullOrEmpty(region) && String.IsNullOrEmpty(endpoint))
            {
                throw new Exception("missed region");
            }

            if (String.IsNullOrEmpty(endpoint))
            {
                endpoint = "https://"+ region+"-api.postmen.com";
            }

            this.endpoint = endpoint;
            this.proxy = proxy;
            this.retry = retry;
            this.rate = rate;

        }

        public HandlerAPI(string api_key, string endpoint)
        {
            // TODO: Complete member initialization
            this.api_key = api_key;
            this.endpoint = endpoint;
        }

        public JObject call(string method, string path, JObject body = null, string query = "", bool retry = true) {
            string endpoint = this.endpoint + path;
            int tries = 0;
            int maxtries = 4;
            int aux_error;
            JObject result = null;
            Exception exception = null;
            
            if (!String.IsNullOrEmpty(query))
            {
                endpoint += "?" + query;
            }

            do
            {

                try
                {
                    string result_json = this.request(endpoint, method, body);
                    result = JObject.Parse(result_json);
                    aux_error = (int)result["meta"]["code"];

                    while (aux_error > 10) 
                        aux_error /= 10;
                    
                    if (aux_error >= 2 && aux_error < 4)
                    {
                        return (JObject) result;
                    }
                    else if((bool)result["meta"]["retryable"] == false){
                        retry = false;
                        throw PostmenException.FactoryMethod(result);
                    }
                    else 
                    {
                        ++tries;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    ++tries;

                }
            } while (retry && tries < maxtries);

            if (exception != null)
                throw exception;

            throw PostmenException.FactoryMethod(result);
        }

        public string request(string endpoint, string method, JObject body)
        {
            
            string result_json = "";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = method;
            httpWebRequest.Headers["postmen-api-key"] = this.api_key;
            httpWebRequest.Headers["x-postmen-agent"] = "NET-sdk-" + HandlerAPI.version;

            if (!(body == null))
            {
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(body.ToString());
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result_json = streamReader.ReadToEnd();
            }

            return result_json;
            
        }

        public JObject GET(string path, string query = "", bool retry = true)
        {
           return this.call(method: "GET", path: path, query: query, retry: retry);
        }

        public JObject POST(string path, JObject body = null, string query = "", bool retry = true)
        {
            return this.call(method: "POST", path: path, body: body, query: query, retry: retry);
        }

        public JObject PUT(string path, JObject body = null, string query = "", bool retry = true)
        {
            return this.call(method: "PUT", path: path, body: body, query: query, retry: retry);
        }

        public JObject DELETE(string path, string query = "", bool retry = true)
        {
            return this.call(method: "DELETE", path: path, query: query, retry: retry);
        }

        public JObject get(string resource, string id = "", string query ="", bool retry = true)
        {
            return this.GET(path: "/" + HandlerAPI.version + "/" + resource + "/" + id, query: query, retry: retry);
        }

        public JObject create(string resource, JObject payload, string query = "", bool retry = true)
        {
            return this.POST(path: "/" + HandlerAPI.version + "/" +  resource, body: payload, query: query, retry: retry);
        }

        public JObject cancel(string resource, string id = "", string query ="", bool retry = true)
        {
            return this.DELETE(path: "/" + HandlerAPI.version + "/" + resource+ "/" + id, query: query, retry: retry);
        }


    }

    public class PostmenException: Exception
    {
        private int code;
        private Dictionary<string,string> details;
        private bool retryable;
        private JObject data;

        public int Code
        {
            get { return code; }
        }

        public Dictionary<string,string> Details
        {
            get { return details; }
        }

        public bool Retryable
        {
            get { return retryable; }
        }

        public JObject Raw
        {
            get { return data; }
        }

        public PostmenException(int code, Dictionary<string, string> details, bool retryable, string message, JObject data) : base(message)
        {
            this.code = code;
            this.details = details;
            this.retryable = retryable;
            this.data = data;

        }

        public static PostmenException FactoryMethod(JObject data)
        {
            int code_i;
            string message_i;
            Dictionary <string, string> details_i;
            bool retryable_i;
            JObject meta = (JObject)data["meta"];

            if (meta["code"] != null)
            {
                code_i = (int)meta["code"];
            }
            else
            {
                code_i = 0;
            }

            if (meta["message"] != null)
            {
                message_i = (string)meta["message"];
                if (code_i > 0)
                    message_i += " (" + code_i + ")";
            }
            else
            {
                message_i = null;
            }

            if (meta["details"] != null)
            {
                JArray details_ja = (JArray)meta["details"];
                JObject aux;
                int i;
                details_i = new Dictionary<String,String>();
                for (i = 0; i < details_ja.Count; i++)
                {
                    aux = (JObject) details_ja[i];
                    details_i.Add((string)aux["path"],(string) aux["info"]);
                    //details_i.Add( (string)details_ja[i].Property<string(), (string) details_ja[i].Value<string>());
                    //details_i[i] = (string)details_ja[i];
                }

            }
            else
            {
                details_i = null;
            }

            if (meta["retryable"] != null)
            {
                retryable_i = (bool)meta["retryable"];
            }
            else
            {
                retryable_i = false;
            }

            return new PostmenException(code_i, details_i, retryable_i, message_i, data); 

        }

    }
}
