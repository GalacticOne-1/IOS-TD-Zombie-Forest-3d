namespace Galactic1.Code.UI.Stations
{
    public readonly struct StorageAlertDTO
    {
        public readonly bool IsRequired; // станция требует спец хранилища
        public readonly bool IsBuilt; // хранилище построено
        public readonly string StorageType; // название типа хранилища

        public StorageAlertDTO(bool isRequired, bool isBuilt, string storageType)
        {
            IsRequired = isRequired;
            IsBuilt = isBuilt;
            StorageType = storageType;
        }

        public bool ShowAlert => IsRequired && !IsBuilt;
    }
}