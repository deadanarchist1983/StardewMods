namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Represents the info that can be displayed about storages.</summary>
[EnumExtensions]
[Flags]
internal enum StorageInfoItem
{
    /// <summary>The name or type of storage.</summary>
    Type = 1 << 0,

    /// <summary>The location of the storage.</summary>
    Location = 1 << 1,

    /// <summary>The position of the storage.</summary>
    Position = 1 << 2,

    /// <summary>The farmer whose inventory contains the storage.</summary>
    Inventory = 1 << 3,

    /// <summary>The number of item stacks and slots in the storage.</summary>
    Capacity = 1 << 4,

    /// <summary>The total items in the storage.</summary>
    TotalItems = 1 << 5,

    /// <summary>The number of unique items in the storage.</summary>
    UniqueItems = 1 << 6,

    /// <summary>The total value of items in the storage.</summary>
    TotalValue = 1 << 7,
}