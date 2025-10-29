using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;

namespace Intervu.Application.UseCases.Authentication
{
    public class RegisterUseCase : IRegisterUseCase
    {
        public Task<bool> ExecuteAsync(RegisterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
