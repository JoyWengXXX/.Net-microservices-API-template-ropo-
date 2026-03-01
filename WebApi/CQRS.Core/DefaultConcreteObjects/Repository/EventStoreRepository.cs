using CQRS.Core.DefaultConcreteObjects.Config;
using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CQRS.Core.DefaultConcreteObjects.Repository
{
    public class EventStoreRepository : IEventStoreRepository, IDisposable
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly string _schema;
        private readonly string _table;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;

        public EventStoreRepository(IEventStoreConnectionManager connectionManager, IOptions<EventStorePostgresConfig> config)
        {
            _dataSource = connectionManager.GetDataSource();
            _schema = string.IsNullOrWhiteSpace(config.Value.Schema) ? "public" : config.Value.Schema;
            _table = string.IsNullOrWhiteSpace(config.Value.Table) ? "EventSourcingEvent" : config.Value.Table;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<List<EventModel>> FindAllAsync()
        {
            var sql = $"SELECT id, time_stamp, aggregate_identifier, aggregate_type, event_operator, version, event_type, event_data FROM {QualifiedTableName()} ORDER BY time_stamp, version";
            using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            var results = new List<EventModel>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                results.Add(ReadEventModel(reader));
            }

            return results;
        }

        public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
        {
            var sql = $"SELECT id, time_stamp, aggregate_identifier, aggregate_type, event_operator, version, event_type, event_data FROM {QualifiedTableName()} WHERE aggregate_identifier = @aggregate_identifier ORDER BY time_stamp, version";
            using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("aggregate_identifier", aggregateId);
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            var results = new List<EventModel>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                results.Add(ReadEventModel(reader));
            }

            return results;
        }

        public async Task SaveAsync(EventModel @event)
        {
            var sql = $"INSERT INTO {QualifiedTableName()} (time_stamp, aggregate_identifier, aggregate_type, event_operator, version, event_type, event_data) VALUES (@time_stamp, @aggregate_identifier, @aggregate_type, @event_operator, @version, @event_type, @event_data) RETURNING id";
            using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("time_stamp", @event.TimeStamp);
            command.Parameters.AddWithValue("aggregate_identifier", @event.AggregateIdentifier);
            command.Parameters.AddWithValue("aggregate_type", @event.AggregateType ?? string.Empty);
            command.Parameters.AddWithValue("event_operator", @event.EventOperator ?? string.Empty);
            command.Parameters.AddWithValue("version", @event.Version);
            command.Parameters.AddWithValue("event_type", @event.EventType ?? @event.EventData?.GetType().Name ?? string.Empty);

            var eventDataJson = SerializeEvent(@event.EventData);
            var eventDataParam = command.Parameters.Add("event_data", NpgsqlDbType.Jsonb);
            eventDataParam.Value = eventDataJson;

            var id = await command.ExecuteScalarAsync().ConfigureAwait(false);
            if (id is Guid eventId)
            {
                @event.Id = eventId;
            }
        }

        public async Task UpdateAsync(EventModel @event)
        {
            var sql = $"UPDATE {QualifiedTableName()} SET time_stamp = @time_stamp WHERE aggregate_identifier = @aggregate_identifier AND version = @version";
            using var connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("time_stamp", @event.TimeStamp);
            command.Parameters.AddWithValue("aggregate_identifier", @event.AggregateIdentifier);
            command.Parameters.AddWithValue("version", @event.Version);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        private EventModel ReadEventModel(NpgsqlDataReader reader)
        {
            var eventType = reader.GetString(reader.GetOrdinal("event_type"));
            var eventDataJson = reader.GetFieldValue<string>(reader.GetOrdinal("event_data"));
            var eventDataNode = string.IsNullOrEmpty(eventDataJson) ? JsonNode.Parse("{}") : JsonNode.Parse(eventDataJson);

            return new EventModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                TimeStamp = reader.GetDateTime(reader.GetOrdinal("time_stamp")),
                AggregateIdentifier = reader.GetGuid(reader.GetOrdinal("aggregate_identifier")),
                AggregateType = ReadNullableString(reader, "aggregate_type"),
                EventOperator = ReadNullableString(reader, "event_operator"),
                Version = reader.GetInt32(reader.GetOrdinal("version")),
                EventType = eventType,
                EventData = DeserializeEvent(eventType, eventDataNode)
            };
        }

        private BaseEvent DeserializeEvent(string eventType, JsonNode eventDataNode)
        {
            var type = ResolveEventType(eventType);
            var jsonString = eventDataNode?.ToString() ?? "{}";
            var deserialized = JsonSerializer.Deserialize(jsonString, type, _jsonOptions);
            if (deserialized is not BaseEvent baseEvent)
            {
                throw new InvalidOperationException($"Event payload could not be deserialized as BaseEvent: {eventType}");
            }

            return baseEvent;
        }

        private JsonNode SerializeEvent(BaseEvent eventData)
        {
            if (eventData == null)
            {
                return JsonNode.Parse("{}");
            }

            var jsonString = JsonSerializer.Serialize(eventData, eventData.GetType(), _jsonOptions);
            return JsonNode.Parse(jsonString);
        }

        private static string ReadNullableString(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        private static Type ResolveEventType(string eventType)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(type => type != null)!;
                    }
                });

            var resolved = types.FirstOrDefault(type =>
                type != null &&
                typeof(BaseEvent).IsAssignableFrom(type) &&
                type.Name == eventType);

            return resolved ?? throw new InvalidOperationException($"Event type not found: {eventType}");
        }

        private string QualifiedTableName()
        {
            return $"{QuoteIdentifier(_schema)}.{QuoteIdentifier(_table)}";
        }

        private static string QuoteIdentifier(string identifier)
        {
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }
    }
}

