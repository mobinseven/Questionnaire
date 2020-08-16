﻿using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using Breeze.Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Services
{
    public class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> logger;

        private readonly DataService dataService;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            this.logger = logger;

            Configuration.Instance.QueryUriStyle = QueryUriStyle.JSON;
            Configuration.Instance.ProbeAssemblies(typeof(ApplicationUser).Assembly);

            dataService = new DataService(httpClient.BaseAddress + "api/data/", httpClient);
            EntityManager = new EntityManager(dataService);

            var clientNameSpace = typeof(ApplicationUser).Namespace;
            var dic = new Dictionary<string, string>();
            dic.Add("BlazorBoilerplate.Infrastructure.Storage.DataModels", clientNameSpace);
            dic.Add("Microsoft.AspNetCore.Identity", clientNameSpace);

            EntityManager.MetadataStore.NamingConvention = new NamingConvention().WithServerClientNamespaceMapping(dic);
        }

        public EntityManager EntityManager { get; }

        public void CancelChanges()
        {
            EntityManager.RejectChanges();
        }

        public async Task SaveChanges()
        {
            try
            {
                await EntityManager.SaveChanges();
            }
            catch (SaveException ex)
            {
                var msg = ex.EntityErrors.First().ErrorMessage;
                logger.LogWarning("SaveChanges: {0}", msg);
                throw new Exception(msg);
            }
            catch (Exception ex)
            {
                logger.LogError("SaveChanges: {0}", ex.GetBaseException().Message);
                throw;
            }
            finally
            {
                EntityManager.RejectChanges();
            }
        }

        public void AddEntity(IEntity entity)
        {
            EntityManager.AddEntity(entity);
        }

        private async Task<QueryResult<T>> GetItems<T>(string from,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            Expression<Func<T, object>> orderByDescending = null,
            int? take = null,
            int? skip = null)
        {
            try
            {
                var query = new EntityQuery<T>().InlineCount().From(from);

                if (predicate != null)
                    query = query.Where(predicate);

                if (orderBy != null)
                    query = query.OrderBy(orderBy);

                if (orderByDescending != null)
                    query = query.OrderByDescending(orderByDescending);

                if (take != null)
                    query = query.Take(take.Value);

                if (skip != null)
                    query = query.Skip(skip.Value);

                return (QueryResult<T>)await EntityManager.ExecuteQuery(query, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError("GetItems: {0}", ex.GetBaseException().Message);

                throw;
            }
        }

        public async Task<QueryResult<Todo>> GetToDos()
        {
            return await GetItems<Todo>(from: "Todos", orderByDescending: i => i.CreatedOn);
        }

        public async Task<QueryResult<DbLog>> GetLogs(Expression<Func<DbLog, bool>> predicate = null, int? take = null, int? skip = null)
        {
            return await GetItems("Logs", predicate, null, i => i.TimeStamp, take, skip);
        }

        public async Task<QueryResult<ApiLogItem>> GetApiLogs(Expression<Func<ApiLogItem, bool>> predicate = null, int? take = null, int? skip = null)
        {
            return await GetItems("ApiLogs", predicate, null, i => i.RequestTime, take, skip);
        }
        public async Task<UserProfile> GetUserProfile()
        {
            return (await EntityManager.ExecuteQuery(new EntityQuery<UserProfile>().From("UserProfile"), CancellationToken.None)).SingleOrDefault();
        }

        public async Task<QueryResult<TenantSetting>> GetTenantSettings()
        {
            return await GetItems<TenantSetting>(from: "TenantSettings", orderBy: i => i.Key);
        }
    }
}
