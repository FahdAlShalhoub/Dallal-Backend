using System.IO;
using System.Threading.Tasks;
using Dallal.Documents.Dtos;

namespace Dallal.Documents;

public interface IDocumentsAppService
{
    Task<PresignedUrlDto> GetPresignedUrl(string fileName);
}
