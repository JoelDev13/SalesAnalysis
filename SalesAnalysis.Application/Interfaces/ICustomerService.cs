using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Application.DTOs;

namespace SalesAnalysis.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<int> RunEtlAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
