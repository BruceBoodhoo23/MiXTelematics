using System.Diagnostics;

namespace NearestVehiclePosition
{
    public class KDTreeLogic
    {
        private static KDTreeNode root;

        public static void KDTree()
        {
            Stopwatch stopWatchReadFile = new Stopwatch();
            stopWatchReadFile.Start();

            var data = Vehicles();

            stopWatchReadFile.Stop();
            TimeSpan ts = stopWatchReadFile.Elapsed;
            
            root = BuildTree(data, 0);

            Stopwatch stopWatchKDtree = new Stopwatch();
            stopWatchKDtree.Start();            

            foreach (Coordinate coordinate in SampleData.GetCoordinates())
            {
                VehiclePosition nearestVehicle = FindNearestVehicle(coordinate.Latitude, coordinate.Longitude);

                Console.WriteLine("Closest vehicle to ({0}, {1}):", coordinate.Latitude, coordinate.Longitude);
                Console.WriteLine("Vehicle ID: {0}", nearestVehicle.VehicleId);
                Console.WriteLine("Registration: {0}", nearestVehicle.VehicleRegistration);
                Console.WriteLine("Latitude: {0}", nearestVehicle.Latitude);
                Console.WriteLine("Longitude: {0}", nearestVehicle.Longitude);
                Console.WriteLine();
            }

            stopWatchKDtree.Stop();

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine();
            Console.WriteLine("KD Tree Read File RunTime: " + elapsedTime);

            ts = stopWatchKDtree.Elapsed;
            // Format and display the TimeSpan value.
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("KD Tree Runtime: " + elapsedTime);
        }

        // recursive method to build the tree node
        private static KDTreeNode BuildTree(List<VehiclePosition> vehicles, int depth)
        {
            if (vehicles.Count == 0)
                return null;

            // we sort the list of vehicles so we can grab the median to use in our traversal - this will take up resources/time.
            int axis = depth % 2;

            vehicles.Sort((a, b) => axis == 0 ? a.Latitude.CompareTo(b.Latitude) : a.Longitude.CompareTo(b.Longitude));

            int medianIndex = vehicles.Count / 2;
            KDTreeNode node = new KDTreeNode(vehicles[medianIndex]);
            node.Left = BuildTree(vehicles.GetRange(0, medianIndex), depth + 1);
            node.Right = BuildTree(vehicles.GetRange(medianIndex + 1, vehicles.Count - medianIndex - 1), depth + 1);

            return node;
        }

        public static VehiclePosition FindNearestVehicle(float targetLat, float targetLon)
        {
            KDTreeNode nearestNode = FindNearestNode(root, targetLat, targetLon, 0);
            return nearestNode.Vehicle;
        }

        private static KDTreeNode FindNearestNode(KDTreeNode node, float targetLat, float targetLon, int depth)
        {
            if (node == null)
                return null;

            int axis = depth % 2;
            KDTreeNode nearestNode = node;
            float currentDistance = CalculateDistance(targetLat, targetLon, node.Vehicle.Latitude, node.Vehicle.Longitude);
            float splitValue = axis == 0 ? node.Vehicle.Latitude : node.Vehicle.Longitude;
            float targetValue = axis == 0 ? targetLat : targetLon;

            if (targetValue < splitValue)
            {
                KDTreeNode leftNode = FindNearestNode(node.Left, targetLat, targetLon, depth + 1);
                if (leftNode != null)
                {
                    float leftDistance = CalculateDistance(targetLat, targetLon, leftNode.Vehicle.Latitude, leftNode.Vehicle.Longitude);
                    if (leftDistance < currentDistance)
                    {
                        nearestNode = leftNode;
                        currentDistance = leftDistance;
                    }
                }

                float perpendicularDistance = Math.Abs(splitValue - targetValue);
                if (perpendicularDistance < currentDistance)
                {
                    KDTreeNode rightNode = FindNearestNode(node.Right, targetLat, targetLon, depth + 1);
                    if (rightNode != null)
                    {
                        float rightDistance = CalculateDistance(targetLat, targetLon, rightNode.Vehicle.Latitude, rightNode.Vehicle.Longitude);
                        if (rightDistance < currentDistance)
                        {
                            nearestNode = rightNode;
                        }
                    }
                }
            }
            else
            {
                KDTreeNode rightNode = FindNearestNode(node.Right, targetLat, targetLon, depth + 1);
                if (rightNode != null)
                {
                    float rightDistance = CalculateDistance(targetLat, targetLon, rightNode.Vehicle.Latitude, rightNode.Vehicle.Longitude);
                    if (rightDistance < currentDistance)
                    {
                        nearestNode = rightNode;
                        currentDistance = rightDistance;
                    }
                }

                float perpendicularDistance = Math.Abs(splitValue - targetValue);
                if (perpendicularDistance < currentDistance)
                {
                    KDTreeNode leftNode = FindNearestNode(node.Left, targetLat, targetLon, depth + 1);
                    if (leftNode != null)
                    {
                        float leftDistance = CalculateDistance(targetLat, targetLon, leftNode.Vehicle.Latitude, leftNode.Vehicle.Longitude);
                        if (leftDistance < currentDistance)
                        {
                            nearestNode = leftNode;
                        }
                    }
                }
            }
            return nearestNode;
        }

        private static List<VehiclePosition> Vehicles()
        {
            const string filePath = "VehiclePositions.dat";
            List<VehiclePosition> vehicles = new List<VehiclePosition>();

            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var vehicle = new VehiclePosition
                    {
                        VehicleId = reader.ReadInt32(),
                        VehicleRegistration = ReadNullTerminatedString(reader),
                        Latitude = reader.ReadSingle(),
                        Longitude = reader.ReadSingle(),
                        RecordedTimeUTC = reader.ReadUInt64()
                    };

                    vehicles.Add(vehicle);
                }
            }

            return vehicles;
        }

        static string ReadNullTerminatedString(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                bytes.Add(b);
            }
            return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
        }

        // calculate the distance in kilometers based on the latitude and longitude coordinates using the Haversine formula.
        static float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
        {
            // Radius of the Earth in kilometers
            const float earthRadius = 6371f;

            // Convert latitude and longitude to radians
            float latRad1 = (float)(Math.PI * lat1 / 180f);
            float lonRad1 = (float)(Math.PI * lon1 / 180f);
            float latRad2 = (float)(Math.PI * lat2 / 180f);
            float lonRad2 = (float)(Math.PI * lon2 / 180f);

            // Calculate the differences between the latitudes and longitudes
            float dLat = latRad2 - latRad1;
            float dLon = lonRad2 - lonRad1;

            // Apply the Haversine formula
            float a = (float)(Math.Sin(dLat / 2f) * Math.Sin(dLat / 2f) +
                             Math.Cos(latRad1) * Math.Cos(latRad2) *
                             Math.Sin(dLon / 2f) * Math.Sin(dLon / 2f));
            float c = 2f * (float)Math.Atan2(Math.Sqrt(a), Math.Sqrt(1f - a));
            float distance = earthRadius * c;

            return distance;
        }
    }
}
