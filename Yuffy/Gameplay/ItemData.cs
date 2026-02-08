using System.Collections.Generic;

namespace Yuffy.Gameplay;

public enum ItemType
{
    None,
    Sword,
    Axe,
    Pickaxe,
    Hammer,
    Rod,
    WateringCan,
    Seeds,
    Basket,
    Letter
}

public class ItemData
{
    public ItemType Type { get; }
    public string TextureKey { get; }
    public string Name { get; }

    public ItemData(ItemType type, string textureKey, string name)
    {
        Type = type;
        TextureKey = textureKey;
        Name = name;
    }

    public static readonly Dictionary<ItemType, ItemData> Catalog = new()
    {
        { ItemType.Sword, new ItemData(ItemType.Sword, "images/tilesets/UI/sword", "Sword") },
        { ItemType.Axe, new ItemData(ItemType.Axe, "images/tilesets/UI/axe", "Axe") },
        { ItemType.Pickaxe, new ItemData(ItemType.Pickaxe, "images/tilesets/UI/pickaxe", "Pickaxe") },
        { ItemType.Hammer, new ItemData(ItemType.Hammer, "images/tilesets/UI/hammer", "Hammer") },
        { ItemType.Rod, new ItemData(ItemType.Rod, "images/tilesets/UI/rod", "Rod") },
        { ItemType.WateringCan, new ItemData(ItemType.WateringCan, "images/tilesets/UI/water", "Watering Can") },
        { ItemType.Seeds, new ItemData(ItemType.Seeds, "images/tilesets/UI/plant", "Seeds") },
        { ItemType.Basket, new ItemData(ItemType.Basket, "images/tilesets/UI/basket", "Basket") },
        { ItemType.Letter, new ItemData(ItemType.Letter, "images/tilesets/UI/expression_chat", "Letter") },
    };
}

public class ItemSlot
{
    public ItemType Item { get; set; } = ItemType.None;
    public int Count { get; set; }

    public bool IsEmpty => Item == ItemType.None || Count <= 0;
}
