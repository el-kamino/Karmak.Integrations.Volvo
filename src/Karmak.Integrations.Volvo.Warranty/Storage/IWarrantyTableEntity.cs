namespace Karmak.Integrations.Volvo.Warranty.Storage
{
    public interface IWarrantyTableEntity
    {
        void PrepareToSave();
        void PopulateFollowingLoad();
    }
}
