using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoDelta.ChangeTracking
{
    internal class TrackedModelCollection<T> : IEnumerable<TrackedModel<T>> where T : class
    {
        private readonly Dictionary<T, TrackedModel<T>> _trackedModels = new Dictionary<T, TrackedModel<T>>();

        public IEnumerator<TrackedModel<T>> GetEnumerator() => _trackedModels.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void New(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (_trackedModels.TryGetValue(model, out var existingModel) && existingModel.State == TrackedModelState.Removed)
            {
                _trackedModels[model] = existingModel.WithNewState(TrackedModelState.Existing);
                return;
            }

            _trackedModels.Add(model, TrackedModel<T>.New(model));
        }

        public void Existing(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _trackedModels.Add(model, TrackedModel<T>.Existing(model));
        }

        public void Remove(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var trackedModel = _trackedModels[model];
            if (trackedModel.State == TrackedModelState.New)
            {
                _trackedModels.Remove(model);
                return;
            }

            _trackedModels[model] = trackedModel.WithNewState(TrackedModelState.Removed);
        }

        public IEnumerable<TrackedModel<T>> OfState(TrackedModelState state) => this.Where(m => m.State == state);
    }
}
