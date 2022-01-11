using System;
using UnityEngine;

[Serializable]
public struct AvatarIdentity
{
    public static readonly AvatarIdentity Null = new AvatarIdentity(-1, "0x0", "Null", "0XF36446105FF682999A442B003F2224BCB3D82067:1");

    public bool IsNull => id == -1;
    
    public int id;
    public string avatarUri;
    public NftAddress avatar => NftAddress.ParseNft(avatarUri);

    public AvatarIdentity(int id, string ethAddress, string nickname, string avatarUri)
    {
        this.id = id;
        this.avatarUri = avatarUri;
    }
    
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }

}