using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using HydroModTools.Client.Abstractions;
using HydroModTools.Client.WinForms;
using HydroModTools.Client.Wpf;
using HydroModTools.Common;
using HydroModTools.Common.Enums;
using HydroModTools.Enums;
using HydroModTools.Tools;
using Microsoft.Extensions.Configuration;

namespace HydroModTools
{
    internal sealed class HydroModTools
    {
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private IClient? _client;
        private readonly Configuration.Configuration _config = new Configuration.Configuration();
        private IContainer? _services;

        private VisualClients _selectedClient = VisualClients.Wpf;

        public void PrepareApplication()
        {
            switch (_selectedClient)
            {
                case VisualClients.WinForms:
                    _client = new WinFormsClient();
                    break;
                case VisualClients.Wpf:
                    _client = new WpfClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _client.ShutdownApp += delegate
            {
                _cancellationToken.Cancel();
            };

            var services = new ContainerBuilder();
            _client.RegisterClientTypes(services);

            services
                .Register((ctx) => new ServiceProvider(_services!))
                .AsImplementedInterfaces()
                .SingleInstance();
            
            services.RegisterAssemblyTypes(typeof(Assembly).Assembly, typeof(DataAccess.Assembly).Assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .SingleInstance();

            services
                .RegisterType<PakerV3>();
            services
                .RegisterType<Stager>();
            services.Register((c) =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.AllowAutoRedirect = false;

                return new HttpClient(httpClientHandler);
            });
            
            services
                .Register(ctx => _config)
                .SingleInstance();

            _services = services.Build();

            _config.SetupConfiguration();

            var config = new ConfigurationBuilder();
            config.AddJsonFile($"{AppVars.ConfigPath}.json", false, true);
            config.AddJsonFile($"{AppVars.GuidsConfigPath}.json", false, true);
            config.AddJsonFile($"{AppVars.UIDsConfigPath}.json", false, true);
            _config.LoadConfigAsync(config.Build()).Wait();

            _client.ConfigureServices(_services);
            DataInterop.Instance.AppLoadStage = AppLoadStage.Preload;
            _client.ToggleSplash(true);
        }

        public async Task RunApplication()
        {
            DataInterop.Instance.AppLoadStage = AppLoadStage.Load;

            await Task.Delay(5000);
            await _client!.ToggleSplash(false);
            
            await _client!.RunClient();
        }
        
        private class ServiceProvider : IServiceProvider
        {
            private readonly IContainer _container;

            public ServiceProvider(IContainer container)
            {
                _container = container;
            }
            
            public object? GetService(Type serviceType)
            {
                return _container.Resolve(serviceType);
            }
        }

        public void UseWpf()
        {
            _selectedClient = VisualClients.Wpf;
        }

        public void UseWinForms()
        {
            _selectedClient = VisualClients.WinForms;
        }
    }
}