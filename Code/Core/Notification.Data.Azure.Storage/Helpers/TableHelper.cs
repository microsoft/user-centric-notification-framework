// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Data.Tables;
using Azure;
using Notification.Data.Azure.Storage.Interface;

namespace Notification.Data.Azure.Storage.Helpers
{
    /// <summary>
    /// The TableHelper class
    /// </summary>
    public class TableHelper : ITableHelper
    {
        private static readonly Regex FieldNameRegex = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly HashSet<string> AllowedComparisonOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eq", "ne", "gt", "ge", "lt", "le"
        };
        private static readonly HashSet<string> AllowedLogicalOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "and", "or"
        };

        private readonly string _azureStorageAccountName;
        private readonly TokenCredential _tokenCredential;

        public TableHelper(string accountName, TokenCredential tokenCredential)
        {
            _azureStorageAccountName = accountName;
            _tokenCredential = tokenCredential;
        }

        /// <summary>
        /// Get the table reference
        /// </summary>
        /// <returns></returns>
        private TableClient CreateTableClient(string tableName)
        {
            var tableClient = new TableClient(new Uri($"https://" + _azureStorageAccountName + ".table.core.windows.net/"),
                                                tableName,
                                                _tokenCredential);
            tableClient.CreateIfNotExists();
            return tableClient;
        }

        /// <summary>
        /// Get Table entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public IEnumerable<T> GetTableEntity<T>(string TableName) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            Pageable<T> queryResults = tableClient.Query<T>();
            return queryResults.AsEnumerable();
        }

        /// <summary>
        /// Get Table Entity by RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public T GetTableEntityByRowKey<T>(string TableName, string rowKey) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.Query<T>(entity => entity.RowKey == rowKey).FirstOrDefault();
        }

        /// <summary>
        /// Get table entity by PartitionKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <returns></returns>
        public T GetTableEntityByPartitionKey<T>(string TableName, string PartitionKey) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.Query<T>(entity => entity.PartitionKey == PartitionKey).FirstOrDefault();
        }

        /// <summary>
        /// Get table entity list by RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="RowKey"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByRowKey<T>(string TableName, string RowKey) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.Query<T>(entity => entity.RowKey == RowKey).ToList();
        }

        /// <summary>
        /// Get table entity list by partitionKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByPartitionKey<T>(string TableName, string PartitionKey) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.Query<T>(entity => entity.PartitionKey == PartitionKey).ToList();
        }

        /// <summary>
        /// Get table entity by field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public T GetTableEntityByfield<T>(string TableName, string fieldName, string fieldValue) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            string fieldNameToken = GetODataIdentifierToken(fieldName);
            FormattableString queryFormattableString = FormattableStringFactory.Create($"{fieldNameToken} eq {{0}}", fieldValue);
            string filter = TableClient.CreateQueryFilter(queryFormattableString);
            Pageable<T> queryResultsFilter = tableClient.Query<T>(filter: filter);
            return queryResultsFilter.FirstOrDefault();
        }

        /// <summary>
        /// Get table entity list by field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByfield<T>(string TableName, string fieldName, string fieldValue) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            string fieldNameToken = GetODataIdentifierToken(fieldName);
            FormattableString queryFormattableString = FormattableStringFactory.Create($"{fieldNameToken} eq {{0}}", fieldValue);
            string filter = TableClient.CreateQueryFilter(queryFormattableString);
            Pageable<T> queryResultsFilter = tableClient.Query<T>(filter: filter);
            return queryResultsFilter?.ToList();
        }

        /// <summary>
        /// Get table entity by partitionKey and RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <param name="RowKey"></param>
        /// <returns></returns>
        public T GetTableEntityByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.GetEntity<T>(PartitionKey, RowKey).Value;
        }

        /// <summary>
        /// Get table entity list by partitionKey and RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <param name="RowKey"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.Query<T>(entity => entity.PartitionKey == PartitionKey && entity.RowKey == RowKey)?.ToList();
        }

        /// <summary>
        /// Insert or Replace table entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entity"></param>
        /// <param name="caseConstraint"></param>
        /// <returns></returns>
        public async Task<bool> InsertOrReplace<T>(string TableName, T entity, bool caseConstraint = false) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            entity.Timestamp = entity.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : entity.Timestamp;
            var response = await tableClient.UpsertEntityAsync(entity);
            return !response.IsError;
        }

        /// <summary>
        /// Insert or Replace table entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entities"></param>
        /// <param name="caseConstraint"></param>
        /// <returns></returns>
        public async Task InsertOrReplaceRows<T>(string TableName, List<T> entities, bool caseConstraint = false) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            var tasks = new List<Task>();
            if (entities != null)
            {
                foreach (var rowsByPartitionKeyGroup in entities.GroupBy(p => p.PartitionKey))
                {
                    List<TableTransactionAction> upsertEntitiesBatch = new List<TableTransactionAction>();

                    foreach (var row in rowsByPartitionKeyGroup)
                    {
                        row.Timestamp = row.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : row.Timestamp;
                        if (caseConstraint)
                        {
                            row.PartitionKey = row.PartitionKey.ToLowerInvariant();
                        }
                        upsertEntitiesBatch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, row));
                    }

                    // Submit for execution
                    var task = tableClient.SubmitTransactionAsync(upsertEntitiesBatch);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Replaces a single entity in the Azure Table Storage.
        /// </summary>
        /// <param name="tableName">Name of storage table</param>
        /// <param name="row">TElement row</param>
        /// <param name="caseConstraint">bool true if need to set row.PartitionKEy to LowerInvariant. default false</param>
        /// <param name="isEncryptionEnabled">bool true if Encryption to be enabled for the operation</param>
        /// <returns>TableResult</returns>
        public async Task<bool> ReplaceRow<T>(string TableName, T entity, bool caseConstraint = false) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            entity.Timestamp = entity.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : entity.Timestamp;
            if (caseConstraint)
                entity.PartitionKey = entity.PartitionKey.ToLowerInvariant();
            var response = await tableClient.UpdateEntityAsync(entity, ETag.All);
            return !response.IsError;
        }

        /// <summary>
        /// Insert a single entity in the Azure Table Storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Insert<T>(string TableName, T entity) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            entity.Timestamp = entity.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : entity.Timestamp;
            var response = await tableClient.AddEntityAsync(entity);
            return !response.IsError;
        }

        /// <summary>
        /// Get table entities by partitionKey and field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public List<T> GetTableEntityByPartitionKeyAndField<T>(string TableName, string PartitionKey, string fieldName, string fieldValue) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            string partitionKeyToken = GetODataIdentifierToken(nameof(ITableEntity.PartitionKey));
            string fieldNameToken = GetODataIdentifierToken(fieldName);
            FormattableString queryFormattableString = FormattableStringFactory.Create($"{partitionKeyToken} eq {{0}} and {fieldNameToken} eq {{1}}", PartitionKey, fieldValue);
            string filter = TableClient.CreateQueryFilter(queryFormattableString);
            Pageable<T> queryResultsFilter = tableClient.Query<T>(filter: filter);
            return queryResultsFilter?.ToList();
        }

        /// <summary>
        /// Get collection of entities by table query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<T> GetDataCollectionByTableQuery<T>(string TableName, FormattableString query) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            return tableClient.Query<T>(filter: TableClient.CreateQueryFilter(query))?.ToList();
        }

        /// <summary>
        /// Get collection of entities by table query segmented
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<T> GetDataCollectionByTableQuerySegmented<T>(string TableName, FormattableString query) where T : class, ITableEntity, new()
        {
            List<T> result = new List<T>();
            TableClient tableClient = CreateTableClient(TableName);
            string continuationToken = null;
            string filter = TableClient.CreateQueryFilter(query);
            do
            {
                var responseList = tableClient.Query<T>(filter: filter);
                foreach (var response in responseList.AsPages())
                {
                    continuationToken = response.ContinuationToken;
                    result.AddRange(response.Values);
                }
            } while (continuationToken != null);
            return result;
        }

        /// <summary>
        /// Get collection of entities by columns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="columnOne"></param>
        /// <param name="columnOneQComparison"></param>
        /// <param name="tableOperator"></param>
        /// <param name="columnTwo"></param>
        /// <param name="columnTwoQComparison"></param>
        /// <returns></returns>
        public List<T> GetDataCollectionByColumns<T>(string TableName, KeyValuePair<string, string> columnOne, string columnOneQComparison, string tableOperator, KeyValuePair<string, string> columnTwo, string columnTwoQComparison) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            string leftFieldToken = GetODataIdentifierToken(columnOne.Key.ToString(CultureInfo.InvariantCulture));
            string rightFieldToken = GetODataIdentifierToken(columnTwo.Key.ToString(CultureInfo.InvariantCulture));
            string leftComparison = GetComparisonOperator(columnOneQComparison);
            string rightComparison = GetComparisonOperator(columnTwoQComparison);
            string logicalOperator = GetLogicalOperator(tableOperator);

            FormattableString queryFormattableString = FormattableStringFactory.Create($"{leftFieldToken} {leftComparison} {{0}} {logicalOperator} {rightFieldToken} {rightComparison} {{1}}",
                columnOne.Value.ToString(CultureInfo.InvariantCulture),
                columnTwo.Value.ToString(CultureInfo.InvariantCulture));

            var query = tableClient.Query<T>(filter: TableClient.CreateQueryFilter(queryFormattableString));
            return query.ToList();
        }

        /// <summary>
        /// Delete azure table storage entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRow<T>(string TableName, T Entity) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            Entity.ETag = ETag.All;
            var response = await tableClient.DeleteEntityAsync(Entity.PartitionKey, Entity.RowKey, Entity.ETag);
            return !response.IsError;
        }

        /// <summary>
        /// Delete azure table storage entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task DeleteRowsAsync<T>(string tableName, List<T> entities) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(tableName);
            var tasks = new List<Task>();
            if (entities != null)
            {
                foreach (var rowsByPartitionKeyGroup in entities.GroupBy(p => p.PartitionKey))
                {
                    List<TableTransactionAction> deleteEntitiesBatch = new List<TableTransactionAction>();

                    foreach (var row in rowsByPartitionKeyGroup)
                    {
                        deleteEntitiesBatch.Add(new TableTransactionAction(TableTransactionActionType.Delete, row));
                    }

                    // Submit for execution
                    var task = tableClient.SubmitTransactionAsync(deleteEntitiesBatch);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Merge entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName">Name of Storage table</param>
        /// <param name="Entity">TElement Row</param>
        /// <returns></returns>
        public async Task<bool> Merge<T>(string TableName, T Entity) where T : class, ITableEntity, new()
        {
            TableClient tableClient = CreateTableClient(TableName);
            Entity.ETag = ETag.All;
            var response = await tableClient.UpdateEntityAsync(Entity, ETag.All, TableUpdateMode.Merge);
            return !response.IsError;
        }

        /// <summary>
        /// Validates and returns the OData identifier token for the specified field name.
        /// </summary>
        /// <param name="fieldName">The field name to validate.</param>
        /// <returns>The OData identifier token for the specified field name.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is null, whitespace, or not a valid field name.</exception>
        private static string GetODataIdentifierToken(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || !FieldNameRegex.IsMatch(fieldName))
            {
                throw new ArgumentException("Invalid field name for query filter.", nameof(fieldName));
            }

            return $"[{fieldName}]";
        }

        /// <summary>
        /// Validates and returns the specified comparison operator.
        /// </summary>
        /// <param name="comparisonOperator">The comparison operator to validate.</param>
        /// <returns>The validated comparison operator.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="comparisonOperator"/> is null, whitespace, or not in the list of allowed comparison operators.</exception>
        private static string GetComparisonOperator(string comparisonOperator)
        {
            if (string.IsNullOrWhiteSpace(comparisonOperator) || !AllowedComparisonOperators.Contains(comparisonOperator))
            {
                throw new ArgumentException("Invalid comparison operator for query filter.", nameof(comparisonOperator));
            }

            return comparisonOperator;
        }

        /// <summary>
        /// Validates and returns the specified logical operator.
        /// </summary>
        /// <param name="tableOperator">The logical operator to validate.</param>
        /// <returns>The validated logical operator.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableOperator"/> is null, whitespace, or not in the list of allowed logical
        /// operators.</exception>
        private static string GetLogicalOperator(string tableOperator)
        {
            if (string.IsNullOrWhiteSpace(tableOperator) || !AllowedLogicalOperators.Contains(tableOperator))
            {
                throw new ArgumentException("Invalid logical operator for query filter.", nameof(tableOperator));
            }

            return tableOperator;
        }
    }
}