using System.Diagnostics;

namespace NearestVehiclePosition
{
    public class BruteForceLogic
    {
        // This is the Brute Force method of using a nested loop to find the closest vehicle to each point. This will be our base benchmark.
        public static void BruteForce()
        {
            Stopwatch stopWatchReadFile = new Stopwatch();
            stopWatchReadFile.Start();

            var data = Vehicles();

            stopWatchReadFile.Stop();
            TimeSpan ts = stopWatchReadFile.Elapsed;            

            Stopwatch stopWatchBruteForceInMemorySearch = new Stopwatch();
            stopWatchBruteForceInMemorySearch.Start();

            foreach (Coordinate coordinate in SampleData.GetCoordinates())
            {
                float closestDistance = float.MaxValue;
                VehiclePosition closestVehicle = new VehiclePosition();

                foreach (VehiclePosition vehicle in data)
                {
                    float distance = CalculateDistance(coordinate.Latitude, coordinate.Longitude, vehicle.Latitude, vehicle.Longitude);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestVehicle = vehicle;
                    }
                }

                Console.WriteLine("Closest vehicle to ({0}, {1}):", coordinate.Latitude, coordinate.Longitude);
                Console.WriteLine("Vehicle ID: {0}", closestVehicle.VehicleId);
                Console.WriteLine("Registration: {0}", closestVehicle.VehicleRegistration);
                Console.WriteLine("Latitude: {0}", closestVehicle.Latitude);
                Console.WriteLine("Longitude: {0}", closestVehicle.Longitude);
                Console.WriteLine();
            }

            stopWatchBruteForceInMemorySearch.Stop();

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine();
            Console.WriteLine("Brute Force Read File RunTime: " + elapsedTime);

            ts = stopWatchBruteForceInMemorySearch.Elapsed;

            // Format and display the TimeSpan value.
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("Brute Force Runtime: " + elapsedTime);
            Console.WriteLine();
            Console.WriteLine();
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
    }
}
