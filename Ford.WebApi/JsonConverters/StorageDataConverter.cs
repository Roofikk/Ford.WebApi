using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ford.WebApi.JsonConverters;

public class StorageDataConverter : JsonConverter<StorageHistory>
{
    public override StorageHistory? ReadJson(JsonReader reader, Type objectType, StorageHistory? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var actionTypeLong = jObject["ActionType"]!.Value<long>();
        var actionType = (ActionType)actionTypeLong;

        if (actionType == ActionType.CreateHorse || actionType == ActionType.UpdateHorse || actionType == ActionType.DeleteHorse)
        {
            jObject["Data"]!.ToObject<HorseDto>();
            var horse = jObject["Data"]!.ToObject<HorseDto>();

            if (horse != null)
            {
                return new StorageHistory(actionType, horse);
            }
            else
            {
                throw new Exception("Horse is null");
            }
        }
        else if (actionType == ActionType.CreateSave || actionType == ActionType.UpdateSave || actionType == ActionType.DeleteSave)
        {
            var save = jObject["Data"]!.ToObject<FullSaveDto>();

            if (save != null)
            {
                return new StorageHistory(actionType, save);
            }
            else
            {
                throw new Exception("Save is null");
            }
        }
        else
        {
            return jObject.ToObject<StorageHistory>();
        }
    }

    public override void WriteJson(JsonWriter writer, StorageHistory? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
