namespace Fcl.Net.Core.Platform
{
    public interface IPlatform
    {
        string Location();
        //TODO - storage/sessions
        //Task StorageSet<T>( Dictionary<string, T> items);
        //Task<T> StorageGet<T>(string key);
        //Task StorageRemove<T>(string key);
    }
}
