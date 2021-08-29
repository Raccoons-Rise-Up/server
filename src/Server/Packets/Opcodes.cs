namespace GameServer.Server.Packets
{
    // Received from Game Client
    public enum ClientPacketOpcode
    {
        Disconnect,
        PurchaseItem,
        CreateAccount,
        Login
    }

    // Sent to Game Client
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

    public enum DisconnectOpcode
    {
        Disconnected,
        Maintenance,
        Restarting,
        Kicked,
        Banned
    }

    // Sent and received (Game Server / Game Client)
    public enum ItemType
    {
        Hut,
        Farm
    }

    // Web Client
    public class WebLoginResponse
    {
        public int opcode { get; set; }
    }

    public enum WebRegisterResponseOpcode
    {
        AccountCreated,
        AccountExistsAlready,
        InvalidUsernameOrPassword
    }

    public enum WebLoginResponseOpcode
    {
        LoginSuccess,
        InvalidUsernameOrPassword,
        AccountDoesNotExist,
        PasswordsDoNotMatch
    }
}
