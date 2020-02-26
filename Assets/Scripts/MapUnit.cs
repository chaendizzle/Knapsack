using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MapUnit
{
    public MovementType movementType;
    public float maxMovement;
}

public enum MovementType
{
    GROUND, AIR
}