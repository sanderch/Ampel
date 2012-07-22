using System.Collections.Generic;
using FBService;

namespace AmpelLib
{
    public class AmpelService
    {
        private bool _lastError;
        public void ToggleAmpel(IAmpel ampel, List<ProjectInformation> projects)
        {
            var isYellow = projects.FindAll(x => x.Status == ProjectStatus.Running).Count > 0;
            var isRed = projects.FindAll(x => x.Status == ProjectStatus.Failure || x.Status == ProjectStatus.ConfigurationError).Count > 0;
            if (!isYellow)
            {
                _lastError = isRed;
            }
            var color1 = isRed || _lastError ? LightColor.Red : LightColor.Green;
            var color2 = isYellow ? LightColor.Yellow : LightColor.None;
            if (color2 != LightColor.None)
            {
                ampel.Light(color1, color2);
            }
            else
            {
                _lastError = isRed;
                ampel.Light(color1);
            }
        }
    }
}