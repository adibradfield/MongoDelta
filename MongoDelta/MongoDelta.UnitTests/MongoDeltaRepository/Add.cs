using System;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace MongoDelta.UnitTests.MongoDeltaRepository
{
    [TestFixture]
    public class Add
    {
        [Test]
        public void AddSingleModel_Success()
        {
            var collection = new Mock<IMongoCollection<TestAggregate>>();
            var repository = new MongoDeltaRepository<TestAggregate>(collection.Object);

            var model = new TestAggregate();

            Assert.DoesNotThrow(() =>
            {
                repository.Add(model);
            });
        }

        [Test]
        public void AddSingleModelTwice_Failure()
        {
            var collection = new Mock<IMongoCollection<TestAggregate>>();
            var repository = new MongoDeltaRepository<TestAggregate>(collection.Object);

            var model = new TestAggregate();

            Assert.DoesNotThrow(() =>
            {
                repository.Add(model);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                repository.Add(model);
            });
        }

        [Test]
        public void AddNull_Failure()
        {
            var collection = new Mock<IMongoCollection<TestAggregate>>();
            var repository = new MongoDeltaRepository<TestAggregate>(collection.Object);

            Assert.Throws<ArgumentNullException>(() =>
            {
                repository.Add(null);
            });
        }

        public class TestAggregate{}
    }
}