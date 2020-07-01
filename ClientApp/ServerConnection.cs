using Encryption;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ClientApp
{
    class ServerConnection : IDisposable
    {
        private readonly TcpClient _client;

        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;
        public ServerConnection()
        {
            _client = new TcpClient("localhost", 8181);
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());

        }

        public void SecureConnect(AESEncryption aes)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            var serverModulus = _reader.ReadLine();

            if(string.IsNullOrEmpty(serverModulus)) throw new NullReferenceException("serverModulus is empty");

            var encModulus = Convert.FromBase64String(serverModulus);

            var decModulus = aes.Decrypt(encModulus);

            rsa.ImportParameters(new RSAParameters
            {
                Modulus = decModulus,
                Exponent = new byte[] { 1, 0, 1 }
            });

            aes.GenerateKey();

            var newAesKey = rsa.Encrypt(aes.Key, false);
            _writer.WriteLine(Convert.ToBase64String(newAesKey));

            _writer.Flush();
        }
        public void Dispose()
        {
            _client.Close();
        }

        public string RequestData(string data)
        {
            _writer.WriteLine(data);
            _writer.Flush();

            return _reader.ReadLine();
        }


    }
}
