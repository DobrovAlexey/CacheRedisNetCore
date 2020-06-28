using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace CacheRedisNetCore.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CacheController : ControllerBase
    {
        private readonly IRedisCacheClient _redisCacheClient;

        public CacheController(IRedisCacheClient redisCacheClient)
        {
            _redisCacheClient = redisCacheClient;
        }

        public async Task<bool> AddProduct()
        {
            var product = new Product
            {
                Id = 1,
                Name = "hand sanitizer",
                Price = 100
            };

            await _redisCacheClient.Db1.AddAsync("Product", product, DateTimeOffset.Now.AddMinutes(10))
                .ConfigureAwait(false);

            return true;
        }

        public async Task<bool> AddProducts()
        {
            var values = new List<Tuple<string, Product>>
            {
                new Tuple<string, Product>("Product1", new Product
                {
                    Id = 1,
                    Name = "hand sanitizer 1",
                    Price = 100
                }),
                new Tuple<string, Product>("Product2", new Product
                {
                    Id = 2,
                    Name = "hand sanitizer 2",
                    Price = 200
                }),
                new Tuple<string, Product>("Product3", new Product
                {
                    Id = 3,
                    Name = "hand sanitizer 3",
                    Price = 300
                })
            };

            await _redisCacheClient.Db1.AddAllAsync(values, DateTimeOffset.Now.AddMinutes(30)).ConfigureAwait(false);

            return true;
        }

        public async Task<Product> GetProduct()
        {
            var productData = await _redisCacheClient.Db1.GetAsync<Product>("Product");
            return productData;
        }

        public async Task<IDictionary<string, Product>> GetProducts()
        {
            var allKeys = new List<string>
            {
                "Product1", "Product2", "Product3"
            };

            var listOfProducts = await _redisCacheClient.Db1.GetAllAsync<Product>(allKeys);
            return listOfProducts;
        }

        public async Task<bool> RemoveProduct()
        {
            var isRemoved = await _redisCacheClient.Db1.RemoveAsync("Product");
            return true;
        }

        public async Task<bool> RemoveProducts()
        {
            var allKeys = new List<string>
            {
                "Product1",
                "Product2",
                "Product3"
            };

            await _redisCacheClient.Db1.RemoveAllAsync(allKeys).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> ExistsProduct()
        {
            var isExists = await _redisCacheClient.Db1.ExistsAsync("Product");
            return isExists;
        }

        public async Task Search()
        {
            // If you want to search all keys that start with 'Product*'
            var listOfkeys1 = await _redisCacheClient.Db1.SearchKeysAsync("Product*");

            // If you want to search all keys that contain with '*Product*'
            var listOfkeys2 = await _redisCacheClient.Db1.SearchKeysAsync("*Product*");

            // If you want to search all keys that end with '*Product'
            var listOfkeys3 = await _redisCacheClient.Db1.SearchKeysAsync("*Product");
        }

        //public async Task HashSet()
        //{
        //    var hashKey = "hashKey";

        //    HashEntry[] redisBookHash = {
        //        new HashEntry("title", "Redis for .NET Developers"),
        //        new HashEntry("year", 2016),
        //        new HashEntry("author", "Taswar Bhatti")
        //    };

        //    _redisCacheClient.HashSet(hashKey, redisBookHash);

        //    if (_redisCacheClient.HashExists(hashKey, "year"))
        //    {
        //        var year = _redisCacheClient.HashGet(hashKey, "year"); //year is 2016
        //    }

        //    var allHash = _redisCacheClient.HashGetAll(hashKey);

        //    //get all the items
        //    foreach (var item in allHash)
        //    {
        //        //output 
        //        //key: title, value: Redis for .NET Developers
        //        //key: year, value: 2016
        //        //key: author, value: Taswar Bhatti
        //        Console.WriteLine(string.Format("key : {0}, value : {1}", item.Name, item.Value));
        //    }

        //    //get all the values
        //    var values = _distributedCache.HashValues(hashKey);

        //    foreach (var val in values)
        //    {
        //        Console.WriteLine(val); //result = Redis for .NET Developers, 2016, Taswar Bhatti
        //    }

        //    //get all the keys
        //    var keys = _distributedCache.HashKeys(hashKey);

        //    foreach (var k in keys)
        //    {
        //        Console.WriteLine(k); //result = title, year, author
        //    }

        //    var len = _distributedCache.HashLength(hashKey);  //result of len is 3

        //    if (_distributedCache.HashExists(hashKey, "year"))
        //    {
        //        var year = _distributedCache.HashIncrement(hashKey, "year", 1); //year now becomes 2017
        //        var year2 = _distributedCache.HashDecrement(hashKey, "year", 1.5); //year now becomes 2015.5
        //    }
        //}

        [Route("{id}")]
        public async Task<Product> GetProductById(int id)
        {
            // Define a unique key for this method and its parameters.
            var key = $"Product:{id}";

            // Try to get the entity from the cache.
            var value = await _redisCacheClient.Db1.GetAsync<Product>(key);

            if (value == null) // Cache miss
            {
                // If there's a cache miss, get the entity from the original store and cache it.
                // Code has been omitted because it is data store dependent.
                value = new Product
                {
                    Id = id,
                    Name = "New Book",
                    Price = 550
                };

                // Avoid caching a null value.
                if (value != null)
                {
                    // Put the item in the cache with a custom expiration time that
                    // depends on how critical it is to have stale data.

                    await _redisCacheClient.Db1.AddAsync(key, value, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                }
            }

            return value;
        }
    }
}