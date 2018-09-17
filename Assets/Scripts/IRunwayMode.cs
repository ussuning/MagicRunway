using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRunwayMode
{
    Mode GetMode();
    void Begin();
    void SetUp();
    void End();
}
