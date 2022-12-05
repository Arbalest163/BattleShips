using System.Collections.Generic;

public interface ICleanUp
{
    string SceneName { get; }
    void Cleanup();
}