using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModifier
{
    // Start is called before the first frame update
    void AddValue(ref int baseValue);
}
