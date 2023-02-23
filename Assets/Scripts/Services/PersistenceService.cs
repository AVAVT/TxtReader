using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class PersistenceService
{
    public static T LoadBinaryData<T>(string path) where T : new()
    {
        if (!File.Exists(path)) return new T();

        var bf = new BinaryFormatter();
        using var file = File.Open(path, FileMode.Open);
        return (T)bf.Deserialize(file);
    }

    public static void SaveBinaryData<T>(string fullPath, string directoryPath, T data)
    {
        Directory.CreateDirectory(directoryPath);
        var bf = new BinaryFormatter();
        using var file = File.Open(fullPath, FileMode.OpenOrCreate);
        bf.Serialize(file, data);
    }
}