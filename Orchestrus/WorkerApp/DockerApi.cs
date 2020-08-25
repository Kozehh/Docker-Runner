using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace WorkerApp
{
    public class DockerApi : IAsyncLifetime
    {
        //public static string hostAddr = "192.168.146.129";
        // Dictionary that associates the images name and container id
        private volatile Dictionary<string, string> _imagesExecuted = new Dictionary<string, string>();
        private readonly DockerClient _dockerClient;
        private string _containerId;
        private readonly string _image;
        private readonly string _hostPort;
        private readonly string _containerPort;
        private string stoppingContainer;

        public DockerApi(string hostAddr, string image, string hostPort, string containerPort)
        {
            _dockerClient = new DockerClientConfiguration(new Uri($"tcp://{hostAddr}:2375")).CreateClient();
            _image = image;
            _hostPort = hostPort;
            _containerPort = containerPort;
        }

        public async Task InitializeAsync()
        {
            await PullImage();
            await StartContainer();
        }

        // On va chercher l'image qu'on veut start
        private async Task PullImage()
        {
            try
            {
                await _dockerClient.Images
                    .CreateImageAsync(new ImagesCreateParameters
                        {
                            FromImage = _image,
                            Tag = "latest"
                        },
                        new AuthConfig(),
                        new Progress<JSONMessage>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        // On lance le container avec l'image précédement pull
        private async Task StartContainer()
        {
            // _hostPort et _containerPort sont les ports spécifié par le client (_hostPort:_containerPort)
            try
            {
                var response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = _image,
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        {
                            _containerPort, default(EmptyStruct)
                        }
                    },
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {_containerPort, new List<PortBinding> {new PortBinding {HostPort = _hostPort}}}
                        },
                        PublishAllPorts = true
                    }
                });
                // Si le container s'est créé correctement, on reçoit l'ID de ce dernier
                _containerId = response.ID;
                Console.WriteLine(_containerId);

                // On regarde le status de l'image qu'on vient de lancer pour voir s'il est "created"
                ContainerInspectResponse res = await _dockerClient.Containers.InspectContainerAsync(_containerId, CancellationToken.None);
                Console.WriteLine($"Status for {_image} : {res.State.Status}");

                // On lance le container
                await _dockerClient.Containers.StartContainerAsync(_containerId, null);

                // On ajoute l'image exécutée associé avec son id dans notre dictionnaire
                _imagesExecuted.Add(_image, _containerId);
                Console.WriteLine($"id of image {_image} is : {_imagesExecuted[_image]}");

                // On regarde le status de l'image qu'on vient de lancer pour voir s'il est "running"
                res = await _dockerClient.Containers.InspectContainerAsync(_containerId, CancellationToken.None);
                Console.WriteLine($"Status for {_image} : {res.State.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task StopContainer(string containerToStop)
        {
            Console.WriteLine("Stppping container : " + containerToStop);
            stoppingContainer = _imagesExecuted[containerToStop];
            await DisposeAsync();
        }

        public async Task DisposeAsync()
        {
            if (stoppingContainer != null)
            {
                await _dockerClient.Containers.KillContainerAsync(stoppingContainer, new ContainerKillParameters());
            }
        }

        public async Task<string> CheckContainerStatus()
        {
            ContainerInspectResponse res = await _dockerClient.Containers.InspectContainerAsync(_containerId, CancellationToken.None);
            return res.State.Status;
        }
    }
}
