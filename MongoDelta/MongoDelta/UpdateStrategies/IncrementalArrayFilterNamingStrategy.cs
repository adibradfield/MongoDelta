namespace MongoDelta.UpdateStrategies
{
    class IncrementalArrayFilterNamingStrategy : IArrayFilterNamingStrategy
    {
        private int _counter = 1;
        public string GetNextName()
        {
            return $"af{_counter++}";
        }
    }
}
