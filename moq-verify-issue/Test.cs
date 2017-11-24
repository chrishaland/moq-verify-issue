using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace moq_verify_issue
{
    [TestFixture]
    public class Test
    {
        private Mock<IUpdater> _updateMock;
        private RemoveTvCommand _subject;

        [SetUp]
        public void Before()
        {
            _updateMock = new Mock<IUpdater>();
            _subject = new RemoveTvCommand(_updateMock.Object);
        }

        [Test]
        public void Verify_should_differentiate_the_object_on_update_call()
        {
            var tv1 = new Tv {Id = 1};
            var tv2 = new Tv {Id = 2};

            var house = new House
            {
                Id = 1,
                DoorStatus = DoorStatus.Locked,
                Tvs = new List<Tv> {tv1, tv2}
            };

            _subject.Handle(house);

            _updateMock.Verify(m => m.Update(It.IsAny<House>()), Times.Exactly(2));

            // Expected
            _updateMock.Verify(m => m.Update(It.Is<House>(h => h.Id == house.Id && h.DoorStatus == DoorStatus.Open && h.Tvs.Contains(tv1) && h.Tvs.Contains(tv2))), Times.Once);
            _updateMock.Verify(m => m.Update(It.Is<House>(h => h.Id == house.Id && h.DoorStatus == DoorStatus.Locked && !h.Tvs.Any())), Times.Once);

            // Actual
            _updateMock.Verify(m => m.Update(It.Is<House>(h => h.Id == house.Id && h.DoorStatus == DoorStatus.Locked && !h.Tvs.Any())), Times.Exactly(2));
        }
    }

    public class RemoveTvCommand
    {
        private readonly IUpdater _updater;

        public RemoveTvCommand(IUpdater updater)
        {
            _updater = updater;
        }

        public void Handle(House house)
        {
            var doorStatus = house.DoorStatus;
            var isClosed = house.DoorStatus == DoorStatus.Locked;

            if (isClosed)
            {
                house.DoorStatus = DoorStatus.Open;
                _updater.Update(house);
            }

            if (house.Tvs == null || !house.Tvs.Any()) return;

            foreach (var tv in house.Tvs.ToList())
            {
                house.Tvs.Remove(tv);
            }

            if (isClosed)
            {
                house.DoorStatus = doorStatus;
            }

            _updater.Update(house);
        }
    }

    public interface IUpdater
    {
        void Update(House house);
    }

    public class House
    {
        public int Id { get; set; }
        public DoorStatus DoorStatus { get; set; }
        public List<Tv> Tvs { get; set; }
    }

    public class Tv
    {
        public int Id { get; set; }
    }

    public enum DoorStatus
    {
        Locked,
        Open
    }
}
