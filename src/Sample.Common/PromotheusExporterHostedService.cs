using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics;
using OpenTelemetry.Metrics.Export;

namespace Sample.Common
{
    public class PromotheusExporterHostedService : IHostedService
    {
        private readonly PrometheusExporter exporter;
        private readonly IEnumerable<IAppMetrics> initializers;
        private PrometheusExporterMetricsHttpServer metricsHttpServer;
        //private Timer timer;
        private MeterProvider meterProvider;

        public PromotheusExporterHostedService(PrometheusExporter exporter, IEnumerable<IAppMetrics> initializers)
        {
            this.exporter = exporter ?? throw new System.ArgumentNullException(nameof(exporter));
            this.initializers = initializers;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.metricsHttpServer = new PrometheusExporterMetricsHttpServer(this.exporter);
            metricsHttpServer.Start();

            var processor = new UngroupedBatcher();
            var interval = TimeSpan.FromSeconds(5);

            MeterProvider.SetDefault(OpenTelemetry.Sdk.CreateMeterProviderBuilder()
                .SetProcessor(processor)
                .SetExporter(exporter)
                .SetPushInterval(interval)
                .Build());

            this.meterProvider = MeterProvider.Default;

            //var simpleProcessor = new UngroupedBatcher(exporter, interval);
            //this.meterFactory = MeterFactoryBase.Create(simpleProcessor);

            foreach (var initializer in initializers)
            {
                initializer.Initialize(this.meterProvider);
            }

            //this.timer = new Timer(CollectMetrics, meterFactory, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            //exporter.Start();

            //this.timer.Change(interval, interval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.metricsHttpServer.Stop();
            //timer.Dispose();
            return Task.CompletedTask;
        }
    }
}