using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MyFramework
{
    public class PipeGrid : MapComponent
    {
        // Бит i выставлен = в этой клетке есть труба сети AllDefs[i]
        private ushort[] connectionGrid;
        private CellIndices cellIndices;

        // Одна сеть на тип на группу связности. Ключ — netTypeIndex.
        private List<PipeNet>[] networksByType;

        public PipeGrid(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            cellIndices = map.cellIndices;
            connectionGrid = new ushort[cellIndices.NumGridCells];

            networksByType = new List<PipeNet>[PipeNetRegistry.AllDefs.Count];
            for (int i = 0; i < networksByType.Length; i++)
                networksByType[i] = new List<PipeNet>();
        }

        public bool HasConnectionAt(IntVec3 cell, PipeNetDef def)
        {
            int idx = cellIndices.CellToIndex(cell);
            return (connectionGrid[idx] & (1 << def.netTypeIndex)) != 0;
        }

        public void RegisterPipe(IntVec3 cell, PipeNetDef def)
        {
            int idx = cellIndices.CellToIndex(cell);
            connectionGrid[idx] |= (ushort)(1 << def.netTypeIndex);
            RebuildNetworksAt(cell, def);
        }

        public void DeregisterPipe(IntVec3 cell, PipeNetDef def)
        {
            int idx = cellIndices.CellToIndex(cell);
            connectionGrid[idx] &= (ushort)~(1 << def.netTypeIndex);
            RebuildNetworksAt(cell, def);
        }

        // Локальный flood-fill только от изменённой клетки и её текущей сети.
        // Не трогаем сети, которых это изменение не касается — вот в чём экономия
        // по сравнению с полным пересчётом карты.
        private void RebuildNetworksAt(IntVec3 changedCell, PipeNetDef def)
        {
            var list = networksByType[def.netTypeIndex];

            // Снести все сети, которые касались этой клетки или соседей —
            // они могли распасться или объединиться.
            list.RemoveAll(net => net.TouchesCell(changedCell, cellIndices));

            var visited = new HashSet<IntVec3>();
            foreach (var neighbor in GenAdj.CardinalDirectionsAndInside
                         .Select(o => changedCell + o))
            {
                if (visited.Contains(neighbor)) continue;
                if (!neighbor.InBounds(map)) continue;
                if (!HasConnectionAt(neighbor, def)) continue;

                var net = FloodFillNet(neighbor, def, visited);
                if (net != null)
                    list.Add(net);
            }
        }

        private PipeNet FloodFillNet(IntVec3 start, PipeNetDef def, HashSet<IntVec3> visited)
        {
            var cells = new List<IntVec3>();
            var queue = new Queue<IntVec3>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                cells.Add(cur);

                foreach (var offset in GenAdj.CardinalDirections)
                {
                    var next = cur + offset;
                    if (visited.Contains(next)) continue;
                    if (!next.InBounds(map)) continue;
                    if (!HasConnectionAt(next, def)) continue;

                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }

            return cells.Count > 0 ? new PipeNet(def, cells, map) : null;
        }

        public PipeNet NetAt(IntVec3 cell, PipeNetDef def)
        {
            foreach (var net in networksByType[def.netTypeIndex])
                if (net.TouchesCell(cell, cellIndices))
                    return net;
            return null;
        }

        public override void MapComponentTick()
        {
            foreach (var list in networksByType)
                for (int i = 0; i < list.Count; i++)
                    list[i].NetTick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref connectionGrid, "connectionGrid");
            
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                // После загрузки connectionGrid пересчитываем все сети
                if (connectionGrid != null && networksByType != null)
                {
                    // Очищаем старые сети
                    for (int i = 0; i < networksByType.Length; i++)
                        networksByType[i].Clear();

                    // Пересчитываем сети на основе загруженного connectionGrid
                    var visited = new HashSet<IntVec3>();
                    for (int cellIdx = 0; cellIdx < connectionGrid.Length; cellIdx++)
                    {
                        if (connectionGrid[cellIdx] == 0) continue;

                        var cell = cellIndices.IndexToCell(cellIdx);
                        foreach (var def in PipeNetRegistry.AllDefs)
                        {
                            if (!HasConnectionAt(cell, def)) continue;
                            if (visited.Contains(cell)) continue;

                            var net = FloodFillNet(cell, def, visited);
                            if (net != null)
                                networksByType[def.netTypeIndex].Add(net);
                        }
                    }
                }
            }
        }
    }
}