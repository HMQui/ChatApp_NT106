using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatApp.Server.Services
{
    public class FirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(string credentialsPath, string bucketName)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                });
            }

            _storageClient = StorageClient.Create(GoogleCredential.FromFile(credentialsPath));
            _bucketName = bucketName;
        }

        public async Task<string> UploadFileAsync(byte[] fileData, string remoteFileName)
        {
            using var stream = new MemoryStream(fileData);
            await _storageClient.UploadObjectAsync(_bucketName, remoteFileName, null, stream);

            string publicUrl = $"https://storage.googleapis.com/{_bucketName}/{remoteFileName}";
            return $"{publicUrl}@filename@{Path.GetFileName(remoteFileName)}";
        }

    }
}
