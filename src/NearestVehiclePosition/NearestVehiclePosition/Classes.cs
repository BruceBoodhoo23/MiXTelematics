namespace NearestVehiclePosition
{
    public class VehiclePosition
    {
        public int VehicleId { get; set; }
        public string VehicleRegistration { get; set; } = string.Empty;
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public ulong RecordedTimeUTC { get; set; }
    }

    public struct Coordinate
    {
        public float Latitude;
        public float Longitude;
    }

    public class KDTreeNode
    {
        public VehiclePosition Vehicle;
        public KDTreeNode Left;
        public KDTreeNode Right;

        public KDTreeNode(VehiclePosition vehicle)
        {
            Vehicle = vehicle;
            Left = null;
            Right = null;
        }
    }

}
