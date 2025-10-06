using BaseClass.API;
using BaseClass.API.Interface;
using BaseClass.Base.Interface;
using BaseClass.Helper;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


namespace ChangeLogCoreLibrary.APIRepositories.Client
{
    public class APIClient<TEntryPoint> : IDisposable where TEntryPoint : class
    {
        private readonly IBaseSettings? baseConfig;
        public int? timeOut {  get; set; }
        public string? PerAccTok { get; set; }

        private static HttpClient? _client = null;
        public static bool? clientCreated = false;
        private bool disposedValue;
        private readonly ILogger? _logWriter;
        private readonly ClientProvider<TEntryPoint>? _clientProvider;

        public APIClient(ILogger? Logger, ClientProvider<TEntryPoint>? clientProvider = null)
        {
            _logWriter = Logger;
            _clientProvider = clientProvider;
        }

        public async Task<T?> Get<T>(string? url = null) where T : class
        {
            try
            {
                if (_client == null)
                {
                    _client = _clientProvider?.CreateClient(null);
                }

                var client = _client;

                if (clientCreated == false)
                {
                    // Set personal access token in request headers of the baseurl:
                    if (PerAccTok != null)
                    {
                        if(_clientProvider.clientBase.Equals("AzureDevOps"))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{PerAccTok}")));
                        }
                        else if (_clientProvider.clientBase.Equals("GitHub"))
                        {
                            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{_clientProvider.appName}");
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PerAccTok);
                        }
                        else
                        {
                            throw new Exception("Requires Valid Client Base");
                        }

                        client.Timeout = TimeSpan.FromSeconds((double)timeOut);
                        clientCreated = true;
                    }
                    else
                    {
                        client.Timeout = TimeSpan.FromSeconds((double)timeOut);
                        clientCreated = true;
                    }
                }

                try
                {
                    // Initiate Get request for the API url:
                    string URL = url == null ? client.BaseAddress.ToString() : PathCombine.CombinePath(CombinationType.URL, client.BaseAddress.ToString(), url).TrimEnd('/');
                    Task<HttpResponseMessage> taskcol = client.GetAsync(URL);
                    Task.WaitAll(taskcol);
                    if (taskcol.IsFaulted)
                    {
                        Console.WriteLine(taskcol.Exception.ToString());
                        System.Diagnostics.Debug.WriteLine($@"Here is the Content of the Error Message: {taskcol.Exception.ToString()}");
                        _logWriter.LogError($"Error in acquiring response from url {client.BaseAddress}: {taskcol.Exception.ToString()}");
                    }
                    else
                    {
                        // Check if the request was successful:
                        HttpResponseMessage response = taskcol.Result;

                        // Check if the request was successful:
                        response.EnsureSuccessStatusCode();

                        // Get the response content from the request:
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Deserialize the JSON response:
                        T? responseObject = JsonConvert.DeserializeObject<T>(responseBody);

                        return responseObject;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                    System.Diagnostics.Debug.WriteLine($@"Here is the Content of the Error Message: {ex.ToString()}");
                    _logWriter.LogError("Error in De-Serializing the JSON Object: " + ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($@"Here is the Content of the Error Message: {ex.ToString()}");
                _logWriter.LogError("Error in De-Serializing the JSON Object: " + ex);
                return null;
            }
        }

        public static void ResetClient()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            clientCreated = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (_client != null && (bool)clientCreated)
                    {
                        _client.Dispose();
                        _client = null;
                        clientCreated = false;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue=true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        //~APIClient()
        //{
        //    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //    Dispose(disposing: false);
        //}

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
