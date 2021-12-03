﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using c231_qrder.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Table = c231_qrder.Models.Table;

namespace c231_qrder.Services
{
    public class TableService : ServiceBase, ITableService
    {
        private readonly IAmazonDynamoDB dynamoDBClient;
        private readonly IDynamoDBContext context;
        private readonly IMapper mapper;

        public TableService(
            IAmazonDynamoDB dynamoDBClient,
            IMapper mapper
            )
        {
            this.dynamoDBClient = dynamoDBClient;
            context = new DynamoDBContext(this.dynamoDBClient);
            this.mapper = mapper;
        }

        public async Task<IEnumerable<TableDto>> GetAllByRestaurantIdAsync(string id)
        {
            if (!await IsPresent(id))
            {
                throw new DataException();
            }

            var config = new DynamoDBOperationConfig()
            {
                QueryFilter = new List<ScanCondition>()
                {
                    new ScanCondition("TableId", ScanOperator.BeginsWith, tableSortKeyPrefix)
                }
            };
            List<Table> allTables = await context.QueryAsync<Table>(id, config).GetRemainingAsync();

            var allTableDtos = new List<TableDto>();
            allTables.ForEach(t =>
            {
                var targetTableDto = mapper.Map<TableDto>(t);
                allTableDtos.Add(targetTableDto);
            });

            return allTableDtos;
        }

        public async Task AddAsync(string id, TableCreateDto tableCreateDto)
        {
            if (!await IsPresent(id))
            {
                throw new DataException();
            }

            // Dto mapping: TableCreateDto -> Table
            var newTable = mapper.Map<Table>(tableCreateDto);

            // set properties
            string guid = base.GetGuidAsStr();
            newTable.RestaurantId = id;
            newTable.TableId = tableSortKeyPrefix + guid;

            // check the name is duplicate
            if (await HasThisTableName(id, newTable.TableName))
            {
                throw new DuplicateNameException();
            }

            await context.SaveAsync(newTable);
        }

        public async Task<bool> IsPresent(string id)
        {
            // check there is the restaurant
            var restaurantService = new RestaurantService(dynamoDBClient, mapper);
            RestaurantDto restaurantDto = await restaurantService.GetByIdAsync(id);

            return (restaurantDto is not null);
        }

        public async Task<bool> IsPresent(string id, string tableId)
        {
            // check there is the restaurant
            if (!await IsPresent(id))
            {
                return false;
            }

            // check there is the table in the restaurant
            var tableDtos = new List<TableDto>();
            tableDtos.AddRange(await GetAllByRestaurantIdAsync(id));
            TableDto targetTableDto = tableDtos.Find(td => td.TableId == tableId);

            return (targetTableDto is not null);
        }

        public async Task<bool> HasThisTableName(string id, string tableName)
        {
            // check there is the restaurant
            if (!await IsPresent(id))
            {
                return false;
            }

            // check there is the table in the restaurant
            var config = new DynamoDBOperationConfig()
            {
                IndexName = "TableName-index",
                QueryFilter = new List<ScanCondition>()
                {
                    new ScanCondition("TableName", ScanOperator.Equal, tableName)
                }
            };
            List<Table> resultTables = await context.QueryAsync<Table>(id, config).GetRemainingAsync();

            return (resultTables.Count > 0);
        }
    }
}