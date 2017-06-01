namespace MasterApi.Core.Data
{
    public interface IIdentifiable<out TKey>
    {
        /// <summary>
        /// Gets or sets the primary key for this entity.
        /// </summary>        
        TKey Id { get; }
    }
}
