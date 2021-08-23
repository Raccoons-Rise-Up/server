namespace GameServer.Server.Packets
{
    public enum ClientPacketOpcode
    {
        DISCONNECT,
        PURCHASE_ITEM,
        CREATE_ACCOUNT,
        LOGIN
    }

    public enum ServerPacketOpcode
    {
        CLIENT_DISCONNECTED,
        PURCHASED_ITEM,
        CREATED_ACCOUNT,
        LOGIN_RESPONSE
    }

    public enum LoginOpcode
    {
        LOGIN_SUCCESS,
        VERSION_MISMATCH
    }

    public enum ItemType 
    {
        Hut,
        Farm
    }
}
