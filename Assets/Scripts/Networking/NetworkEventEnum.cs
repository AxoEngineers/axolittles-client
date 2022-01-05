namespace LwNetworking
{
    public enum NetworkEventEnum
    {
        Connect,
        Disconnect,
        PlayersUpdate,
        PlayerConnect,
        PlayerDisconnect,
        PlayerInfoRequest,
        AuthCheck, // for now this is all done through javascript
        PlayerUpdate,
        AuthSuccess,
        AuthFailure,
        PlayerChat,
        AuthVerify,
        Error,
        PropCreate,
        PropDestroy,
        PropUpdate,
        PropLevelLoad, // request all props
        PlayerPermissions
    }
}