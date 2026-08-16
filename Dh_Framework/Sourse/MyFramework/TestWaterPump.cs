using Verse;

namespace MyFramework
{
    // Тестовый водяной насос — производитель воды
    public class CompProperties_WaterPump : CompProperties
    {
        public float outputPerTick = 10f;

        public CompProperties_WaterPump()
        {
            compClass = typeof(CompWaterPump);
        }
    }

    public class CompWaterPump : ThingComp, IPipeNetworkClient
    {
        public CompProperties_WaterPump Props => (CompProperties_WaterPump)props;

        public PipeNetDef PipeNetDef => DefDatabase<PipeNetDef>.GetNamed("Water");
        public bool IsProducer => true;
        public float DesiredThroughput => Props.outputPerTick;

        public void ReceiveResource(float amount)
        {
            // Насос не потребляет, только производит
        }

        public override string CompInspectStringExtra()
        {
            var grid = parent.Map.GetComponent<PipeGrid>();
            var net = grid.NetAt(parent.Position, PipeNetDef);
            if (net == null)
                return "No network connected";
            
            return $"Water Network: {net.StoredResource:F1}/{net.Capacity:F1} (Output: {DesiredThroughput}/tick)";
        }
    }

    // Тестовый потребитель воды — радиатор
    public class CompProperties_WaterRadiator : CompProperties
    {
        public float consumptionPerTick = 5f;

        public CompProperties_WaterRadiator()
        {
            compClass = typeof(CompWaterRadiator);
        }
    }

    public class CompWaterRadiator : ThingComp, IPipeNetworkClient
    {
        private float receivedThisTick;

        public CompProperties_WaterRadiator Props => (CompProperties_WaterRadiator)props;

        public PipeNetDef PipeNetDef => DefDatabase<PipeNetDef>.GetNamed("Water");
        public bool IsProducer => false;
        public float DesiredThroughput => Props.consumptionPerTick;

        public void ReceiveResource(float amount)
        {
            receivedThisTick += amount;
        }

        public override void CompTick()
        {
            base.CompTick();
            
            // Логируем для отладки
            if (receivedThisTick > 0)
            {
                Log.Message($"[WaterRadiator] Получил воды: {receivedThisTick}");
                receivedThisTick = 0;
            }
        }

        public override string CompInspectStringExtra()
        {
            var grid = parent.Map.GetComponent<PipeGrid>();
            var net = grid.NetAt(parent.Position, PipeNetDef);
            if (net == null)
                return "No network connected";
            
            return $"Water Network: {net.StoredResource:F1}/{net.Capacity:F1} (Demand: {DesiredThroughput}/tick)";
        }
    }
}
