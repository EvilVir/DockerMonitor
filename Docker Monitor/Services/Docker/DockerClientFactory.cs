using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace StrangeFog.Docker.Monitor.Services.Docker
{
    public class DockerClientFactory
    {
        protected ILogger logger;
        protected readonly DockerClientConfiguration dockerClientConfiguration;

        public DockerClientFactory(ILogger<DockerClientFactory> logger, DockerClientConfiguration dockerClientConfiguration)
        {
            this.logger = logger;
            this.dockerClientConfiguration = dockerClientConfiguration;
        }

        public DockerClient Create()
        {
            return dockerClientConfiguration.CreateClient();
        }
    }
}
