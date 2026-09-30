namespace Timesheet.Infrastructure.Data;

using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Timesheet.Domain.Interfaces;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbService> _logger;

    public MongoDbService(IMongoDatabase database, ILogger<MongoDbService> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<T> GetByIdAsync<T>(string id) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var filter = Builders<T>.Filter.Eq("Id", id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving {typeof(T).Name} with id {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            return await collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving all {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<T>> GetAsync<T>(Func<T, bool> predicate) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var all = await collection.Find(_ => true).ToListAsync();
            return all.Where(predicate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error querying {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    public async Task<T> InsertAsync<T>(T entity) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            await collection.InsertOneAsync(entity);
            _logger.LogInformation($"Inserted {typeof(T).Name}");
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error inserting {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync<T>(string id, T entity) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var filter = Builders<T>.Filter.Eq("Id", id);
            await collection.ReplaceOneAsync(filter, entity);
            _logger.LogInformation($"Updated {typeof(T).Name} with id {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync<T>(string id) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var filter = Builders<T>.Filter.Eq("Id", id);
            await collection.DeleteOneAsync(filter);
            _logger.LogInformation($"Deleted {typeof(T).Name} with id {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    public async Task<long> CountAsync<T>(Func<T, bool> predicate) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var all = await collection.Find(_ => true).ToListAsync();
            return all.Count(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error counting {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }
}
