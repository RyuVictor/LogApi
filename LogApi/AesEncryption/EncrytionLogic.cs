using System.Security.Cryptography;
using System.Text;

namespace LogApi.AesEncryption
{
    public class EncrytionLogic
    {
        public byte[] Encrypt(byte[] inputBytes)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("ThisIs16CharsKey"); // Replace with a valid secret key
                aesAlg.IV = new byte[16]; // Initialization Vector (IV) for AES

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(inputBytes, 0, inputBytes.Length);
                    }
                    return msEncrypt.ToArray();
                }
            }
        }
        public byte[] Decrypt(byte[] encryptedBytes)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("ThisIs16CharsKey"); // Replace with the same valid secret key used for encryption
                aesAlg.IV = new byte[16]; // Initialization Vector (IV) for AES

                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream decryptedStream = new MemoryStream())
                        {
                            csDecrypt.CopyTo(decryptedStream);
                            return decryptedStream.ToArray();
                        }
                    }
                }
            }
        }
    }
}
