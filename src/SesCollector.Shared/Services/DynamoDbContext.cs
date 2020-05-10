using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using SesCollector.Shared.Models;

namespace SesCollector.Shared.Services
{
    public interface IDynamoContext
    {
        Task<TModel> GetByIdAsync<TModel>(string id) where TModel : class, new();
        Task<IEnumerable<TModel>> GetAsync<TModel>(IEnumerable<ScanCondition> conditions = null) where TModel : class, new();

        Task SaveAsync<TModel>(TModel model) where TModel : class, new();

        Task DeleteAsync<TModel>(TModel model) where TModel : class, new();
    }

    public class DynamoDbContext : IDynamoContext
    {
        private readonly IAmazonDynamoDB _ddbClient;
        private readonly IDynamoDBContext _dbContext;

        public DynamoDbContext(IAmazonDynamoDB ddbClient)
        {
            _ddbClient = ddbClient;

            var ddbConfig = new Amazon.DynamoDBv2.DataModel.DynamoDBContextConfig
            {
                Conversion = DynamoDBEntryConversion.V2
            };
            _dbContext = new DynamoDBContext(ddbClient, ddbConfig);

            Initialize();
        }

        private void Initialize()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(SesEvent)] = 
                new Amazon.Util.TypeMapping(typeof(SesEvent), Environment.GetEnvironmentVariable("SesEventTableName"));
        }

        public async Task<TModel> GetByIdAsync<TModel>(string id) where TModel : class, new()
        {
            return await _dbContext.LoadAsync<TModel>(id);
        }

        public async Task<IEnumerable<TModel>> GetAsync<TModel>(IEnumerable<ScanCondition> conditions = null) where TModel : class, new()
        {            
            var search = _dbContext.ScanAsync<TModel>(conditions);
            var page = await search.GetNextSetAsync();

            return page;
        }

        public async Task SaveAsync<TModel>(TModel model) where TModel : class, new()
        {
            var table = Environment.GetEnvironmentVariable("SesEventTableName");
            await _dbContext.SaveAsync(model);
        }

        public async Task DeleteAsync<TModel>(TModel model) where TModel : class, new()
        {
            await _dbContext.DeleteAsync(model);
        }
    }
}