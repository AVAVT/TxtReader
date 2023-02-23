using System;

public static class Services<T>
{
    static T _instance;

    public static void Bind(T instance) => _instance = instance;

    public static T Get()
    {
        if (_instance == null) throw new Exception($"You didn't bind any instance for {typeof(T).Name} service!!!");
        return _instance;
    }
}