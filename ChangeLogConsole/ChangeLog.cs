using BaseClass.Base.Interface;
using BaseClass.Config;
using BaseClass.ConsoleAppBase;
using BaseClass.JSON;
using BaseLogger;
using ChangeLogConsole.Base;
using ChangeLogCoreLibrary.APIRepositories.Client;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Timer = System.Timers.Timer;

namespace ChangeLogConsole
{
    internal class ChangeLog : ConsoleBase
    {
        private readonly IBaseProvider _provider;
        private readonly IBaseSettings _settings;
        private readonly ILogger logger;
        private ConfigHandler _configHandler;
        private ChangeLogBase<DBNull> _clb;
        private JSONFileHandler jsonHandler;
        private IAPIRepo<DBNull> _repo;
        public CLGConfig _config;
        private Timer _runTimer;

        private int? _delay;
        private int mainRun = 0;

        public ChangeLog(IBaseProvider provider) : base(provider)
        {
            _provider = provider;

            _configHandler = provider.GetItem<ConfigHandler>();
            _settings = provider.GetItem<IBaseSettings>();
            logger = provider.GetItem<ILogger>();
            _config = provider.GetItem<CLGConfig>();
            jsonHandler = provider.GetItem<JSONFileHandler>();
        }

        async void Run(object? sender, EventArgs e)
        {
            try
            {
                await ExecuteTasks();

                if (Volatile.Read(ref mainRun) == 0)
                {
                    logger.LogBase($"Service tasks completed. Waiting for {_delay} seconds or less before the next cycle...");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
            }
        }

        private async Task ExecuteTasks()
        {
            if (Interlocked.CompareExchange(ref mainRun, 1, 0) != 0)
            {
                logger.LogDebug($"Maintenance Poll is already running. Will not create a new thread.");
                return;
            }

            string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;
            _config.ConfigFilePath = _settings.ConfigPath;

            // Setting up and running ChangeLogConsole for Creating/Appending ChangeLog:
            if (_config == null)
            {
                logger.LogError($@"Error Message: Unable to find the path to save Commit File.");
                return;
            }

            string commitmessagesfilename = "CommitsChangeLog.txt";
            string targetcommitjsonpath = @$"{currentDirectory2}JsonFiles\";
            string targetcommitjsonfile = "PrevMap.json";
            string backuptargetcommitjsonpath = $@"{Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName}\JsonFiles\";

            string? repoProject = _configHandler.ReadInfo("Project", "changelogSettings");
            string? repoName = _configHandler.ReadInfo("RepositoryName", "changelogSettings");

            _config.logfilepath = Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName;
            _config.jsonpath = targetcommitjsonpath;
            _config.jsonfilename = targetcommitjsonfile;
            _config.backupjsonpath = backuptargetcommitjsonpath;
            _config.logfilename = commitmessagesfilename;
            _settings.FilePath = _config.logfilepath;

            string? tarRepo = _configHandler.ReadInfo("Repo", "changelogSettings");
            RepoMode mode;

            if (tarRepo == null)
            {
                logger.LogError($@"Error Message: Value for the Target Repo is empty.");
                return;
            }
            else
            {
                if (tarRepo.ToLower().Equals("GitHub".ToLower()))
                {
                    mode = RepoMode.GitHub;
                    _config.runType = "GitHub";
                }
                else if (tarRepo.ToLower().Equals("AzureDevOps".ToLower()))
                {
                    mode = RepoMode.AzureDevOps;
                    _config.runType = "AzureDevOps";
                }
                else
                {
                    logger.LogError($@"Error Message: Selected Repo Mode can not be found.");
                    return;
                }
            }

            _repo = APIFactory<DBNull>.GetAPIRepo(mode, _config, jsonHandler, _configHandler, logger);

            var clientProvider = new ClientProvider<DBNull>(logger, _config);
            clientProvider.clientBase = _config.runType;
            clientProvider.appName = _configHandler.ReadInfo("RepositoryName", "changelogSettings");

            _clb = new(_provider);

            try
            {
                _clb.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                logger.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
            }
        }

        public override bool CanStart()
        {
            try
            {
                _delay = _configHandler.ReadInfo("Poll", "changelogSettings") == "" ? null : int.Parse(_configHandler.ReadInfo("Poll", "changelogSettings"));

                if (_delay != null)
                {
                    int DelayInt = _delay.Value;
                    _runTimer = CreateTimer(DelayInt * 1000, Run);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override Task StartupTasks()
        {
            _runTimer.Start();

            Run(this, EventArgs.Empty);

            return Task.CompletedTask;
        }
    }
}
