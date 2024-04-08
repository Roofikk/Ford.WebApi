namespace Ford.WebApi.Dtos.Horse;

public class StorageHistory
{
    public ActionType ActionType { get; set; }
    public IStorageData Data { get; set; }

    public StorageHistory(ActionType actionType, IStorageData data)
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