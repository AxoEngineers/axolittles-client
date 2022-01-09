using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Infteract;
using UnityEngine;
using UnityEngine.Events;

public enum NftType
{
    UNKNOWN,
    SAPPYSEALS,
    WINTERBEARS,
    PX24_CAT,
    PX24_APE
}

public struct NftAddress : IComparable
{
    public static readonly NftAddress Null = new NftAddress("0x0", "-1");
    
    public NftType type;
    public int id;
    public string contract;

    public NftAddress(string contract, string nftId)
    {
        this.contract = contract;
        switch (contract.ToUpperInvariant())
        {
            case "SF_PX24_CAT": type = NftType.PX24_CAT; break;
            case "SF_PX24_APE": type = NftType.PX24_APE; break;
            case "0X364C828EE171616A39897688A831C2499AD972EC": type = NftType.SAPPYSEALS; break;
            case "0XC8BCBE0E8AE36D8F9238CD320EF6DE88784B1734": type = NftType.WINTERBEARS; break;
            default: type = NftType.UNKNOWN; break;
        }
        id = int.Parse(nftId);
    }

    public static NftAddress ParseNft(string rawData) {
        if (rawData.Contains(":"))
        {
            var split = rawData.Split(':');
            return new NftAddress(split[0], split[1]);
        }

        throw new Exception("Invalid NFT format.");
    }
    
    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;
        return id.CompareTo(((NftAddress) obj).id);
    }

    public override string ToString()
    {
        return $"{contract}:{id}";
    }
}

public class WalletInfo
{
    public string eth_address { get; private set; }
    public string signature { get; private set; }
    
    public NftAddress[] avatars { get; private set; } // in ids
    
    public string full { get; private set; }

    public WalletInfo(string serializedData)
    {
        full = serializedData;

        string buffer = serializedData;

        // (1) get eth address first
        var delim = buffer.IndexOf('|');
        eth_address = buffer.Substring(0, delim);

        // (2) get signature next
        buffer = buffer.Substring(delim + 1);
        delim = buffer.IndexOf('|');
        signature = buffer.Substring(0, delim);
        
        // (3) parse the seal ids
        buffer = buffer.Substring(delim + 1);
        avatars = ParseNfts(buffer);
    }

    private NftAddress[] ParseNfts(string sealData)
    {
        sealData = sealData.StartsWith("[") && sealData.EndsWith("]") ? sealData.Substring(1, sealData.Length - 2) : sealData;
        sealData = sealData.Replace("\"", "");
        Debug.Log(sealData);
        
        List<NftAddress> nftAddresses = new List<NftAddress>();
        if (sealData.Length > 0)
        {
            if (sealData.Contains(","))
            {
                foreach (var nft in sealData.Split(','))
                {
                    nftAddresses.Add(NftAddress.ParseNft(nft));
                }

                return nftAddresses.ToArray();
            }

            nftAddresses.Add(NftAddress.ParseNft(sealData));
        }

        return nftAddresses.ToArray();
    }

}

public class MetamaskAuth : Mingleton<MetamaskAuth>
{
    [DllImport("__Internal")]
    private static extern void MetamaskAuthenticate(string web3URL);
    
    private WalletInfo walletInfo;

    public WalletInfo Wallet => walletInfo;
    public NftAddress SelectedAvatar { get; set; }
    
    public UnityEvent<string, string> onLoginFail = new UnityEvent<string, string>();
    public UnityEvent<WalletInfo> onLoginData = new UnityEvent<WalletInfo>();

    public bool Authenticated => walletInfo != null;
    
    new void Awake()
    {
        base.Awake();
        gameObject.name = "MetamaskAuth"; // REQUIRED FOR COMMUNICATION DO NOT CHANGE
    }

    public void CompleteMetamaskAuth(string data) // JS LIB EVENT DONT TOUCH
    {
        walletInfo = new WalletInfo(data);
        onLoginData.Invoke(walletInfo);
        
        // sample address: 0x38e6b922545cd931030ad7fb4c00409a04b213c4
        var ethAddress = "0x38e6b922545cd931030ad7fb4c00409a04b213c4";
        if (!Configuration.IsDev)
        {
            ethAddress = walletInfo.eth_address;
        }
    }

    public void CompleteMetamaskError(string data)
    {
        var delim = data.IndexOf('|');
        var code = data.Substring(0, delim);
        var message = data.Substring(delim + 1);
        onLoginFail.Invoke(code, message);
    }

    public IEnumerator Authenticate()
    {
        MetamaskAuthenticate(Configuration.GetWeb3URL());
        yield return null;
    }
}