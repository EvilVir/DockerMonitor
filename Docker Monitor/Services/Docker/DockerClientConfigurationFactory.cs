using Docker.DotNet;
using Docker.DotNet.BasicAuth;
using Docker.DotNet.X509;
using Microsoft.Extensions.Logging;
using StrangeFog.Docker.Monitor.Configuration;
using System;
using System.Security.Cryptography.X509Certificates;

namespace StrangeFog.Docker.Monitor.Services.Docker
{
    public class DockerClientConfigurationFactory
    {
        protected readonly ILogger logger;

        public DockerClientConfigurationFactory(ILogger<DockerClientConfigurationFactory> logger)
        {
            this.logger = logger;
        }

        public DockerClientConfiguration Create(DockerEngineConfiguration configuration)
        {
            var credentials = CreateCredentials(configuration.BasicAuthorization) ?? 
                              CreateCredentials(configuration.CertificateAuthorization);

            var uri = new Uri(configuration.Uri);
            var output = new DockerClientConfiguration(uri, credentials, TimeSpan.FromMinutes(1));

            logger.LogInformation(LogEventId.DOCKER_CLIENT_CONFIGURATION_CREATED, "Docker client configuration created for URI {Uri} with authorization {AuthorizationType}", 
                                    uri, 
                                    credentials?.GetType().ToString() ?? "None");

            return output;
        }

        protected Credentials CreateCredentials(DockerEngineBasicAuthConfiguration configuration)
        {
            if (configuration != null)
            {
                return new BasicAuthCredentials(configuration.Username, configuration.Password, configuration.UseTls);
            }

            return null;
        }

        protected Credentials CreateCredentials(DockerEngineX509AuthConfiguration configuration)
        {
            if (configuration != null)
            {
                var cert = string.IsNullOrEmpty(configuration.Password) ?
                            new X509Certificate2(configuration.File) :
                            new X509Certificate2(configuration.File, configuration.Password);

                return new CertificateCredentials(cert);
            }

            return null;
        }
    }
}
