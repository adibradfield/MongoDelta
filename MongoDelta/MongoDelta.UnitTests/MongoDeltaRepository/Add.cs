using System;
using MongoDB.Driver;
using MongoDelta.UnitTests.Models;
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
            var collection = new Mock<IMongoCollection<BlankAggregate>>();
            var repository = new MongoDeltaRepository<BlankAggregate>(collection.Object);

            var model = new BlankAggregate();

            Assert.DoesNotThrow(() =>
            {
                repository.Add(model);
            });
        }

        [Test]
        public void AddSingleModelTwice_Failure()
        {
            var collection = new Mock<IMongoCollection<BlankAggregate>>();
            var repository = new MongoDeltaRepository<BlankAggregate>(collection.Object);

            var model = new BlankAggregate();

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
            var collection = new Mock<IMongoCollection<BlankAggregate>>();
            var repository = new MongoDeltaRepository<BlankAggregate>(collection.Object);

            Assert.Throws<ArgumentNullException>(() =>
            {
                repository.Add(null);
            });
        }
    }
}