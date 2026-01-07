using System;

namespace Intervu.Application.DTOs.Admin
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string LogoPath { get; set; }
    }
}
