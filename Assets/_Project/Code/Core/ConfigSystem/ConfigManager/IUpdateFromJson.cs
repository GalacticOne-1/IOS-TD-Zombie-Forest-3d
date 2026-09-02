namespace Galactic1.Configs
{
    public interface IUpdateFromJson
    {
        void UpdateFromJson(string json);
        
    }

    public interface IUpdateArrayFromJson
    {
        void UpdateFromJson<TData>(string json) where TData : class;
    }
}