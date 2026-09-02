public sealed class ItemStackView
{
    public readonly string ItemId;
    public readonly int Count;

    public ItemStackView(string id, int count)
    {
        ItemId = id;
        Count = count;
    }
}