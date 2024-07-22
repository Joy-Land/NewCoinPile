using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;

namespace ThinRL.Core.Encryption
{

    public class Aes
    {
        private static RijndaelManaged rijndael = new RijndaelManaged();

        private void InitializeRijndael()
        {
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.Zeros;
        }

        public Aes(byte[] key, byte[] iv)
        {
            InitializeRijndael();

            rijndael.Key = key;
            rijndael.IV = iv;
        }

        public byte[] Decrypt(byte[] cipher)
        {
            ICryptoTransform transform = rijndael.CreateDecryptor();
            byte[] decryptedValue = transform.TransformFinalBlock(cipher, 0, cipher.Length);
            return decryptedValue;
        }

        public byte[] Encrypt(byte[] cipher)
        {
            ICryptoTransform encryptor = rijndael.CreateEncryptor();
            byte[] encryptedValue = encryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return encryptedValue;
        }
    }
}
