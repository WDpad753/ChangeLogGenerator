using BaseConsole = BaseClass.ConsoleAppBase.ConsoleBase;
using BaseClass.ConsoleAppBase.BaseWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogConsole
{
    public class Worker : ConsoleWorkerBase
    {
        private readonly BaseConsole _consoleBase;

        public Worker(BaseConsole console)
        {
            _consoleBase = console;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
