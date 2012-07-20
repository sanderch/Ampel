using AmpelLib;

namespace AmpelTest
{
    class AmpelStub : IAmpel
    {
        public bool Green { get; set; }
        public bool Yellow { get; set; }
        public bool Red { get; set; }

        public void Light(LightColor color)
        {
            Off();
            switch (color)
            {
                case LightColor.Green:
                    Green = true;
                    break;
                case LightColor.Yellow:
                    Yellow = true;
                    break;
                case LightColor.Red:
                    Red = true;
                    break;
            }
        }

        public void Light(LightColor color1, LightColor color2)
        {
            Off();
            switch (color1)
            {
                case LightColor.Green:
                    Green = true;
                    break;
                case LightColor.Yellow:
                    Yellow = true;
                    break;
                case LightColor.Red:
                    Red = true;
                    break;
            }
            switch (color2)
            {
                case LightColor.Green:
                    Green = true;
                    break;
                case LightColor.Yellow:
                    Yellow = true;
                    break;
                case LightColor.Red:
                    Red = true;
                    break;
            }
        }

        public void Off()
        {
            Green = Yellow = Red = false;
        }

    }
}
