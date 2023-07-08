namespace IL2ShakerDriver.Samplers;

internal interface ISampleWriter
{
    void Write(float[] buffer, int offset, int count, SimTime simTime);
}