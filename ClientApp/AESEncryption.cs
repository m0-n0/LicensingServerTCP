using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Encryption
{
    public class AESEncryption
    {
        private string _key = "ABCDEFGHIJKLMNO11111111111111111";
        private string _iv = "ABCDEFGHIJKLMNO1";

        private readonly AesCryptoServiceProvider _aes = new AesCryptoServiceProvider
        {
            BlockSize = 128,
            KeySize = 256,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        };

        public AESEncryption()
        {
            _aes.IV = UTF8Encoding.UTF8.GetBytes(_iv);
            _aes.Key = UTF8Encoding.UTF8.GetBytes(_key);
        }
        public byte[] Key
        {
            get => _aes.Key;
            set => _aes.Key = value;
        }
        public void GenerateKey()
        {
            _aes.GenerateKey();
        }
        public string Encrypt(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(Encrypt(data));
        }
        public byte[] Encrypt(byte[] message)
        {
            byte[] data = message;
            using (ICryptoTransform encrypt = _aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);
                return dest;
            }
        }
        public string Decrypt(string encryptedText)
        {
            byte[] cipherText = Convert.FromBase64String(encryptedText);
            return Encoding.UTF8.GetString(Decrypt(cipherText));
        }
        public byte[] Decrypt(byte[] encryptedText)
        {
            ICryptoTransform decryptor = _aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedText, 0, encryptedText.Length);
        }
    }
}