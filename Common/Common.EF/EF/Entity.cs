
using Common.EF.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DamainEvent
{
    public abstract class Entity<TKey> : IEntity<TKey>
     where TKey : IEquatable<TKey>
    {
        public virtual TKey Id { get; set; } = default!;

        protected Entity() { }

        protected Entity(TKey id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TKey> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Id?.Equals(default) == true || other.Id?.Equals(default) == true)
                return false;

            return Id!.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }

        public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
        {
            return !Equals(left, right);
        }
    }
}
