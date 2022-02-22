using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace ConnectorConcrete
{
    public class SimpleResponse
    {
        public readonly int status_code;
        public readonly object content;
        public SimpleResponse(HttpWebResponse response)
        {
            this.status_code = response.StatusCode.GetHashCode();
            using (Stream dataStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    try
                    {
                        this.content = JsonSerializer.Deserialize<Data>(reader.ReadToEnd());
                    }
                    catch (Exception)
                    {
                        this.content = reader.ReadToEnd();
                    }
                }
            }
        }
    }

    public class Data: Dictionary<string, object>
    {
        public Data()
        {

        }

        public Data Serialize()
        {
            var entries = this.Select(d =>
                string.Format("\"{0}\": {1}", d.Key, JsonSerializer.Serialize(d.Value))
            );
            return JsonSerializer.Deserialize<Data>("{" + string.Join(",", entries) + "}");
        }
    }

    public class ConcreteDatastoreClient
    {
        public string url;
        public string token;
        public Dictionary<string, string> headers;
        public Dictionary<string, string> default_headers = new Dictionary<string, string>();
        public Dictionary<string, string> scopes;
        public WebRequest request;
        bool retry_indefinitely;
        int retry_timeout;
        int delay_between_retries;
        Dictionary<object, object> external_cache;

        public ConcreteDatastoreClient(
            string url,
            string token = null,
            Dictionary<string, string> headers = null,
            Dictionary<string, string> scopes = null,
            bool retry_indefinitely = false,
            int retry_timeout = 100000,
            int delay_between_retries = 1000,
            Dictionary<object, object> external_cache = null
        )
        {
            this.url = url;
            this.token = token;
            this.headers = headers;
            this.scopes = scopes;
            this.retry_indefinitely = retry_indefinitely;
            this.retry_timeout = retry_timeout;
            this.delay_between_retries = delay_between_retries;
            this.external_cache = external_cache;
        }

        private WebRequest BuildRequest(String objectName, String method = "GET", string _url = null)
        {
            if (_url != null)
            {
                _url += "/";
            }
            request = WebRequest.Create(this.url + objectName + "/" + _url);
            request.ContentType = "application/json";
            String authorization = String.Format("Token {0}", this.token);
            request.Headers["Authorization"] = authorization;
            foreach (KeyValuePair<string, string> pair in this.default_headers)
            {
                this.request.Headers[pair.Key] = pair.Value;
            }
            request.Method = method;
            return request;
        }

        public static string GetResponseMessage(HttpWebResponse resp)
        {
            using (Stream dataStream = resp.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public Data Get(String objectName)
        {
            try
            {
                request = BuildRequest(objectName: objectName);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                SimpleResponse simpleResp = new SimpleResponse(response);
                return (Data)simpleResp.content;
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                SimpleResponse simpleResp = new SimpleResponse(response);
                return (Data)simpleResp.content;
            }
        }

        public Data Retrieve(string objectName, string uid)
        {
            HttpWebResponse response;
            try
            {
                request = BuildRequest(objectName: objectName, _url: uid);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            SimpleResponse simpleResp = new SimpleResponse(response);
            return (Data)simpleResp.content;
        }

        public Data Post(string objectName, Data requestBody)
        {
            Data postData = requestBody.Serialize();
            HttpWebResponse response;
            try
            {
                request = BuildRequest(objectName: objectName, method: "POST");
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postData);
                    streamWriter.Flush();
                    streamWriter.Close();
                    response = (HttpWebResponse)request.GetResponse();
                }
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            SimpleResponse simpleResp = new SimpleResponse(response);
            return (Data)simpleResp.content;
        }
    }
}
