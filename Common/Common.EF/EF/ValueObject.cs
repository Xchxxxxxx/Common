using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;

            using var thisComponents = GetEqualityComponents().GetEnumerator();
            using var otherComponents = other.GetEqualityComponents().GetEnumerator();

            while (thisComponents.MoveNext() && otherComponents.MoveNext())
            {
                if (thisComponents.Current is null && otherComponents.Current is null)
                    continue;

                if (thisComponents.Current is null || otherComponents.Current is null)
                    return false;

                if (!thisComponents.Current.Equals(otherComponents.Current))
                    return false;
            }

            return !thisComponents.MoveNext() && !otherComponents.MoveNext();
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !Equals(left, right);
        }
    }
}
