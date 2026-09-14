using Newtonsoft.Json;
using System.Text;

namespace Karmak.Integrations.Volvo.Inbox.Validators.Blob
{
    public class BlobUploadRequest<TEntity>
    {
        public BlobUploadRequest(TEntity contents)
        {
            Contents = contents;
        }

        public TEntity Contents { get; }

        public string? BlobName { get; set; }

        public string? OriginalFileName { get; set; }

        public string? FileType { get; set; }

        public byte[] ContentBuffer => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Contents));
    }
}