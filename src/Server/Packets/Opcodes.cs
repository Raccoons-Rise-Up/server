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

    public enum PurchaseItemResponseOpcode
    {
        Purchased,
        NotEnoughGold
    }

    public enum LoginResponseOpcode
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
