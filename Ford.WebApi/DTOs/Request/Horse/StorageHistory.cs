namespace Ford.WebApi.Dtos.Horse;

public class StorageHistory<T> where T : IStorageAction
{
    public ActionType ActionType { get; set; }
    public T Data { get; set; }

    public StorageHistory(ActionType actionType, T data)
    {
        ActionType = actionType;
        Data = data;
    }
}

public enum ActionType
{
    CreateHorse,
    UpdateHorse,
    DeleteHorse,
    CreateSave,
    UpdateSave,
    DeleteSave,
}