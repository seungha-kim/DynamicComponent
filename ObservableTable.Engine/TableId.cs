using System;

namespace ObservableTable.Engine
{
    public readonly struct TableId : IEquatable<TableId>
    {
        public string InternalID { get; }
        public TableId(string id)
        {
            InternalID = id;
        }

        public bool Equals(TableId other)
        {
            return InternalID == other.InternalID;
        }

        public override bool Equals(object? obj)
        {
            return obj is TableId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return InternalID.GetHashCode();
        }

        public static bool operator ==(TableId left, TableId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TableId left, TableId right)
        {
            return !left.Equals(right);
        }
    }
}