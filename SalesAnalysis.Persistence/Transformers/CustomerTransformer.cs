using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SalesAnalysis.Domain.Entities.Api;
using SalesAnalysis.Domain.Entities.Csv;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Transformers
{
    public class CustomerTransformer : ITransformer<CustomerCsv, Customer>, ITransformer<CustomerApiResponse, Customer>
    {
        public Task<IEnumerable<Customer>> TransformAsync(IEnumerable<CustomerCsv> source, CancellationToken cancellationToken = default)
        {
            var result = source?.Select(csv => new Customer
            {
                FirstName = csv.FirstName?.Trim() ?? string.Empty,
                LastName = csv.LastName?.Trim() ?? string.Empty,
                Email = csv.Email?.Trim() ?? string.Empty,
                Phone = csv.Phone?.Trim() ?? string.Empty,
                City = csv.City?.Trim() ?? string.Empty,
                Country = csv.Country?.Trim() ?? string.Empty
            }) ?? Enumerable.Empty<Customer>();

            return Task.FromResult(result);
        }

        Task<IEnumerable<Customer>> ITransformer<CustomerApiResponse, Customer>.TransformAsync(IEnumerable<CustomerApiResponse> source, CancellationToken cancellationToken)
        {
            var result = source?.Select(api => new Customer
            {
                FirstName = api.FirstName?.Trim() ?? string.Empty,
                LastName = api.LastName?.Trim() ?? string.Empty,
                Email = api.Email?.Trim() ?? string.Empty,
                Phone = api.Phone?.Trim() ?? string.Empty,
                City = api.City?.Trim() ?? string.Empty,
                Country = api.Country?.Trim() ?? string.Empty
            }) ?? Enumerable.Empty<Customer>();

            return Task.FromResult(result);
        }
    }
}
