# DC-Auditing-Service

Auditing web job that runs continously as a singleton receiving audit message DTO's placed on the audit queue from other services. Audit messages are then persisted into the Auditing database.