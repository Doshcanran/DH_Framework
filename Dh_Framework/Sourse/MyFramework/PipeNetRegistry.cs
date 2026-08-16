using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MyFramework
{
    [StaticConstructorOnStartup]
    public static class PipeNetRegistry
    {
        public static readonly List<PipeNetDef> AllDefs;

        static PipeNetRegistry()
        {
            AllDefs = DefDatabase<PipeNetDef>.AllDefsListForReading
                .OrderBy(d => d.defName)
                .ToList();

            if (AllDefs.Count > 16)
                Log.Error("[MyFramework] Больше 16 типов PipeNet — connectionGrid как ushort переполнится. Переходи на uint/ulong.");

            for (int i = 0; i < AllDefs.Count; i++)
                AllDefs[i].netTypeIndex = i;
        }
    }
}