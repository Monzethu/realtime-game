using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Program : MonoBehaviour
{
    void Start()
    {
        Print("aaa123");
        Print("bbb123");
        Print("ccc123");
        Print("ddd123");
    }

    void Print(string password)
    {
        using var sha = SHA256.Create();
        var hash = Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(password))
        );

        Debug.Log($"{password} => {hash}");
    }
}
