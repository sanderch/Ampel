namespace AmpelLib
{
    public interface IAmpel
    {
        void Light(LightColor color);
        void Light(LightColor color1, LightColor color2);
        void Off();
    }
}