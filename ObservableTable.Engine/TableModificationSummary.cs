using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal class TableModificationSummary
    {
        private readonly HashSet<TableId> _createdTableIds;
        private readonly HashSet<TableId> _removedTableIds;
        private readonly HashSet<(TableId table, TableId? oldParent, TableId? newParent)> _parentUpdates;
        private readonly HashSet<PropertyDescriptor> _updatedProperties;
        private readonly HashSet<PropertyDescriptor> _removedProperties;

        internal IEnumerable<TableId> CreatedTableIds => _createdTableIds;
        internal IEnumerable<TableId> RemovedTableIds => _removedTableIds;
        internal IEnumerable<(TableId table, TableId? oldParent, TableId? newParent)> ParentUpdates => _parentUpdates;
        internal IEnumerable<PropertyDescriptor> UpdatedProperties => _updatedProperties;
        internal IEnumerable<PropertyDescriptor> RemovedProperties => _removedProperties;

        internal TableModificationSummary()
        {
            _createdTableIds = new HashSet<TableId>();
            _removedTableIds = new HashSet<TableId>();
            _parentUpdates = new HashSet<(TableId table, TableId? oldParent, TableId? newParent)>();
            _updatedProperties = new HashSet<PropertyDescriptor>();
            _removedProperties = new HashSet<PropertyDescriptor>();
        }

        internal void Connect(TableScriptRepository scriptRepository)
        {
            scriptRepository.OnTableCreated += id => _createdTableIds.Add(id);
            scriptRepository.OnTableRemoved += id => _removedTableIds.Add(id);
            scriptRepository.OnParentUpdate += (id, parent, newParent) => _parentUpdates.Add((id, parent, newParent));
            scriptRepository.OnPropertyUpdated += desc => _updatedProperties.Add(desc);
            scriptRepository.OnPropertyRemoved += desc => _removedProperties.Add(desc);
        }

        internal void Clear()
        {
            _createdTableIds.Clear();
            _removedProperties.Clear();
            _parentUpdates.Clear();
            _updatedProperties.Clear();
            _removedProperties.Clear();
        }
    }
}