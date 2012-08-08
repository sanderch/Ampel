using System.Collections.Generic;
using AmpelLib;
using FBService;
using NUnit.Framework;

namespace AmpelTest
{
    [TestFixture]
    public class AmpelServiceTest
    {
        private AmpelStub _ampel;
        private IAmpelService _ampelService;

        [SetUp]
        public void create_stubs()
        {
            _ampel = new AmpelStub();
            _ampelService = new AmpelService();
        }

        [Test]
        public void ampel_is_green_if_builds_succeeded()
        {
            var projects = new List<ProjectInformation>
                {
                    new ProjectInformation {Name = "proj1", Status = ProjectStatus.Success},
                    new ProjectInformation {Name = "proj2", Status = ProjectStatus.Success},
                    new ProjectInformation {Name = "proj3", Status = ProjectStatus.Success}
                };

            _ampelService.ToggleAmpel(_ampel, projects);

            Assert.That(_ampel.Green, Is.True);
            Assert.That(_ampel.Red, Is.False);
            Assert.That(_ampel.Yellow, Is.False);
        }

        [Test]
        public void ampel_is_yellow_if_build_is_in_progress()
        {
            var projects = new List<ProjectInformation>
                {
                    new ProjectInformation {Name = "proj1", Status = ProjectStatus.Success},
                    new ProjectInformation {Name = "proj2", Status = ProjectStatus.Running},
                    new ProjectInformation {Name = "proj3", Status = ProjectStatus.Failure}
                };

            _ampelService.ToggleAmpel(_ampel, projects);

            Assert.That(_ampel.Yellow, Is.True);
        }

        [Test]
        public void ampel_is_red_if_build_is_broken()
        {
            var projects = new List<ProjectInformation>
                {
                    new ProjectInformation {Name = "proj1", Status = ProjectStatus.Success},
                    new ProjectInformation {Name = "proj2", Status = ProjectStatus.Running},
                    new ProjectInformation {Name = "proj3", Status = ProjectStatus.Failure}
                };

            _ampelService.ToggleAmpel(_ampel, projects);

            Assert.That(_ampel.Red, Is.True);
        }
    }
}