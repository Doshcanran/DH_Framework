
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace MyFramework
{
    public class PipeNet
    {
        public readonly PipeNetDef def;
        public readonly HashSet<int> cellIndicesInNet;  // Исправлено: сделано public для доступа из PipeGrid
        public float StoredResource;
        public float Capacity => cellIndicesInNet.Count * def.maxCapacityPerCell;

        private readonly List<IPipeNetworkClient> clients = new List<IPipeNetworkClient>();

        public PipeNet(PipeNetDef def, List<IntVec3> cells, Map map)
        {
            this.def = def;
            var ci = map.cellIndices;
            cellIndicesInNet = new HashSet<int>(cells.Select(c => ci.CellToIndex(c)));

            // Подтягиваем потребителей/производителей, стоящих на этих клетках
            foreach (var cell in cells)
            {
                foreach (var thing in cell.GetThingList(map))
                {
                    if (thing is IPipeNetworkClient client && client.PipeNetDef == def)
                        clients.Add(client);
                }
            }
        }

        public bool TouchesCell(IntVec3 cell, CellIndices ci)
            => cellIndicesInNet.Contains(ci.CellToIndex(cell));

        public void NetTick()
        {
            // Тикаем сеть целиком одним проходом, а не по трубе за раз.
            float produced = 0f;

            foreach (var client in clients)
            {
                if (client.IsProducer) produced += client.DesiredThroughput;
            }

            StoredResource = Mathf.Min(StoredResource + produced, Capacity);

            foreach (var client in clients)
            {
                if (client.IsProducer) continue;
                float want = client.DesiredThroughput;
                float give = Mathf.Min(want, StoredResource);
                StoredResource -= give;
                client.ReceiveResource(give);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref StoredResource, "storedResource", 0f);
            // Сохраняем индексы клеток для корректной сериализации
            var cellsList = cellIndicesInNet.ToList();
            Scribe_Collections.Look(ref cellsList, "cellIndices", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                cellIndicesInNet.Clear();
                if (cellsList != null)
                {
                    foreach (var idx in cellsList)
                        cellIndicesInNet.Add(idx);
                }
            }
        }
    }
}