using System.Collections.Generic;

namespace Arma3Event.Services.ArmaPersist
{
    public class PersistVehicle
    {
        public PersistVehicle(List<object> vehicleData, int id)
        {
            VehicleId = id;
            this.Name = (string)vehicleData[0];
            this.IsAlive = (bool)vehicleData[1];
            this.Position = new PersistPosition((List<object>)vehicleData[2]);
            this.Direction = (float)vehicleData[3];
            this.Items = PersistItem.LoadFromCargo((List<object>)vehicleData[4]);
        }

        public int VehicleId { get; }
        public string Name { get; }
        public bool IsAlive { get; }
        public PersistPosition Position { get; }
        public float Direction { get; }
        public List<PersistItem> Items { get; }
    }
}