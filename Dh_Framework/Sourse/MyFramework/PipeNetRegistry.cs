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

            // Исправление: увеличено ограничение с 16 до 32 типов сетей (uint вместо ushort)
            if (AllDefs.Count > 32)
                Log.Error($"[MyFramework] Больше {AllDefs.Count} типов PipeNet — возможно переполнение uint. Рассмотрите переход на ulong.");

            for (int i = 0; i < AllDefs.Count; i++)
                AllDefs[i].netTypeIndex = i;
        }
    }
}