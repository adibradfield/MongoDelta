using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace MongoDelta.UpdateStrategies
{
    abstract class PreparedWriteModel
    {
        protected readonly object InternalWriteModel;

        protected PreparedWriteModel(object internalWriteModel)
        {
            InternalWriteModel = internalWriteModel;
        }

        public PreparedWriteModel<T> Cast<T>() => (PreparedWriteModel<T>) this;
        public abstract void ThrowIfNotValid();
    }

    class PreparedWriteModel<T> : PreparedWriteModel
    {
        public PreparedWriteModel(IEnumerable<WriteModel<T>> internalWriteModel) : base(internalWriteModel) { }
        public bool HasChanges => MongoWriteModels.Any();
        public IEnumerable<WriteModel<T>> MongoWriteModels => (IEnumerable<WriteModel<T>>) InternalWriteModel;
        public override void ThrowIfNotValid()
        {
            foreach (var mongoWriteModel in MongoWriteModels)
            {
                mongoWriteModel.ThrowIfNotValid();
            }
        }
    }
}
