namespace MyFramework
{
    public interface IPipeNetworkClient
    {
        PipeNetDef PipeNetDef { get; }
        bool IsProducer { get; }
        float DesiredThroughput { get; }
        void ReceiveResource(float amount);
    }
}