﻿using System;
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
            if (url.Contains("testdev.sappyseals.io"))
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
            case BuildEnvironment.Development: return "https://testdev.sappyseals.io/";
            case BuildEnvironment.Production: return "https://master.sappyseals.io/";
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
    private static extern string GetURL();
}