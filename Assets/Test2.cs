using UnityEngine;
using System.Security.Cryptography;
using OneNetworking;
using System.Text;
using System;

public class Test2 : MonoBehaviour
{
	public static EncryptKeyPair CreateKeyPair()
	{
		CspParameters cspParams = new CspParameters { ProviderType = 1 };

		RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(1024, cspParams);

		string publicKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(false));
		string privateKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(true));

		return new EncryptKeyPair(privateKey,publicKey);
	}

	public static byte[] Encrypt(string publicKey, byte[] data)
	{
		CspParameters cspParams = new CspParameters { ProviderType = 1 };
		RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
		rsaProvider.ImportCspBlob(Convert.FromBase64String(publicKey));

		return rsaProvider.Encrypt(data, false);
	}

	public static byte[] Decrypt(string privateKey, byte[] encryptedBytes)
	{
		CspParameters cspParams = new CspParameters { ProviderType = 1 };
		RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
		rsaProvider.ImportCspBlob(Convert.FromBase64String(privateKey));

		return rsaProvider.Decrypt(encryptedBytes, false);
	}

	private EncryptKeyPair encryptionKeys;

    private void Start()
    {
		encryptionKeys = CreateKeyPair ();
		string guid = Guid.NewGuid ().ToString ();
		Debug.Log ("Start value: " + guid);
		byte[] unencryptedBytes = System.Text.Encoding.ASCII.GetBytes (guid);
		byte[] encryptedGUID = Encrypt (encryptionKeys.publicKey, unencryptedBytes);
		Debug.Log ("Encrypted: " + System.Text.Encoding.ASCII.GetString (encryptedGUID));
		byte[] decryptedGUID = Decrypt (encryptionKeys.privateKey, encryptedGUID);
		Debug.Log ("Decrypted: "+System.Text.Encoding.ASCII.GetString (decryptedGUID));
    }
}