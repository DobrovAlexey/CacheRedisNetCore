using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public async Task AddProducts()
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

            await _redisCacheClient.Db1.AddAllAsync(values, DateTimeOffset.Now.AddMinutes(30));
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

        public async Task AddMill()
        {
            var random = new Random();

            using var sr = new StreamReader(@"words.txt");
            var strings = (await sr.ReadToEndAsync()).Replace("\r", string.Empty).Split(new[] {'\n'}).ToList();

            for (var i = 0; i < 1000; i++)
            {
                Trace.WriteLine(i);

                var values = new List<Tuple<string, Product>>();

                for (var j = 0; j < 1000; j++)
                {
                    var entity = new Product
                    {
                        Id = random.Next(1, int.MaxValue - 1),
                        Name = string.Join(' ',
                            strings.GetRange(random.Next(1, strings.Count - 1001), random.Next(10, 1000))),
                        Price = random.Next(1, int.MaxValue)
                    };

                    var tuple = new Tuple<string, Product>($"GetProductEntity:{GetPropertyHashFromType(entity)}",
                        entity);

                    values.Add(tuple);
                }

                await _redisCacheClient.Db1.AddAllAsync(values, TimeSpan.FromDays(2));

                values.Clear();

                Trace.WriteLine("successfully add Products ");
            }
        }

        [HttpPost]
        public async Task<string> GetProductEntity(Product product)
        {
            var hashProp = GetPropertyHashFromType(product);
            var key = $"GetProductEntity:{hashProp}";

            var value = await _redisCacheClient.Db1.GetAsync<Product>(key);
            if (value != null) return "Объект находится в кэше.";

            await _redisCacheClient.Db1.AddAsync(key, product, TimeSpan.FromDays(2)).ConfigureAwait(false);
            return "Объект не найден в кэше.";
        }

        private static string GetPropertyHashFromType<T>(T value)
        {
            var typeProperties = typeof(T).GetProperties().Select(p => p.GetValue(value));
            var propertiesValues = string.Join(string.Empty, typeProperties);

            using var sha = new SHA256Managed();
            var textData = Encoding.UTF8.GetBytes(propertiesValues);
            var hash = sha.ComputeHash(textData);
            var hashKey = BitConverter.ToString(hash).Replace("-", string.Empty);

            return hashKey;
        }

    }
}