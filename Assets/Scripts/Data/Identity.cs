using System;
using Infteract;
using UnityEngine;

[Serializable]
public struct SappyIdentity
{
    public static readonly SappyIdentity Null = new SappyIdentity(-1, "0x0", "Null", "SF_PX24_APE:4869");

    public bool IsNull => id == -1;
    
    public int id;
    public string ethAddress;
    public string nickname;
    public string avatarUri;
    public NftAddress avatar => NftAddress.ParseNft(avatarUri);

    public SappyIdentity(int id, string ethAddress, string nickname, string avatarUri)
    {
        this.id = id;
        this.nickname = nickname;
        this.ethAddress = ethAddress;
        this.avatarUri = avatarUri;
    }
    
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }

}