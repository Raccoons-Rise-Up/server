namespace GameServer.Server.Packets
{
    public enum ClientPacketType
    {
        Disconnect,
        PurchaseItem,
        CreateAccount,
        Login
    }

    public enum ServerPacketType
    {
        ClientDisconnected,
        PurchasedItem,
        CreatedAccount,
        LoginResponse
    }

    public enum LoginOpcode
    {
        LOGIN_SUCCESS,
        VERSION_MISMATCH
    }
}
