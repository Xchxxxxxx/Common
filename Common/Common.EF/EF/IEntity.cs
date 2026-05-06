using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    public interface IEntity<TKey> where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
