﻿using BookSleeve;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloudStructures.Redis
{
    public class RedisSet<T>
    {
        public string Key { get; private set; }
        public int Db { get; private set; }
        readonly RedisSettings settings;
        readonly IRedisValueConverter valueConverter;

        public RedisSet(RedisSettings settings, string stringKey)
        {
            this.settings = settings;
            this.Db = settings.Db;
            this.valueConverter = settings.ValueConverter;
            this.Key = stringKey;
        }

        public RedisSet(RedisGroup connectionGroup, string stringKey)
            : this(connectionGroup.GetSettings(stringKey), stringKey)
        {
        }

        protected RedisConnection Connection
        {
            get
            {
                return settings.GetConnection();
            }
        }

        protected ISetCommands Command
        {
            get
            {
                return Connection.Sets;
            }
        }

        /// <summary>
        /// SADD http://redis.io/commands/sadd
        /// </summary>
        public Task<bool> Add(T value, bool queueJump = false)
        {
            return Command.Add(Db, Key, valueConverter.Serialize(value), queueJump);
        }

        /// <summary>
        /// SADD http://redis.io/commands/sadd
        /// </summary>
        public Task<long> Add(T[] values, bool queueJump = false)
        {
            var v = values.Select(x => valueConverter.Serialize(x)).ToArray();
            return Command.Add(Db, Key, v, queueJump);
        }

        /// <summary>
        /// SISMEMBER http://redis.io/commands/sismember
        /// </summary>
        public Task<bool> Contains(T value, bool queueJump = false)
        {
            return Command.Contains(Db, Key, valueConverter.Serialize(value), queueJump);
        }


        /// <summary>
        /// SMEMBERS http://redis.io/commands/smembers
        /// </summary>
        public async Task<T[]> GetAll(bool queueJump = false)
        {
            var v = await Command.GetAll(Db, Key, queueJump).ConfigureAwait(false);
            return v.Select(valueConverter.Deserialize<T>).ToArray();
        }

        /// <summary>
        /// SCARD http://redis.io/commands/scard
        /// </summary>
        public Task<long> GetLength(bool queueJump = false)
        {
            return Command.GetLength(Db, Key, queueJump);
        }

        /// <summary>
        /// SRANDMEMBER http://redis.io/commands/srandmember
        /// </summary>
        public async Task<T> GetRandom(bool queueJump = false)
        {
            var v = await Command.GetRandom(Db, Key, queueJump).ConfigureAwait(false);
            return valueConverter.Deserialize<T>(v);
        }

        /// <summary>
        /// SRANDMEMBER http://redis.io/commands/srandmember
        /// </summary>
        public async Task<T[]> GetRandom(int count, bool queueJump = false)
        {
            var v = await Command.GetRandom(Db, Key, count, queueJump).ConfigureAwait(false);
            return v.Select(valueConverter.Deserialize<T>).ToArray();
        }

        /// <summary>
        /// SREM http://redis.io/commands/srem
        /// </summary>
        public Task<bool> Remove(T member, bool queueJump = false)
        {
            return Command.Remove(Db, Key, valueConverter.Serialize(member), queueJump);
        }

        /// <summary>
        /// SREM http://redis.io/commands/srem
        /// </summary>
        public Task<long> Remove(T[] members, bool queueJump = false)
        {
            var v = members.Select(x => valueConverter.Serialize(x)).ToArray();
            return Command.Remove(Db, Key, v, queueJump);
        }

        /// <summary>
        /// SPOP http://redis.io/commands/spop
        /// </summary>
        public async Task<T> RemoveRandom(bool queueJump = false)
        {
            var v = await Command.RemoveRandom(Db, Key, queueJump).ConfigureAwait(false);
            return valueConverter.Deserialize<T>(v);
        }

        public Task<bool> SetExpire(TimeSpan expire, bool queueJump = false)
        {
            return SetExpire((int)expire.TotalSeconds, queueJump);
        }

        public Task<bool> SetExpire(int seconds, bool queueJump = false)
        {
            return Connection.Keys.Expire(Db, Key, seconds, queueJump);
        }

        public Task<bool> Clear(bool queueJump = false)
        {
            return Connection.Keys.Remove(Db, Key, queueJump);
        }
    }
}