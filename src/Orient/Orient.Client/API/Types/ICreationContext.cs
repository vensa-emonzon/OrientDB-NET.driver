using System;

namespace Orient.Client
{
    public interface ICreationContext
    {
        object GetExistingObject(Orid key);
        void AddObject(Orid key, Object value);
        bool AlreadyCreated(Orid key);

        object CreateObject(string oClassName);
    }
}