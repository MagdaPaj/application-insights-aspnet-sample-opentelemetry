﻿using System;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using Sample.Common;

namespace Sample.TimeApi.Data
{
    public class OpenTelemetryCollectingDeviceRepository<TDeviceRepository> : IDeviceRepository
           where TDeviceRepository : IDeviceRepository
    {
        private readonly TDeviceRepository repository;
        private Tracer tracer;

        public OpenTelemetryCollectingDeviceRepository(TDeviceRepository repository, TracerProvider tracerProvider)
        {
            this.tracer = tracerProvider.GetTracer("sql");
            this.repository = repository;
        }

        public async Task<DateTime> GetTimeFromSqlAsync()
        {
            var span = this.tracer.StartSpan(nameof(GetTimeFromSqlAsync), SpanKind.Client);
            try
            {
                FailGenerator.FailIfNeeded(1);

                return await this.repository.GetTimeFromSqlAsync();
            }
            catch (Exception ex)
            {
                span.SetAttribute("error", true);
                span.SetStatus(Status.Error.WithDescription(ex.ToString()));
                throw;
            }
            finally
            {
                span.End();
            }

        }
    }
}
