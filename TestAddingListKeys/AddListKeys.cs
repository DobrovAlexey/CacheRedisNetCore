using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheRedisNetCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace TestAddingListKeys
{
    public class Tests
    {
        private ServiceProvider ServiceProvider { get; set; }

        private IRedisCacheClient _redisCacheClient;

        [SetUp]
        public void Setup()
        {
            ServiceProvider = new ServiceCollection()
                .AddStackExchangeRedisExtensions<NewtonsoftSerializer>(new RedisConfiguration 
                {
                    Password = "#########", 
                    AllowAdmin = true,
                    Ssl = false,
                    ConnectTimeout = 6000,
                    Database = 0,
                    Hosts = new[] { new RedisHost { Host = "##.##.##.##", Port = 6379 } }

                })
                .BuildServiceProvider();

            _redisCacheClient = ServiceProvider.GetService<IRedisCacheClient>();
        }

        [Test]
        public async Task AddOneListKeys()
        {
            var values = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("ProductOneList1", "1"),
                new Tuple<string, string>("ProductOneList2", "2"),
                new Tuple<string, string>("ProductOneList3", "3"),
            };

            await _redisCacheClient.Db1.AddAllAsync(values, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(2));

            foreach (var value in values)
            {
                var exists = await _redisCacheClient.Db1.ExistsAsync(value.Item1);
                Assert.IsFalse(exists);
            }
        }

        [Test]
        public async Task AddManyListKeys()
        {
            var valuesOneList = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("ProductManyList1", "1"),
                new Tuple<string, string>("ProductManyList2", "2"),
                new Tuple<string, string>("ProductManyList3", "3"),
            };

            await _redisCacheClient.Db1.AddAllAsync(valuesOneList, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(2));

            foreach (var value in valuesOneList)
            {
                var exists = await _redisCacheClient.Db1.ExistsAsync(value.Item1);
                Assert.IsFalse(exists);
            }

            var valuesTwoLis = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("ProductManyList4", "1"),
                new Tuple<string, string>("ProductManyList5", "2"),
                new Tuple<string, string>("ProductManyList6", "3"),
            };

            await _redisCacheClient.Db1.AddAllAsync(valuesTwoLis, TimeSpan.FromMilliseconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(2));

            foreach (var value in valuesTwoLis)
            {
                var exists = await _redisCacheClient.Db1.ExistsAsync(value.Item1);
                Assert.IsFalse(exists);
            }
        }
    }
}