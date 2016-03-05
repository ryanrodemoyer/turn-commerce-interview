using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TurnCommerce.BizLogic
{
    public class Sockets
    {
        private const int ATTEMPTS = 500;

        public async Task<string> RunAsync()
        {
            Encoding encoding = Encoding.GetEncoding("ISO-8859-1");

            var responses = new Dictionary<int, string>(ATTEMPTS);
            var tasks = new List<Task<Tuple<byte[], int>>>(ATTEMPTS);
            var requests = new List<byte[]>(ATTEMPTS);

            for (int i = 1; i <= ATTEMPTS; i++)
            {
                Request r = new Request { RequestId = i };
                requests.Add(SerializeEncodingBinary(r, encoding));
            }

            for (int i = 1; i <= requests.Count; i++)
            {
                tasks.Add(BuildTask(requests[i - 1]));
            }

            Tuple<byte[], int>[] results = await Task.WhenAll(tasks);

            foreach (var responseBuffer in results)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(responseBuffer.Item1, 0, responseBuffer.Item2);

                    Response response = DeserializeEncodingBinary<Response>(ms);

                    responses[response.RequestId] = response.Message;
                }
            }

            StringBuilder builder = new StringBuilder(ATTEMPTS);
            foreach (var kvp in responses.OrderByDescending(x => x.Key))
            {
                builder.Append(kvp.Value[2]);
            }

            return builder.ToString();
        }

        private static Task<Tuple<byte[], int>> BuildTask(byte[] requestBuffer)
        {
            return Task.Run(async () =>
            {
                Tuple<byte[], int> returnval = null;

                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync("216.38.192.141", 8765);

                    using (NetworkStream ns = client.GetStream())
                    {
                        await ns.WriteAsync(requestBuffer, 0, requestBuffer.Length);

                        byte[] responseBuffer = new byte[256];

                        int read = await ns.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                        if (read >= 0)
                        {
                            returnval = new Tuple<byte[], int>(responseBuffer, read);
                        }
                    }
                }

                return returnval;
            });
        }

        private static T DeserializeEncodingBinary<T>(MemoryStream ms)
        {
            using (MemoryStream ms2 = new MemoryStream(ms.ToArray()))
            {
                using (StreamReader sw = new StreamReader(ms2))
                {
                    var xml = new XmlSerializer(typeof(T));
                    return (T)xml.Deserialize(sw);
                }
            }
        }

        private static byte[] SerializeEncodingBinary<T>(T input, Encoding encoding)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, encoding))
                {
                    var xml = new XmlSerializer(typeof(T));
                    xml.Serialize(sw, input);
                }

                return ms.ToArray();
            }
        }
    }

    [Serializable, XmlRoot("request")]
    public class Request
    {
        [XmlElement(ElementName = "requestID")]
        public int RequestId { get; set; }
    }

    [Serializable, XmlRoot("response")]
    public class Response
    {
        [XmlElement(ElementName = "responseID")]
        public int ResponseId { get; set; }

        [XmlElement(ElementName = "requestID")]
        public int RequestId { get; set; }

        [XmlElement(ElementName = "message")]
        public string Message { get; set; }
    }
}
