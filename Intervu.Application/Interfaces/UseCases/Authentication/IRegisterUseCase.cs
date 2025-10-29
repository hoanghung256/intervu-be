using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.User;

namespace Intervu.Application.Interfaces.UseCases.Authentication
{
    public interface IRegisterUseCase
    {
        Task<bool> ExecuteAsync(RegisterRequest request);
    }
}
