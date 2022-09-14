using System;

namespace Kwytto.Interfaces
{
    public interface IEnumerableIndex<T> where T : Enum
    {
        T Index { get; set; }
    }
}