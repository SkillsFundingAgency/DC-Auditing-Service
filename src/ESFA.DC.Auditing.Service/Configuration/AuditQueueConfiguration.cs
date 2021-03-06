﻿using ESFA.DC.Queueing;

namespace ESFA.DC.Auditing.Service.Configuration
{
    public sealed class AuditQueueConfiguration : QueueConfiguration
    {
        public AuditQueueConfiguration(string connectionString, string queueName, int maxConcurrentCalls, string topicName = null, string subscriptionName = null, int minimumBackoffSeconds = 5, int maximumBackoffSeconds = 50, int maximumRetryCount = 10)
            : base(connectionString, queueName, maxConcurrentCalls, topicName, subscriptionName, minimumBackoffSeconds, maximumBackoffSeconds, maximumRetryCount)
        {
        }
    }
}
