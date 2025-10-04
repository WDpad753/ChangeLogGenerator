using BaseClass.Base.Interface;
using BaseClass.ConsoleAppBase;
using BaseClass.ConsoleAppBase.BaseWorker;
using BaseLogger;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogConsole
{
    public class Worker : ConsoleWorkerBase
    {
        private readonly ConsoleBase _consoleBase;
        private readonly IHostApplicationLifetime _lifeTime;
        private readonly ILogger _logger;
        public Worker(IHostApplicationLifetime lifetime, IBaseProvider provider, ConsoleBase console)
        {
            _consoleBase = console;
            _lifeTime = lifetime;

            _logger = provider.GetItem<ILogger>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if(_consoleBase.CanStart())
            {
                await _consoleBase.Start(stoppingToken);
            }
            else
            {
                _lifeTime.StopApplication();
            }
        }
    }
}
