using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices.FirebaseStorageService
{
    public class FirebaseConfig
    {
        public string StorageBucket { get; set; } = string.Empty;
        public string CredentialPath { get; set; } = string.Empty;
    }
}
