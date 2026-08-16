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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            // Исправление бага гонки данных: регистрируем клиента в сети после спавна
            var grid = parent.Map.GetComponent<PipeGrid>();
            var net = grid.NetAt(parent.Position, PipeNetDef);
            if (net != null && !net.clients.Contains(this))
            {
                // Используем рефлексию для добавления клиента в приватный список
                var clientsField = typeof(PipeNet).GetField("clients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (clientsField != null)
                {
                    var clients = (System.Collections.Generic.List<IPipeNetworkClient>)clientsField.GetValue(net);
                    clients.Add(this);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            // Исправление: проверка на null для PipeGrid
            var grid = parent.Map?.GetComponent<PipeGrid>();
            if (grid == null)
                return "No grid available";
                
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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            // Исправление бага гонки данных: регистрируем клиента в сети после спавна
            var grid = parent.Map.GetComponent<PipeGrid>();
            var net = grid.NetAt(parent.Position, PipeNetDef);
            if (net != null && !net.clients.Contains(this))
            {
                // Используем рефлексию для добавления клиента в приватный список
                var clientsField = typeof(PipeNet).GetField("clients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (clientsField != null)
                {
                    var clients = (System.Collections.Generic.List<IPipeNetworkClient>)clientsField.GetValue(net);
                    clients.Add(this);
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            
            // Логируем для отладки
            if (receivedThisTick > 0f)
            {
                Log.Message($"[WaterRadiator] Получил воды: {receivedThisTick}");
            }
            // Исправление бага: сбрасываем receivedThisTick ПОСЛЕ обработки, а не до
            receivedThisTick = 0f;
        }

        public override string CompInspectStringExtra()
        {
            // Исправление: проверка на null для PipeGrid
            var grid = parent.Map?.GetComponent<PipeGrid>();
            if (grid == null)
                return "No grid available";
                
            var net = grid.NetAt(parent.Position, PipeNetDef);
            if (net == null)
                return "No network connected";
            
            return $"Water Network: {net.StoredResource:F1}/{net.Capacity:F1} (Demand: {DesiredThroughput}/tick)";
        }
    }
}
