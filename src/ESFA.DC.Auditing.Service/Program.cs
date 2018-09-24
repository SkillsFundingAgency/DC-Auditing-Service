using System.Collections.Generic;
using System.IO;
using System.Threading;
using ESFA.DC.Auditing.Dto;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.Auditing.Persistence.Service;
using ESFA.DC.Auditing.Service.Configuration;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Config;
using ESFA.DC.Logging.Config.Interfaces;
using ESFA.DC.Logging.Enums;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Queueing.Interface.Configuration;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using Microsoft.Extensions.Configuration;
using ExecutionContext = ESFA.DC.Logging.ExecutionContext;

namespace ESFA.DC.Auditing.Service
{
    public static class Program
    {
#if DEBUG
        private const string ConfigFile = "privatesettings.json";
#else
        private const string ConfigFile = "appsettings.json";
#endif

        public static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(ConfigFile);

            IConfiguration configuration = configBuilder.Build();

            IAuditingPersistenceServiceConfig auditingPersistenceServiceConfig = new AudtingPersistenceServiceConfig(configuration["auditConnectionString"]);
            IQueueConfiguration queueConfiguration = new AuditQueueConfiguration(configuration["queueConnectionString"], configuration["queueName"], 1);
            ISerializationService serializationService = new JsonSerializationService();
            IApplicationLoggerSettings applicationLoggerOutputSettings = new ApplicationLoggerSettings
            {
                ApplicationLoggerOutputSettingsCollection = new List<IApplicationLoggerOutputSettings>
                {
                    new MsSqlServerApplicationLoggerOutputSettings
                    {
                        ConnectionString = configuration["logDbConString"],
                        MinimumLogLevel = LogLevel.Information
                    },
                    new ConsoleApplicationLoggerOutputSettings
                    {
                        MinimumLogLevel = LogLevel.Information
                    }
                },
                TaskKey = "Job Status",
                EnableInternalLogs = true,
                JobId = "Job Status Service",
                MinimumLogLevel = LogLevel.Information
            };
            IExecutionContext executionContext = new ExecutionContext
            {
                JobId = "Job Status Service",
                TaskKey = "Job Status"
            };
            ILogger logger = new SeriLogger(applicationLoggerOutputSettings, executionContext);
            IDateTimeProvider dateTimeProvider = new DateTimeProvider.DateTimeProvider();
            IQueueSubscriptionService<AuditingDto> queueSubscriptionService = new QueueSubscriptionService<AuditingDto>(queueConfiguration, serializationService, logger, dateTimeProvider);
            IAuditingPersistenceService<AuditingDto> auditingPersistenceService = new AuditingPersistenceService<AuditingDto>(auditingPersistenceServiceConfig, queueSubscriptionService, logger);
            auditingPersistenceService.Subscribe();

            logger.LogInfo("Started!");

            ManualResetEvent oSignalEvent = new ManualResetEvent(false);
            oSignalEvent.WaitOne();
        }
    }
}
