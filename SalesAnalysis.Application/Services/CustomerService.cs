using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Application.DTOs;
using SalesAnalysis.Application.Interfaces;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IEtlService _etlService;
        private readonly ICustomerReadRepository _customerRepository;

        public CustomerService(
            IEtlService etlService,
            ICustomerReadRepository customerRepository)
        {
            _etlService = etlService;
            _customerRepository = customerRepository;
        }

        public Task<int> RunEtlAsync(CancellationToken cancellationToken = default)
        {
            return _etlService.ExecuteAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var customers = await _customerRepository.GetAllAsync(cancellationToken);
            return customers.Select(MapToDto);
        }

        public async Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var customer = await _customerRepository.GetByEmailAsync(email, cancellationToken);
            return customer is null ? null : MapToDto(customer);
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                City = customer.City,
                Country = customer.Country
            };
        }
    }
}
