using Com.Bateeq.Service.Pos.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocDataUtils
{
    public class SalesDocIHttpService : IHttpClientService
    {
        public static string Token;

        public Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
        {
            return Task.Run(() => new HttpResponseMessage());
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            

            return Task.Run(() => new HttpResponseMessage());
        }

        public Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            return Task.Run(() => new HttpResponseMessage());
        }

        public Task<HttpResponseMessage> DeleteAsync(string url)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
        {
            throw new NotImplementedException();
        }
    }
}
