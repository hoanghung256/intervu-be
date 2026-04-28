using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IPurgePineconeNamespace
    {
        Task ExecuteAsync(string @namespace);
    }
}
