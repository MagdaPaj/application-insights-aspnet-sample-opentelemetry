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
        private Timer timer;
        private MeterFactoryBase meterFactory;

        public PromotheusExporterHostedService(PrometheusExporter exporter, IEnumerable<IAppMetrics> initializers)
        {
            this.exporter = exporter ?? throw new System.ArgumentNullException(nameof(exporter));
            this.initializers = initializers;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //var metricsHttpServer = new PrometheusExporterMetricsHttpServer(this.exporter);
            //metricsHttpServer.Start();

            //var processor = new UngroupedBatcher();
            var interval = TimeSpan.FromSeconds(5);

            //MeterProvider.SetDefault(OpenTelemetry.Sdk.CreateMeterProviderBuilder()
            //    .SetProcessor(processor)
            //    .SetExporter(exporter)
            //    .SetPushInterval(interval)
            //    .Build());

            //var meterProvider = MeterProvider.Default;
            //var meter = meterProvider.GetMeter("MyMeter");

            var simpleProcessor = new UngroupedBatcher(exporter, interval);
            this.meterFactory = MeterFactoryBase.Create(simpleProcessor);

            foreach (var initializer in initializers)
            {
                initializer.Initialize(meterFactory);
            }

            this.timer = new Timer(CollectMetrics, meterFactory, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            exporter.Start();

            this.timer.Change(interval, interval);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Need to dig deeper into this
        /// This call should not be needed
        /// </summary>
        /// <param name="state"></param>
        private static void CollectMetrics(object state)
        {
            var meterFactory = (MeterFactoryBase)state;
            var m = meterFactory.GetMeter("Sample App");
            ((MeterSdk)m).Collect();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            exporter.Stop();
            timer.Dispose();
            return Task.CompletedTask;
        }
    }
}