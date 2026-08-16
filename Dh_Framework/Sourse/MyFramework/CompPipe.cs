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
            parent.Map.GetComponent<PipeGrid>()
                .RegisterPipe(parent.Position, Props.pipeNetDef);
        }

        public override string CompInspectStringExtra()
        {
            var grid = parent.Map.GetComponent<PipeGrid>();
            var net = grid.NetAt(parent.Position, Props.pipeNetDef);
            if (net == null)
                return "No network connected";
            
            return $"Network '{Props.pipeNetDef.label}': {net.StoredResource:F1}/{net.Capacity:F1}";
        }

    }
}