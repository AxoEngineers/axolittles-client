using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public enum BuildEnvironment
{
    Local,
    Development,
    Production
}

public static class Configuration
{
    public static bool IsDev => GetEnv() == BuildEnvironment.Development || GetEnv() == BuildEnvironment.Local;

    public static BuildEnvironment GetEnv()
    {
        #if UNITY_EDITOR
        return BuildEnvironment.Local;
        #else
        string url = GetURL();
        bool localhost = url.Contains("127.0.0.1") || url.Contains("localhost");
        
        if (localhost)
        {
            return BuildEnvironment.Local;
        }
        else
        {
            if (url.Contains("axoquarium-test.herokuapp.com"))
            {
                return BuildEnvironment.Development;
            }
            else
            {
                return BuildEnvironment.Production;
            }
        }
        #endif
    }
    
    public static string GetWeb3URL()
    {
        switch (GetEnv())
        {
            case BuildEnvironment.Local: return "http://localhost:3000/";
            case BuildEnvironment.Production: return "https://axoquarium.herokuapp.com/";
        }

        return null;
    }
    
    public static string GetEnvName()
    {
        switch (GetEnv())
        {
            case BuildEnvironment.Local: return "LOCAL";
            case BuildEnvironment.Development: return "DEV";
            case BuildEnvironment.Production: return "PROD";
        }

        return "null";
    }
    
    public static string GetLocalServerWs()
    {
        if (GetEnv() == BuildEnvironment.Local)
        {
            return "ws://127.0.0.1:7667";
        }

        throw new Exception("This function should not be called on dev/prod.");
    }

    [DllImport("__Internal")]
    public static extern string GetURL();
}

public static class UriHelper
{
    public static Dictionary<string, string> DecodeQueryParameters(this Uri uri)
    {
        if (uri == null)
            throw new ArgumentNullException("uri");

        if (uri.Query.Length == 0)
            return new Dictionary<string, string>();

        return uri.Query.TrimStart('?')
            .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(parameter => parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
            .GroupBy(parts => parts[0],
                parts => parts.Length > 2 ? string.Join("=", parts, 1, parts.Length - 1) : (parts.Length > 1 ? parts[1] : ""))
            .ToDictionary(grouping => grouping.Key,
                grouping => string.Join(",", grouping));
    }
}