using CQRS.Core.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogInService.Infrastructure.Converters
{
    internal class EventJsonConverter : JsonConverter<BaseEvent>
    {
        public override BaseEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

