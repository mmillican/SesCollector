﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using SesCollector.Shared.Models;
using SesCollector.Shared.Services;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SesCollector.Api
{
    public class Functions
    {
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "SesEventTableName";

        public const string ID_QUERY_STRING_NAME = "Id";

        private readonly IDynamoContext _dbContext;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            
            _dbContext = new DynamoDbContext(new AmazonDynamoDBClient());
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            _dbContext = new DynamoDbContext(ddbClient);
        }

        /// <summary>
        /// A Lambda function that returns back a page worth of SesEvent posts.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of SesEvents</returns>
        public async Task<APIGatewayProxyResponse> GetSesEventsAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Getting SesEvents");
            var allowedCorsOrigin = Environment.GetEnvironmentVariable("AllowedCorsOrigin");

            var queryConditions = new List<ScanCondition>();

            string fromEmail = null;
            if (request?.QueryStringParameters?.TryGetValue("fromEmail", out fromEmail) ?? false)
            {
                if (!string.IsNullOrEmpty(fromEmail))
                {
                    queryConditions.Add(new ScanCondition(nameof(SesEvent.FromAddress), ScanOperator.Equal, fromEmail));
                }
            }
            string toEmail = null;
            if (request?.QueryStringParameters?.TryGetValue("toEmail", out toEmail) ?? false)
            {
                if (!string.IsNullOrEmpty(toEmail))
                {
                    queryConditions.Add(new ScanCondition(nameof(SesEvent.Recipients), ScanOperator.Contains, toEmail));
                }
            }

            string queryFromDate = null, queryToDate = null;
            if (request?.QueryStringParameters?.TryGetValue("fromDate", out queryFromDate) ?? false)
            {
                if (DateTime.TryParse(queryFromDate, out var fromDate))
                {
                    queryConditions.Add(new ScanCondition(nameof(SesEvent.Timestamp), ScanOperator.GreaterThanOrEqual, fromDate));
                }
            }
            if (request?.QueryStringParameters?.TryGetValue("toDate", out queryToDate) ?? false)
            {
                if (DateTime.TryParse(queryToDate, out var toDate))
                {
                    queryConditions.Add(new ScanCondition(nameof(SesEvent.Timestamp), ScanOperator.LessThanOrEqual, toDate));
                }
            }

            var events = await _dbContext.GetAsync<SesEvent>(queryConditions);

            context.Logger.LogLine($"Found {events.Count()} SesEvents");

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(events),
                Headers = new Dictionary<string, string> 
                { 
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", allowedCorsOrigin }
                }
            };

            return response;
        }

        // /// <summary>
        // /// A Lambda function that returns the SesEvent identified by SesEventId
        // /// </summary>
        // /// <param name="request"></param>
        // /// <returns></returns>
        // public async Task<APIGatewayProxyResponse> GetSesEventAsync(APIGatewayProxyRequest request, ILambdaContext context)
        // {
        //     string SesEventId = null;
        //     if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
        //         SesEventId = request.PathParameters[ID_QUERY_STRING_NAME];
        //     else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
        //         SesEventId = request.QueryStringParameters[ID_QUERY_STRING_NAME];

        //     if (string.IsNullOrEmpty(SesEventId))
        //     {
        //         return new APIGatewayProxyResponse
        //         {
        //             StatusCode = (int)HttpStatusCode.BadRequest,
        //             Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
        //         };
        //     }

        //     context.Logger.LogLine($"Getting SesEvent {SesEventId}");
        //     var SesEvent = await DDBContext.LoadAsync<SesEvent>(SesEventId);
        //     context.Logger.LogLine($"Found SesEvent: {SesEvent != null}");

        //     if (SesEvent == null)
        //     {
        //         return new APIGatewayProxyResponse
        //         {
        //             StatusCode = (int)HttpStatusCode.NotFound
        //         };
        //     }

        //     var response = new APIGatewayProxyResponse
        //     {
        //         StatusCode = (int)HttpStatusCode.OK,
        //         Body = JsonConvert.SerializeObject(SesEvent),
        //         Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        //     };
        //     return response;
        // }

        // /// <summary>
        // /// A Lambda function that adds a SesEvent post.
        // /// </summary>
        // /// <param name="request"></param>
        // /// <returns></returns>
        // public async Task<APIGatewayProxyResponse> AddSesEventAsync(APIGatewayProxyRequest request, ILambdaContext context)
        // {
        //     var SesEvent = JsonConvert.DeserializeObject<SesEvent>(request?.Body);
        //     SesEvent.Id = Guid.NewGuid().ToString();
        //     // SesEvent.CreatedTimestamp = DateTime.Now;

        //     context.Logger.LogLine($"Saving SesEvent with id {SesEvent.Id}");
        //     await DDBContext.SaveAsync<SesEvent>(SesEvent);

        //     var response = new APIGatewayProxyResponse
        //     {
        //         StatusCode = (int)HttpStatusCode.OK,
        //         Body = SesEvent.Id.ToString(),
        //         Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        //     };
        //     return response;
        // }

        // /// <summary>
        // /// A Lambda function that removes a SesEvent post from the DynamoDB table.
        // /// </summary>
        // /// <param name="request"></param>
        // public async Task<APIGatewayProxyResponse> RemoveSesEventAsync(APIGatewayProxyRequest request, ILambdaContext context)
        // {
        //     string SesEventId = null;
        //     if (request.PathParameters != null && request.PathParameters.ContainsKey(ID_QUERY_STRING_NAME))
        //         SesEventId = request.PathParameters[ID_QUERY_STRING_NAME];
        //     else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ID_QUERY_STRING_NAME))
        //         SesEventId = request.QueryStringParameters[ID_QUERY_STRING_NAME];

        //     if (string.IsNullOrEmpty(SesEventId))
        //     {
        //         return new APIGatewayProxyResponse
        //         {
        //             StatusCode = (int)HttpStatusCode.BadRequest,
        //             Body = $"Missing required parameter {ID_QUERY_STRING_NAME}"
        //         };
        //     }

        //     context.Logger.LogLine($"Deleting SesEvent with id {SesEventId}");
        //     await this.DDBContext.DeleteAsync<SesEvent>(SesEventId);

        //     return new APIGatewayProxyResponse
        //     {
        //         StatusCode = (int)HttpStatusCode.OK
        //     };
        // }
    }
}
