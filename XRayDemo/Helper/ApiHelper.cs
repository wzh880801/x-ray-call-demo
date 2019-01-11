using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Malong.Common.Helper;

namespace Malong.Common.Api
{
    public class ApiHelper
    {
        public string ApiUrl { get; set; }

        public ApiHelper(string apiUrl)
        {
            this.ApiUrl = apiUrl;
        }

        public ApiHelper()
        {

        }

        public Dictionary<string, string> RequestHeaders { get; set; }

        public HttpRequestBody RequestBody { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.GET;

        public HttpContentType ContentType { get; set; } = HttpContentType.X_WWW_FORM_URLENCODED;

        private WebHeaderCollection _headers;
        public WebHeaderCollection Header { get { return _headers; } }

        private static Dictionary<int, string> _methodsDic = typeof(HttpMethod).ToDictionary();
        private static Dictionary<int, string> _contentTypeDic = typeof(HttpContentType).ToDictionary();

        private HttpWebRequest CreateRequest()
        {
            var url = this.ApiUrl;

            if (this.Method == HttpMethod.GET)
            {
                var paras = this.RequestBody != null ? this.RequestBody.GetQueryFormString() : "";
                if (!string.IsNullOrEmpty(paras))
                {
                    if (url.Contains("?"))
                        url = string.Format("{0}&{1}", this.ApiUrl, paras);
                    else
                        url = string.Format("{0}?{1}", this.ApiUrl, paras);
                }
            }

            var request = WebRequest.Create(url) as HttpWebRequest;

            if (this.Method == HttpMethod.GET)
                request.ContentType = _contentTypeDic[(int)HttpContentType.X_WWW_FORM_URLENCODED];

            // SET Header
            if (this.RequestHeaders != null && this.RequestHeaders.Count > 0)
            {
                foreach (var key in this.RequestHeaders.Keys)
                {
                    try
                    {
                        request.Headers.Add(key, this.RequestHeaders[key]);
                    }
                    catch
                    {
                        request.Headers[key] = this.RequestHeaders[key];
                    }
                }
            }

            request.UserAgent = "Malong.Common V1.0.0";

            request.Method = _methodsDic[(int)this.Method];

            if (this.Method != HttpMethod.GET && this.RequestBody != null)
            {
                var boundary = FileHelper.GetBoundary();
                var bytes = this.RequestBody.GetBytes(boundary);
                request.ContentType = _contentTypeDic[(int)this.ContentType];
                if (this.ContentType == HttpContentType.FILE)
                    request.ContentType += boundary;

                request.ContentLength = bytes.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
            }

            return request;
        }

        public string GetResponse()
        {
            var request = CreateRequest();
            try
            {
                using (var response = request.GetResponse())
                {
                    _headers = response.Headers;
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                var _response = ex.Response as HttpWebResponse;
                if (_response == null)
                    return null;

                return null;
            }
        }

        public byte[] GetResponseBytes()
        {
            var request = CreateRequest();
            try
            {
                using (var response = request.GetResponse())
                {
                    _headers = response.Headers;
                    using (var stream = response.GetResponseStream())
                    {
                        if (response.ContentLength == 0)
                            return null;

                        byte[] bytes = new byte[response.ContentLength];
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            var b = stream.ReadByte();
                            if (b != -1)
                                bytes[i] = (byte)b;
                        }
                        return bytes;
                    }
                }
            }
            catch (WebException)
            {
#if DEBUG
                Console.WriteLine("WebException");
#endif
                return null;
            }
        }

        public T GetResponse<T>()
        {
            var json = GetResponse();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }

    public class HttpRequestBody
    {
        public Dictionary<string, string> QueryForms { get; set; }

        public FileInfo File { get; set; }

        public HttpRequestFile RequestFile { get; set; }

        /// <summary>
        /// 可以直接传入Object或者Json字符串（Object会被自动序列化为Json字符串）
        /// </summary>
        public object JsonBody { get; set; }

        public string GetQueryFormString()
        {
            if (this.QueryForms != null && this.QueryForms.Count > 0)
            {
                var list = from p in this.QueryForms select string.Format("{0}={1}", p.Key, p.Value);
                return string.Join("&", list.ToArray());
            }

            return "";
        }

        private string GetJsonBody()
        {
            if (this.JsonBody != null)
            {
                if (typeof(System.String) == this.JsonBody.GetType())
                    return this.JsonBody.ToString();
                return Newtonsoft.Json.JsonConvert.SerializeObject(this.JsonBody);
            }
            return null;
        }

        public byte[] GetBytes(string boundary)
        {
            if (this.File != null && this.File.Exists)
                return FileHelper.GetMultipartBytes(this.File, boundary, this.QueryForms, _paraName);

            if (this.RequestFile != null && this.RequestFile.Bytes != null && this.RequestFile.Bytes.Length > 0)
            {
                return FileHelper.GetMultipartBytes(this.RequestFile.Name, this.RequestFile.Bytes, boundary, this.QueryForms, _paraName);
            }

            var body = GetQueryFormString();
            if (!string.IsNullOrEmpty(body))
                return System.Text.Encoding.UTF8.GetBytes(body);

            var json = GetJsonBody();
            if (!string.IsNullOrEmpty(json))
                return System.Text.Encoding.UTF8.GetBytes(json);

            return null;
        }

        public HttpRequestBody()
        {

        }

        private string _paraName = "file";

        public HttpRequestBody(string paraName)
        {
            _paraName = paraName;
        }
    }

    public class HttpRequestFile
    {
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
    }

    public enum HttpMethod
    {
        [CustomizedAttribute.EnumDescription("GET")]
        GET,

        [CustomizedAttribute.EnumDescription("POST")]
        POST
    }

    public enum HttpContentType
    {
        [CustomizedAttribute.EnumDescription("application/x-www-form-urlencoded;charset=utf-8")]
        X_WWW_FORM_URLENCODED,

        [CustomizedAttribute.EnumDescription("application/json;charset=utf-8")]
        JSON,

        [CustomizedAttribute.EnumDescription("multipart/form-data; boundary=")]
        FILE
    }
}