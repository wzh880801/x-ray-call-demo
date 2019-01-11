using System;

namespace XRayDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            CallByFileBytes();

            CallByFile();

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void CallByFileBytes()
        {
            Console.WriteLine("\r\nCallByFileBytes:");

            byte[] fileBytes = System.IO.File.ReadAllBytes(@".\001.jpg");

            var api = new Malong.Common.Api.ApiHelper
            {
                ApiUrl = "http://192.168.5.101:8080/service/detect/xray",
                ContentType = Malong.Common.Api.HttpContentType.FILE,
                Method = Malong.Common.Api.HttpMethod.POST,
                RequestBody = new Malong.Common.Api.HttpRequestBody("search")
                {
                    RequestFile = new Malong.Common.Api.HttpRequestFile
                    {
                        Name = "001.jpg",
                        Bytes = fileBytes
                    }
                }
            };

            DetectResponse response = api.GetResponse<DetectResponse>();

            if (response != null && !string.IsNullOrEmpty(response.request_id))
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));

                Console.WriteLine("检测结果：");
                foreach (var b in response.boxes_detected)
                {
                    Console.WriteLine(b.type);
                }
            }
        }

        static void CallByFile()
        {
            Console.WriteLine("\r\nCallByFile:");

            var file = new System.IO.FileInfo(@".\001.jpg");

            var api = new Malong.Common.Api.ApiHelper
            {
                ApiUrl = "http://192.168.5.101:8080/service/detect/xray",
                ContentType = Malong.Common.Api.HttpContentType.FILE,
                Method = Malong.Common.Api.HttpMethod.POST,
                RequestBody = new Malong.Common.Api.HttpRequestBody("search")
                {
                    File = file
                }
            };

            DetectResponse response = api.GetResponse<DetectResponse>();

            if (response != null && !string.IsNullOrEmpty(response.request_id))
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));

                Console.WriteLine("检测结果：");
                foreach (var b in response.boxes_detected)
                {
                    Console.WriteLine(b.type);
                }
            }
        }
    }
}
