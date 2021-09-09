﻿namespace GameServer.Server.Packets
{
    // Received from Game Client
    public enum ClientOpcode
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
        LoginSuccessReturningPlayer,
        LoginSuccessNewPlayer,
        VersionMismatch,
        InvalidToken
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
    public enum ResourceType
    {
        Wood,
        Stone,
        Wheat,
        Gold
    }

    public enum StructureType
    {
        Hut,
        WheatFarm
    }

    public enum TechType 
    {
        
    }
}
