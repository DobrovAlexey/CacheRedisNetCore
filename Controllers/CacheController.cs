using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace CacheRedisNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {
        private IRedisCacheClient _redisCacheClient;
        public CacheController(IRedisCacheClient redisCacheClient)
        {
            _redisCacheClient = redisCacheClient;
        }

        //[HttpGet]
        //[Route("Set")]
        //public async Task<bool> Set()
        //{
        //    var product = new Product()
        //    {
        //        Id = 1,
        //        Name = "Book",
        //        Price = 250
        //    };

        //    var serializeObject = JsonConvert.SerializeObject(product);

        //    byte[] data = Encoding.UTF8.GetBytes(serializeObject);

        //    await _redisCacheClient.SetAsync("product", data, new DistributedCacheEntryOptions()
        //    {
        //        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2)
        //    });

        //    return true;
        //}


        //[HttpGet]
        //[Route("Get")]
        //public async Task<Product> Get()
        //{
        //    var timer = Stopwatch.StartNew();

        //    var product = await _redisCacheClient.GetAsync("product");

        //    timer.Stop();
        //    Debug.WriteLine($"Get: {timer.Elapsed.Seconds} seconds, {timer.Elapsed.Milliseconds} milliseconds.");

        //    if (product == null)
        //    {
        //        return new Product();
        //    }

        //    var bytesAsString = Encoding.UTF8.GetString(product);
        //    var deserializeObject = JsonConvert.DeserializeObject<Product>(bytesAsString);

        //    return deserializeObject;
        //}

        //[HttpGet]
        //[Route("HashSet")]
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

        [HttpGet]
        [Route("Index")]
        public async Task<bool> Index()
        {
            var product = new Product()
            {
                Id = 1,
                Name = "hand sanitizer",
                Price = 100
            };

            await _redisCacheClient.Db1.AddAsync("Product", product, DateTimeOffset.Now.AddMinutes(10))
                .ConfigureAwait(false);

            return true;
        }


        //[HttpGet]
        //[Route("GetProductById/{id}")]
        //public async Task<Product> GetProductById(int id)
        //{
        //    // Define a unique key for this method and its parameters.
        //    //var tableKey = "GetProductById";
        //    var key = $"Product:{id}";

        //    // Try to get the entity from the cache.
        //    var json = await _distributedCache.GetAsync(key).ConfigureAwait(false);

        //    var bytesAsString = json == null ? string.Empty : Encoding.UTF8.GetString(json);
        //    var value = string.IsNullOrWhiteSpace(bytesAsString)
        //        ? default(Product)
        //        : JsonConvert.DeserializeObject<Product>(bytesAsString);

        //    if (value == null) // Cache miss
        //    {
        //        // If there's a cache miss, get the entity from the original store and cache it.
        //        // Code has been omitted because it is data store dependent.
        //        value = new Product
        //        {
        //            Id = 3,
        //            Name = "New Book",
        //            Price = 550
        //        };

        //        // Avoid caching a null value.
        //        if (value != null)
        //        {
        //            // Put the item in the cache with a custom expiration time that
        //            // depends on how critical it is to have stale data.

        //            var serializeObject = JsonConvert.SerializeObject(value);
        //            byte[] data = Encoding.UTF8.GetBytes(serializeObject);

        //            await _distributedCache.SetAsync(key, data, new DistributedCacheEntryOptions
        //            {
        //                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        //            }).ConfigureAwait(false);

        //        }
        //    }

        //    return value;
        //}



        //public async Task<MyEntity> GetMyEntityAsync(int id)
        //{
        //    // Define a unique key for this method and its parameters.
        //    var key = $"MyEntity:{id}";
        //    var cache = Connection.GetDatabase();

        //    // Try to get the entity from the cache.
        //    var json = await cache.StringGetAsync(key).ConfigureAwait(false);
        //    var value = string.IsNullOrWhiteSpace(json)
        //        ? default(MyEntity)
        //        : JsonConvert.DeserializeObject<MyEntity>(json);

        //    if (value == null) // Cache miss
        //    {
        //        // If there's a cache miss, get the entity from the original store and cache it.
        //        // Code has been omitted because it is data store dependent.
        //        value = ...;

        //        // Avoid caching a null value.
        //        if (value != null)
        //        {
        //            // Put the item in the cache with a custom expiration time that
        //            // depends on how critical it is to have stale data.
        //            await cache.StringSetAsync(key, JsonConvert.SerializeObject(value)).ConfigureAwait(false);
        //            await cache.KeyExpireAsync(key, TimeSpan.FromMinutes(DefaultExpirationTimeInMinutes)).ConfigureAwait(false);
        //        }
        //    }

        //    return value;
        //}
    }
}
