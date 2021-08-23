namespace GameServer.Server.Packets
{
    public enum ClientPacketOpcode
    {
        Disconnect,
        PurchaseItem,
        CreateAccount,
        Login
    }

    public enum ServerPacketOpcode
    {
        ClientDisconnected,
        PurchasedItem,
        CreatedAccount,
        LoginResponse
    }

    public enum LoginOpcode
    {
        LoginSuccess,
        VersionMismatch
    }

    public enum ItemType 
    {
        Hut,
        Farm
    }
}
