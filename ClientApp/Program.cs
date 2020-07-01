using Encryption;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var rq = new ServerConnection())
            {
                var aes = new AESEncryption();

                rq.SecureConnect(aes);

                dynamic requestData = new ExpandoObject();

                requestData.hwid = "12340";

                dynamic privateData = new ExpandoObject();
                privateData.hwid = "123";

                requestData.data = aes.Encrypt(JsonConvert.SerializeObject(requestData));

                var requestDataSerialized = JsonConvert.SerializeObject(requestData);

                var tt = rq.RequestData(requestDataSerialized);

                var responseDataDeserialized = JsonConvert.DeserializeObject<dynamic>(tt);
                var privateDataDecrypted = aes.Decrypt(responseDataDeserialized.data.ToString());
            }
        }
    }
}
