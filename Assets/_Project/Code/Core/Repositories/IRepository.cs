using System.Collections.Generic;

namespace Galactic1.Code.Core.Repositories
{
    public interface IRepository<T>
    {
        void Register(string withId, T instance);
        void Unregister(string withId, T item);

        IReadOnlyDictionary<string,T> All { get; }

        void Clear();
    }
}