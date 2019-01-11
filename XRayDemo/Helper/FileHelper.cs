using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Malong.Common.Helper
{
    public class FileHelper
    {
        public static string GetBoundary()
        {
            return "---------------------------" + DateTime.Now.Ticks.ToString("x");
        }

        public static string GetContentType(string boundary)
        {
            return "multipart/form-data; boundary=" + boundary;
        }

        public static byte[] GetMultipartBytes(FileInfo file, string boundary, Dictionary<string, string> options, string paraName = "search")
        {
            var bytes = new List<byte>();
            bytes.AddRange(BoundaryBytes(boundary));
            if (options != null && options.Count > 0)
            {
                foreach (var opt in options)
                    bytes.AddRange(FieldBytes(opt.Key, opt.Value, boundary));
            }
            bytes.AddRange(FileHeaders(file, paraName));
            bytes.AddRange(File.ReadAllBytes(file.FullName));
            bytes.AddRange(TailBytes(boundary));
            return bytes.ToArray();
        }

        /// <summary>
        /// 获取HTTP Request POST Body
        /// </summary>
        /// <param name="filename">文件名，随便起，比如001.jpg</param>
        /// <param name="fileBytes">图片的二进制</param>
        /// <param name="boundary"></param>
        /// <param name="options"></param>
        /// <param name="paraName"></param>
        /// <returns></returns>
        public static byte[] GetMultipartBytes(string filename, byte[] fileBytes, string boundary, Dictionary<string, string> options, string paraName = "search")
        {
            var bytes = new List<byte>();
            bytes.AddRange(BoundaryBytes(boundary));
            if (options != null && options.Count > 0)
            {
                foreach (var opt in options)
                    bytes.AddRange(FieldBytes(opt.Key, opt.Value, boundary));
            }
            bytes.AddRange(FileHeaders(filename, fileBytes, paraName));
            bytes.AddRange(fileBytes);
            bytes.AddRange(TailBytes(boundary));
            return bytes.ToArray();
        }

        private static byte[] BoundaryBytes(string boundary)
        {
            return Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
        }

        private static byte[] FileHeaders(FileInfo file, string paraName = "search")
        {
            string header = "Content-Disposition: form-data;";
            header += string.Format(" name=\"{0}\";", paraName);
            header += string.Format(" filename=\"{0}\"\r\n", file.Name);
            header += string.Format("Content-Type: {0}\r\n\r\n", GetFileType(file.Extension));
            return Encoding.UTF8.GetBytes(header);
        }

        private static byte[] FileHeaders(string filename, byte[] bytes, string paraName = "search")
        {
            string header = "Content-Disposition: form-data;";
            header += string.Format(" name=\"{0}\";", paraName);
            header += string.Format(" filename=\"{0}\"\r\n", filename);
            header += string.Format("Content-Type: {0}\r\n\r\n", GetFileType(".jpg"));
            return Encoding.UTF8.GetBytes(header);
        }

        private static byte[] FieldBytes(string key, string value, string boundary)
        {
            string field = "Content-Disposition: form-data;";
            field += string.Format(" name=\"{0}\"\r\n\r\n{1}", key, value);
            byte[] fdBytes = Encoding.UTF8.GetBytes(field);
            byte[] bdBytes = BoundaryBytes(boundary);
            var bytes = new List<byte>();
            bytes.AddRange(fdBytes);
            bytes.AddRange(bdBytes);
            return bytes.ToArray();
        }

        private static byte[] TailBytes(string boundary)
        {
            string tail = string.Format("\r\n--{0}--\r\n", boundary);
            return Encoding.UTF8.GetBytes(tail);
        }

        private static string GetFileType(string ext)
        {
            switch (ext.ToLower())
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".csv":
                    return "application/vnd.ms-excel";
                case ".txt":
                    return "text/plain";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return "text/plain";
        }
    }
}