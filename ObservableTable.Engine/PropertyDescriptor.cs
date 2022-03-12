using System;

namespace ObservableTable.Engine
{
    public struct PropertyDescriptor : IEquatable<PropertyDescriptor>
    {
        public TableId ID { get; }
        public string Name { get; }

        public PropertyDescriptor(TableId id, string name)
        {
            ID = id;
            Name = name;
        }

        public bool Equals(PropertyDescriptor other)
        {
            return ID.Equals(other.ID) && Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is PropertyDescriptor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ID.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(PropertyDescriptor left, PropertyDescriptor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PropertyDescriptor left, PropertyDescriptor right)
        {
            return !left.Equals(right);
        }
    }
}