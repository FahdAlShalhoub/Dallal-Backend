using System.IO;
using System.Threading.Tasks;

namespace Dallal.Documents;

public interface IDocumentsAppService
{
    Task<string> GetPresignedUrl(string fileName);
}
