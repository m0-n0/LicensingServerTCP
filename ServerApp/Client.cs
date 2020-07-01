using Encryption;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    class Client : IDisposable
    {
        private static readonly Database _database = new Database();

        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly NetworkStream _baseStream;
        private AESEncryption _aes;

        public Client(TcpClient cl)
        {
            _client = cl;
            _baseStream = _client.GetStream();
            _reader = new StreamReader(_baseStream);
            _writer = new StreamWriter(_baseStream);
        }

        public static Action<string> Log;
        private void WriteData(string data)
        {
            _writer.WriteLine(data);
            _writer.Flush();
        }
        private void WriteData(dynamic data)
        {
            WriteData(JsonConvert.SerializeObject(data));
        }
        private string ReadData()
        {
            var response = _reader.ReadLine();
            if (string.IsNullOrEmpty(response)) throw new NullReferenceException("response is empty");
            return response;
        }

        private dynamic DeserializeUserData(string data)
        {
            var userEncryptedData = JsonConvert.DeserializeObject<dynamic>(data);

            var userDecryptedData = _aes.Decrypt(userEncryptedData.data.ToString());
            var userDecryptedDataDeserialized = JsonConvert.DeserializeObject<dynamic>(userDecryptedData);
            userEncryptedData.data = userDecryptedDataDeserialized;

            return userEncryptedData;
        }

        private string GetEncryptedPrivateData()
        {

            dynamic userDataToEncrypt = new ExpandoObject();

          
            var targetLocation = "{\"targetLocation\":{\"latitude\":4.73,\"longitude\":7.56,\"type\":\"other\"}}";
            userDataToEncrypt.checkpost = targetLocation;

            var encryptedDataString = _aes.Encrypt(JsonConvert.SerializeObject(userDataToEncrypt));
            return encryptedDataString;
        }

        public void CreateSecureConnection()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            var modulus = rsa.ExportParameters(false).Modulus;

            _aes = new AESEncryption();

            var encryptedRsa = _aes.Encrypt(modulus);

            WriteData(Convert.ToBase64String(encryptedRsa));

            var newAesKey = rsa.Decrypt(Convert.FromBase64String(ReadData()), RSAEncryptionPadding.Pkcs1);

            _aes.Key = newAesKey;
        }


        public void ValidateUser()
        {
            CreateSecureConnection();

            var userFirstData = ReadData();
            if(!userFirstData.Contains("data")) throw new NullReferenceException("userFirstData is invalid");

            var userIp = ((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString();


            var userEncryptedData = DeserializeUserData(userFirstData);
            string hwidDecrypted = userEncryptedData.data.hwid.ToString();
            string hwidPublic = userEncryptedData.hwid.ToString();

            string info = $"Check: {hwidDecrypted == hwidPublic}\r\nPublic: {hwidPublic}\r\nDecrypted: {hwidDecrypted}\r\n";

            Log?.Invoke($"IP: {userIp} | Check: {hwidDecrypted == hwidPublic} | Decrypted: {hwidDecrypted}");


            var user = _database.GetUserByHwid(hwidDecrypted);
            dynamic response = new ExpandoObject();
            if (user == null)
            {
                _database.CreateUserByHwid(hwidDecrypted, userIp, info);
                response.error = "User not found!";
                WriteData(response);
                return;
            }

            if (user.userIp != userIp) user.ChangedIp = true;
            if (user.auth == null) user.auth = new List<string>();
            user.auth.Add(info);


            if (user.Active)
            {
                response.data = GetEncryptedPrivateData();
                WriteData(response);
            }
            else
            {
                response.error = "User not found!";
                WriteData(response);
            }

            _database.Update(user);
        }

        public void Dispose()
        {
            _baseStream.Close();
            _client.Close();
        }
    }
}
