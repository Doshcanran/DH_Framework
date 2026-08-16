using Verse;

namespace MyFramework
{
    public class PipeNetDef : Def
    {
        public float maxCapacityPerCell = 100f;

        // Индекс в глобальном реестре сетей — присваивается при загрузке,
        // используется как номер бита в connectionGrid и как индекс массива сетей.
        public int netTypeIndex = -1;
    }
}