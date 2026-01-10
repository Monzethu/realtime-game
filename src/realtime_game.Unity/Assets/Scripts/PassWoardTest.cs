using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class PasswordHashTest : MonoBehaviour
{
    void Start()
    {
        Print("aaa123");
        Print("bbb123");
        Print("ccc123");
    }

    static void Print(string pass)
    {
        Debug.Log($"{pass} => {Hash(pass)}");
    }

    static string Hash(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
