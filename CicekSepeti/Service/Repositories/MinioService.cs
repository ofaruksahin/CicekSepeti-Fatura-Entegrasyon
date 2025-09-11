using Core.Dtos.Response.File;
using Core.Interfaces;
using Core.Utilities;
using Core.Utilities.Result;
using System.Threading.Tasks;
using System.IO;

namespace Service.Repositories
{
    public class MinioService : IMinioService
    {
        private ConnectionInfo _connectionInfo = ConnectionInfo.Instance;

        public async Task<bool> Upload(string fileName, string filePath, string contentType)
        {
            var source = filePath;
            var destination = System.IO.Path.Combine(_connectionInfo.LocalFilePath, fileName);  
            File.Copy(source, destination);

            return true;
        }

        public async Task<IDataResult<DownloadResponse>> GetLink(string id)
        {
            return new SuccessDataResult<DownloadResponse>(new DownloadResponse()
            {
                Url = $"{_connectionInfo.CdnUrl}/{id}.zip"
            });
        }
    }
}
