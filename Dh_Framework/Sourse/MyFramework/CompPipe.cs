using Verse;

namespace MyFramework
{
    public class CompProperties_Pipe : CompProperties
    {
        public PipeNetDef pipeNetDef;

        public CompProperties_Pipe()
        {
            compClass = typeof(CompPipe);
        }
    }

    public class CompPipe : ThingComp
    {
        public CompProperties_Pipe Props => (CompProperties_Pipe)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            // Исправление: проверка на null для PipeGrid
            var grid = parent.Map?.GetComponent<PipeGrid>();
            if (grid != null)
            {
                grid.RegisterPipe(parent.Position, Props.pipeNetDef);
            }
        }

        public override string CompInspectStringExtra()
        {
            // Исправление: проверка на null для PipeGrid
            var grid = parent.Map?.GetComponent<PipeGrid>();
            if (grid == null)
                return "No grid available";
                
            var net = grid.NetAt(parent.Position, Props.pipeNetDef);
            if (net == null)
                return "No network connected";
            
            return $"Network '{Props.pipeNetDef.label}': {net.StoredResource:F1}/{net.Capacity:F1}";
        }

    }
}