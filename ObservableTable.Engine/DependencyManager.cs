using System.Collections.Generic;
using System.Linq;

namespace ObservableTable.Engine
{
    internal class DependencyManager
    {
        internal bool IsCyclic => Cycle is { };
        private List<PropertyDescriptor>? Cycle { get; set; }

        internal IEnumerable<PropertyDescriptor>? GetPropertyCycle()
        {
            return Cycle;
        }

        internal void AnalysisDependencies(IDictionary<TableId, TableScript> scripts)
        {
            HashSet<TableId> nodes = new HashSet<TableId>(scripts.Keys);
            while (nodes.Count > 0)
            {
                var id = nodes.First();
                nodes.Remove(id);
                // TODO: 이 시점에 파싱이 되어있어야 함
                // TODO: TableScript 가 AST를 들고있는게 좋을듯
                // TODO: TableScript 혹은 다른 무언가가 AST 순회하면서 의존성 분석하기 - DependencyManager?
                // TODO: 간단한 정적 분석..
                // scripts[id];
            }
        }

        internal IEnumerable<PropertyDescriptor> GetSenders(PropertyDescriptor desc)
        {
            throw new System.NotImplementedException();
        }

        internal IEnumerable<PropertyDescriptor> GetReceivers(PropertyDescriptor desc)
        {
            throw new System.NotImplementedException();
        }

        internal IEnumerable<PropertyDescriptor> GetDependentProperties(PropertyDescriptor desc)
        {
            throw new System.NotImplementedException();
        }
    }
}