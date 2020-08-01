﻿using System.Collections.Generic;

namespace Arma3Event.Services.ArmaPersist
{
    public class PersistPosition
    {
        public float X { get; set; }

        public float Y { get; set; }

        public PersistPosition(List<object> list)
        {
            this.X = (float)list[0];
            this.Y = (float)list[1];
        }
    }
}