using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;

namespace ReviewService.Repositories
{
    public class MongoRepositoryBase<T> where T : IDocumentBase
    {
        public static MongoClient _client;
        private IMongoCollection<T> _collection;
        private IMongoDatabase _database;

        public IMongoCollection<T> Collection => _collection;
        public IMongoDatabase Database => _database;

        public MongoRepositoryBase(MongoClient client)
        {
            _database = client.GetDatabase("ReviewDB");
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }

        public static void Register()
        {

        }

        public virtual Task Create(T item)
        {
            if (string.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            return _collection.InsertOneAsync(item);
        }

        public virtual Task Delete(string id)
        {
            return _collection.FindOneAndDeleteAsync<T>(x => x.Id == id);
        }

        public virtual T Get(string id)
        {
            return _collection.Find<T>(x => x.Id == id).FirstOrDefault();
        }

        public virtual IQueryable<T> GetItems(Expression<Func<T, bool>> predicate)
        {
            return _collection.AsQueryable().Where(predicate);
        }

        public virtual Task Update(string id, T item)
        {
            return _collection.ReplaceOneAsync<T>(x => x.Id == id, item, new UpdateOptions { IsUpsert = true });
        }

        public virtual async Task BulkCreate(IEnumerable<T> data)
        {
            foreach (var item in data)
            {
                if (string.IsNullOrEmpty(item.Id))
                    item.Id = Guid.NewGuid().ToString();
            }

            //paginate
            int itemCount = 100;
            int pageCount = 0;

            var items = data.Skip(pageCount * itemCount).Take(itemCount);
            while (items.Count() > 0)
            {
                await this.Collection.InsertManyAsync(items);

                pageCount++;
                items = data.Skip(pageCount * itemCount).Take(itemCount);

                Thread.Sleep(1000);
            }
        }
    }
}

