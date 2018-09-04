using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;

namespace payssion
{
    public class PayssionClient
    {
        const string VERSION = "1.3.0.160612";

        private static readonly string DefaultUserAgent = "Payssion/php/$langVersion/" + VERSION;

        private string api_url;

        private bool ssl_verify = true;

        private string api_key;

        protected string secret_key;

        /// <summary>
        /// set api_url
        /// </summary>
        /// <param name="_apiurl"></param>
        public void setApiUrl(string _apiurl)
        {
            this.api_url = _apiurl;
        }
        /// <summary>
        /// get api_url
        /// </summary>
        /// <returns></returns>
        public string getApiurl()
        {
            return this.api_url;
        }

        public void setSSLVerify(bool _sslVerify)
        {
            this.ssl_verify = _sslVerify;
        }
        public bool getSSLVerify()
        {
            return this.ssl_verify;
        }

        /// <summary>
        /// http request error
        /// </summary>
        Dictionary<int, string> http_errors = new Dictionary<int, string>()
        { 
            { 400,"400 Bad Request"},
            { 401,"401 Unauthorized" },
            { 500, "500 Internal Server Error" },
            { 501, "501 Not Implemented" },
            { 502, "502 Bad Gateway" },
            { 503, "503 Service Unavailable" },
            { 504,"504 Gateway Timeout"}
        };

        /// <summary>
        /// request status
        /// </summary>
        public bool is_success = false;

        protected string[] allowed_request_methods = new string[4] { "get", "put", "post", "delete" };

        public enum sig_keys
        { 
            create = 0,
            detail = 1
        }

        public string create(Dictionary<string, string> paras)
        {
            return this.call("create", "post", paras);
        }

        public string getDetails(Dictionary<string, string> paras)
        {
           return this.call("details", "get", paras);
        }

        protected Dictionary<string, string[]> sig_keys_arr = new Dictionary<string, string[]>()
        { 
            {"create",new string[]{"api_key","pm_id","amount","currency","order_id"}}, 
            {"details",new string[] { "api_key","transaction_id","order_id"} } 
        };
        
       
        private string getSig(Dictionary<string, string> parameters,string sig_key)
        {
            int length = sig_keys_arr[sig_key].Length;
            string [] msg_arr =new string[length];
            int i=0;
            foreach (string key in sig_keys_arr[sig_key])
            {
                msg_arr[i] = parameters[key];
                i++;
            }
            string msg = string.Join("|", msg_arr);
            msg += "|" + this.secret_key;
            string md5 =System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(msg, "MD5").ToString();
            //byte[]   result   =   md5.ComputeHash(System.Text.Encoding.Default.GetBytes(msg));
           // string sig =System.Text.Encoding.Default.GetString(result);
            return md5.ToLower();
        }

        public string call(string method,string request,Dictionary<string,string> paras)
        {
            this.is_success = false;
            paras.Add("api_key", this.api_key);
            paras.Add("api_sig", getSig(paras, method));
            Encoding encoding = Encoding.GetEncoding("utf-8");
            string response = this.pushData(method, request, paras, encoding);

            if (response != "")
            {

                JavaScriptSerializer json = new JavaScriptSerializer();
                ResponseStatus resp_status = new ResponseStatus();
                ResponseStatus response_status = (ResponseStatus)json.Deserialize<ResponseStatus>(response);
                if (response_status.result_code == "200")
                {
                    this.is_success = true;
                }
            }

            return response;
           
        }


        public PayssionClient(string api_key, string secret_key, bool is_livemode = true)
        {

            this.api_key = api_key;
            this.secret_key = secret_key;
            if (this.api_key == "")
            {
                throw new MyException("api_key is not set!");
            }
            if (this.secret_key == "")
            {
                throw new MyException("secret_key is not set!");
            }
            setLiveMode(is_livemode);
        }

        public void setLiveMode(bool is_livemode)
        {
    	    if (is_livemode) {
    		    this.api_url = "https://www.payssion.com/api/v1/payment/";
    	    } else {
    		    this.api_url = "http://sandbox.payssion.com/api/v1/payment/";
    	    }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }
       

        public string UserAgent()
        {
            string donetVersion = System.Environment.Version.ToString();
            string uname = System.Environment.OSVersion.ToString();
            return "verson:" + VERSION + ",lang:.NET,lang_verson:" + donetVersion + ",publisher:payssion,uname:" + uname;
        }

        private string pushData(string method, string method_type, Dictionary<string, string> parameters, Encoding charset)
        {
           
            try
            {
                HttpWebRequest request = null;
                string dirPath = HttpContext.Current.Server.MapPath("lib");
                X509Certificate cer = X509Certificate.CreateFromCertFile(dirPath + "\\cacert.pem");

                //HTTPSQ请求  
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                StringBuilder buffer = new StringBuilder();
                if (!(parameters == null || parameters.Count == 0))
                {
                  
                    int i = 0;
                    foreach (string key in parameters.Keys)
                    {
                        if (i > 0)
                        {
                            buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        }
                        i++;
                    }

                }

                request = WebRequest.Create(this.api_url+method+"?"+buffer.ToString()) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = method_type;
  
                request.Headers.Add("X-Payssion-Client-User-Agent", UserAgent());
                request.ContentType = "Content-Type: application/x-www-form-urlencoded";
                request.UserAgent = DefaultUserAgent;
                if (this.ssl_verify)
                {
                    request.ClientCertificates.Add(cer);
                }
                //如果需要POST数据     
               
                byte[] data = charset.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream_t = response.GetResponseStream();   
                StreamReader sr = new StreamReader(stream_t); 
                string html = sr.ReadToEnd();

                return html;
               
            }
            catch (WebException ex)
            {
                ResponseStatus status = new ResponseStatus();
                status.result_code = "500";
                status.description = ex.Message;
                 JavaScriptSerializer json = new JavaScriptSerializer();
                 return json.Serialize(status);
                
            }
            return " ";
        }

       
       
    }

    public class ResponseStatus
    {
        public string result_code { get; set; }
        public string description { get; set; }
    }


    public class MyException : Exception
    {

        public MyException(string message) : base(message) { }
        public override string Message
        {
            get
            {
                return base.Message;
            }
        }
    }
}


