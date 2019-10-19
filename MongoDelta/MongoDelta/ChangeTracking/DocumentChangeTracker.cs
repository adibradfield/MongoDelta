using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.ChangeTracking.ElementChangeTrackers;
using MongoDelta.Mapping;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking
{
    class DocumentChangeTracker
    {
        private readonly DocumentElementChangeTrackerFactory _changeTrackerFactory = new DocumentElementChangeTrackerFactory();
        private readonly IDocumentElementChangeTracker[] _changeTrackers;
        private readonly bool _shouldReplace;
        private readonly BsonClassMap _classMap;

        public DocumentChangeTracker(Type modelType)
        {
            _classMap = BsonClassMap.LookupClassMap(modelType);
            _shouldReplace = !_classMap.ShouldUseDeltaUpdateStrategy();

            if (!_shouldReplace)
            {
                _changeTrackers = _classMap.AllMemberMaps
                    .Select(map => _changeTrackerFactory.GetChangeTrackerForElement(map)).ToArray();
            }
        }

        public UpdateDefinition GetUpdatesForChanges(BsonDocument original, BsonDocument current)
        {
            if (_shouldReplace)
            {
                var model = original != current ? BsonSerializer.Deserialize(current, _classMap.ClassType) : null;
                return UpdateDefinition.Replace(model);
            }

            var updateDefinition = UpdateDefinition.Delta;
            if (original == current) return updateDefinition;

            foreach (var changeTracker in _changeTrackers)
            {
                changeTracker.ApplyChangesToDefinition(updateDefinition, original, current);
            }

            return updateDefinition;
        }
    }
}
