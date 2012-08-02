using AmpelLib;
using NUnit.Framework;

namespace AmpelTest
{
    [TestFixture]
    class AmpelTest
    {
        private AmpelStub _ampel;

        [SetUp]
        public void init_ampel()
        {
            _ampel = new AmpelStub();
        }

        [Test]
        public void can_turn_single_light()
        {
            _ampel.Light(LightColor.Green);
            Assert.That(_ampel.Green);
            Assert.That(!_ampel.Red);
            Assert.That(!_ampel.Yellow);
        }

        [Test]
        public void can_turn_two_lights()
        {
            _ampel.Light(LightColor.Green, LightColor.Yellow);
            Assert.That(_ampel.Green);
            Assert.That(_ampel.Yellow);
            Assert.That(!_ampel.Red);
        }

        [Test]
        public void can_turn_off_the_lights()
        {
            _ampel.Light(LightColor.Green, LightColor.Yellow);
            Assert.That(_ampel.Green);
            Assert.That(_ampel.Yellow);
            Assert.That(!_ampel.Red);

            _ampel.Off();
            Assert.That(!(_ampel.Green && _ampel.Yellow && _ampel.Red)); // TODO WTF should OR be used instead
        }
    }
}
