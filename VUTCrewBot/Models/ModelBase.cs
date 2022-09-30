

using VUTCrewBot.DAL.Entities;

namespace ZbytkyBot.Models
{
    public class ModelBase<TEntity> where TEntity:BasicEntity
    {
        public TEntity Entity;
        public int Id { get => Entity.Id; }
        public ModelBase(TEntity entity)
        {
            this.Entity = entity;
        }
    }
}
