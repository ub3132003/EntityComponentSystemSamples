using System.Collections;
using UnityEngine;

public enum COST_TYPES
{
    FLAT,
    PERCENT_OF_MAX,
    PERCENT_OF_CURRENT
}

public enum COMPARABLE_TYPE
{
    GREAT,
    EQUAL,
    LESS
}

public enum TRIGGER
{
    NONE,
    HIGH_LEVEL = 1,
    LOW_LEVEL = 1 << 1,
    RISING_EDGE = 1 << 2,
    FALLING_EDGE = 1 << 3,
}
