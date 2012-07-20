using System;
using AmpelLib;

namespace AmpelTest
{
    class AmpelStub : IAmpel
    {
        public void Light(LightColor color)
        {
            throw new NotImplementedException();
        }

        public void Light(LightColor color1, LightColor color2)
        {
            throw new NotImplementedException();
        }

        public void Off()
        {
            throw new NotImplementedException();
        }
    }
}
