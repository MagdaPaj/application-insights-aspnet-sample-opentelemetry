using OpenTelemetry.Metrics;

namespace Sample.Common
{
    public interface IAppMetrics
    {
        void Initialize(MeterFactoryBase meterFactory);
    }
}