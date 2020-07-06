using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace TestAddingListKeys
{
    public class AddBigKeyValue
    {
        private ServiceProvider ServiceProvider { get; set; }

        private IRedisCacheClient _redisCacheClient;

        [SetUp]
        public void Setup()
        {
            ServiceProvider = new ServiceCollection()
                .AddStackExchangeRedisExtensions<NewtonsoftSerializer>(new RedisConfiguration
                {
                    Password = "################",
                    AllowAdmin = true,
                    Ssl = false,
                    ConnectTimeout = 6000,
                    SyncTimeout = 100,
                    Database = 0,
                    Hosts = new[] { new RedisHost { Host = "##.##.##.##", Port = 6379 } }
                })
                .BuildServiceProvider();

            _redisCacheClient = ServiceProvider.GetService<IRedisCacheClient>();
        }

        [Test]
        public async Task AddManyListBigKeys()
        {
            var random = new Random();

            using var stream = new StreamReader("words.txt");
            var strings = (await stream.ReadToEndAsync()).Replace("\r", string.Empty).Split(new[] { '\n' }).ToList();

            for (var i = 0; i < 100; i++)
            {
                var values = new List<Tuple<string, string>>();

                for (var j = 0; j < 1000; j++)
                {
                    var key = random.Next(1, int.MaxValue - 1).ToString();

                    var text = string.Join(' ',
                        strings.GetRange(random.Next(1, strings.Count - 1001), random.Next(10, 1000)));

                    var tuple = new Tuple<string, string>(key, text);
                    values.Add(tuple);
                }

                try
                {
                    await _redisCacheClient.Db1.AddAllAsync(values, TimeSpan.FromMilliseconds(1));
                }
                catch (RedisTimeoutException exception)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(2));

                    foreach (var value in values)
                    {
                        var exists = await _redisCacheClient.Db1.ExistsAsync(value.Item1);
                        Assert.IsFalse(exists, value.Item1);
                    }
                }
            }
        }
    }
}
