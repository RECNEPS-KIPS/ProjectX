namespace GamePlay.Item
{
    public interface IItemable
    {
        int ID { get; set; }
        ItemConfig ItemConfig { get; }
    }
}