using System.Security.Cryptography;
using System.Text;
using ChatChallenge.Core.Interfaces;

namespace ChatChallenge.Api.Services;

public class EncryptionService : IEncryptionService
{
  private readonly byte[] _key;

  public EncryptionService(IConfiguration configuration)
  {
    var keyString = configuration["Encryption:Key"] ?? "MySecretKey12345MySecretKey12345";

    if (keyString.Length < 32)
    {
      keyString = keyString.PadRight(32, '0');
    }
    else if (keyString.Length > 32)
    {
      keyString = keyString.Substring(0, 32);
    }

    _key = Encoding.UTF8.GetBytes(keyString);
  }

  public string Encrypt(string plainText)
  {
    if (string.IsNullOrEmpty(plainText))
      return plainText;

    using var aes = Aes.Create();
    aes.Key = _key;
    aes.GenerateIV();

    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
    
    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
    using var memoryStream = new MemoryStream();
    using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
    
    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
    cryptoStream.FlushFinalBlock();
    
    var encryptedData = memoryStream.ToArray();
    var result = new byte[aes.IV.Length + encryptedData.Length];
    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
    Array.Copy(encryptedData, 0, result, aes.IV.Length, encryptedData.Length);
    
    return Convert.ToBase64String(result);
  }

  public string Decrypt(string cipherText)
  {
    if (string.IsNullOrEmpty(cipherText))
      return cipherText;

    try
    {
      var fullCipher = Convert.FromBase64String(cipherText);
      
      var iv = new byte[16];
      Array.Copy(fullCipher, 0, iv, 0, iv.Length);
      
      var cipher = new byte[fullCipher.Length - iv.Length];
      Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);
      
      using var aes = Aes.Create();
      aes.Key = _key;
      aes.IV = iv;

      var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
      using var memoryStream = new MemoryStream(cipher);
      using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
      
      var decryptedBytes = new byte[cipher.Length];
      int decryptedByteCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
      
      return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedByteCount).TrimEnd('\0');
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Decryption failed for '{cipherText}': {ex.Message}");
      return cipherText;
    }
  }
}
