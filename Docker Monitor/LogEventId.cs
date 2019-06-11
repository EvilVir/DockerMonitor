namespace StrangeFog.Docker.Monitor
{
    public static class LogEventId
    {
        // Command executor
        public const int COMMAND_EXECUTOR_COUNT = 101;
        public const int COMMAND_EXECUTOR_START = 102;
        public const int COMMAND_EXECUTOR_START_DETAILS = 103;
        public const int COMMAND_EXECUTOR_STDOUT = 104;
        public const int COMMAND_EXECUTOR_STDERR = 105;
        public const int COMMAND_EXECUTOR_STOP = 106;
        public const int COMMAND_EXECUTOR_EXCEPTION = 107;
        public const int COMMAND_EXECUTOR_EXCEPTION_DETAILS = 108;

        // Monitoring service
        public const int MONITORING_SERVICE_FILTER_CREATED = 201;
        public const int MONITORING_SERVICE_CHECK_START = 202;
        public const int MONITORING_SERVICE_GOT_INFO = 203;
        public const int MONITORING_SERVICE_CHECK_COMPLETED = 204;
        public const int MONITORING_SERVICE_CHECK_ERROR = 205;
        public const int MONITORING_SERVICE_ACTIONS_DETAILS = 206;
        public const int MONITORING_SERVICE_ACTIONS_MATCH_DETAILS = 207;
        public const int MONITORING_SERVICE_ACTIONS_CALCULATOR = 208;
        public const int MONITORING_SERVICE_ACTIONS_CALCULATOR_STATES = 209;

        // Monitoring service factory
        public const int MONITORING_SERVICE_FACTORY_CREATED = 301;

        // Docker client configuration factory
        public const int DOCKER_CLIENT_CONFIGURATION_CREATED = 401;

        // Monitoring job
        public const int MONITORING_JOB_START = 501;
        public const int MONITORING_JOB_ERROR = 502;
        public const int MONITORING_JOB_ERROR_DETAILS = 503;
        public const int MONITORING_JOB_END = 504;

        // Scheduled monitoring service
        public const int SCHEDULED_MONITORING_SERVICE_START_BEGIN = 601;
        public const int SCHEDULED_MONITORING_SERVICE_START_DETAILS = 602;
        public const int SCHEDULED_MONITORING_SERVICE_START_SCHEDULER_STARTING = 603;
        public const int SCHEDULED_MONITORING_SERVICE_START_DONE = 604;
        public const int SCHEDULED_MONITORING_SERVICE_PAUSE_BEGIN = 605;
        public const int SCHEDULED_MONITORING_SERVICE_PAUSE_DONE = 606;
        public const int SCHEDULED_MONITORING_SERVICE_RESUME_BEGIN = 607;
        public const int SCHEDULED_MONITORING_SERVICE_RESUME_DONE = 608;
        public const int SCHEDULED_MONITORING_SERVICE_STOP_BEGIN = 609;
        public const int SCHEDULED_MONITORING_SERVICE_STOP_DONE = 610;

        // Events Bus
        public const int EVENTS_BUS_PUBLISH_START = 701;
        public const int EVENTS_BUS_HANDLERS_COUNT = 702;
        public const int EVENTS_BUS_HANDLER_START = 703;
        public const int EVENTS_BUS_HANDLER_END = 704;
        public const int EVENTS_BUS_PUBLISH_END = 705;
    }
}
