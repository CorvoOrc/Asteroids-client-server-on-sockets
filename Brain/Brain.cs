using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Brain
{
    class Brain
    {
        World world = null;

        Dictionary<Socket, KeyValuePair<Socket, NetworkStream>> nerves = null;
        Dictionary<SpaceShip, Socket> shipSocketDict = null;

        Socket server = null;
        Socket broadcast = null;

        String separator = null;
        String logPath = null;

        Int32 backlog;
        Int32 sleepTime;
        Int32 port;
        Int32 portBroad;
        //String address = null;
        //String policy = null; // The best way - use different policy server

        public const String separatorDefault = ">>";
        public const String logPathDefault = "logfile";

        const Byte sendtoall = 1;
        const Byte poschange = 2;
        const Byte angchange = 3;
        const Byte shot = 4;
        const Byte hit = 5;
        const Byte collision = 6;
        const Byte close = 7;
        const Byte newspace = 8;
        const Byte unknown = 9;

        public Brain(int port_)
        {
            world = new World();

            //address = address_;
            port = port_;
            portBroad = port + 10;
            backlog = 50;
            sleepTime = 50;

            separator = separatorDefault;
            logPath = logPathDefault;

            /*policy =
@"<?xml version=""1.0""?>
<cross-domain-policy>
    <site-control permitted-cross-domain-policies=""master-only\""/>
    <allow-access-from domain=""" + address + @""" to-ports=""10000-15000, 843""/>
</cross-domain-policy>";*/

            nerves = new Dictionary<Socket, KeyValuePair<Socket, NetworkStream>>();
            shipSocketDict = new Dictionary<SpaceShip, Socket>();

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);

            broadcast.Bind(new IPEndPoint(IPAddress.Any, portBroad));
            broadcast.Listen(backlog);
        }

        public Brain(int port_, String separator_)
        {
            world = new World();

            //address = address_;
            port = port_;
            portBroad = port + 10;
            backlog = 50;
            sleepTime = 50;

            separator = separator_;
            logPath = logPathDefault;

            /*policy =
@"<?xml version=""1.0""?>
<cross-domain-policy>
    <site-control permitted-cross-domain-policies=""master-only\""/>
    <allow-access-from domain=""" + address + @""" to-ports=""10000-15000, 843""/>
</cross-domain-policy>";*/

            nerves = new Dictionary<Socket, KeyValuePair<Socket, NetworkStream>>();
            shipSocketDict = new Dictionary<SpaceShip, Socket>();

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);

            broadcast.Bind(new IPEndPoint(IPAddress.Any, portBroad));
            broadcast.Listen(backlog);
        }

        public Brain(int port_, String separator_, String logPath_)
        {
            world = new World();

            //address = address_;
            port = port_;
            portBroad = port + 10;
            backlog = 50;
            sleepTime = 50;

            separator = separator_;
            logPath = logPath_;

            /*policy =
@"<?xml version=""1.0""?>
<cross-domain-policy>
    <site-control permitted-cross-domain-policies=""master-only\""/>
    <allow-access-from domain=""" + address + @""" to-ports=""10000-15000, 843""/>
</cross-domain-policy>";*/

            nerves = new Dictionary<Socket, KeyValuePair<Socket, NetworkStream>>();
            shipSocketDict = new Dictionary<SpaceShip, Socket>();

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);

            broadcast.Bind(new IPEndPoint(IPAddress.Any, portBroad));
            broadcast.Listen(backlog);
        }

        public void Dream()
        { 
            try
            {
                // Enter the listening loop. 
                while (true)
                {
                    Log("Waiting for a connection...");

                    //TcpClient client = server.AcceptTcpClient();
                    Socket client = server.Accept();
                    Socket sender = broadcast.Accept();
                    NetworkStream streamSender = new NetworkStream(sender);
                    //KeyValuePair<Socket, NetworkStream> pair = new KeyValuePair<Socket,NetworkStream>(sender, streamSender);
                    nerves.Add(client, new KeyValuePair<Socket, NetworkStream>(sender, streamSender));

                    Thread thread = new Thread(new ParameterizedThreadStart(Processing));
                    object clientObj = client as object;

                    try
                    {
                        thread.Start(clientObj);
                    }
                    catch(Exception e)
                    {
                        Log(String.Format("Exception: {0}", e.Message));
                    }
                }
            }
            catch (SocketException e)
            {
                Log(String.Format("SocketException: {0}", e.Message));
            }
            catch (Exception e)
            {
                Log(String.Format("Exception: {0}", e.Message));
            }
            finally
            {
                server.Close();
                broadcast.Close();

                Log("\nServer terminated...");
            }
        }

        private void Processing(Object clientObj)
        {
            Socket client = clientObj as Socket;
            NetworkStream stream = new NetworkStream(client);

            Log("Connected!");

            SpaceShip spaceShip = null;
            try
            {
                spaceShip = InitWorld(client, stream, newspace, separator);

                shipSocketDict.Add(spaceShip, client);
                world.Ships.Add(spaceShip.Id, spaceShip);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\nInit nerve is failed");
            }

            try
            {
                String[] parseData;

                while (true)
                {
                    String data = String.Empty;

                    Byte command = ReadByte(stream);
                    Log(String.Format("Received command: {0}", Brain.DecryptCommand(command)));

                    //System.Threading.Thread.Sleep(sleepTime);

                    /*if (data.Contains("<policy-file-request/>"))
                    {
                        SendPolicyFile(stream);

                        break;
                    }*/

                    switch (command)
                    {
                        case sendtoall:
                            data = Read(stream);
                            Log(String.Format("Received: {0}", data));

                            SendToAllApartSender(client, data);
                            Log(String.Format("Received and SendToAllApartSender: {0}", data));

                            break;
                        case poschange:
                            data = Read(stream);
                            Log(String.Format("Received: {0}", data));

                            parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            world.Ships[spaceShip.Id].Pos.x = Convert.ToDouble(parseData[0]);
                            world.Ships[spaceShip.Id].Pos.y = Convert.ToDouble(parseData[1]);

                            SendToAllApartSender(client, command);
                            Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                            data = spaceShip.Id + separator + data;

                            SendToAllApartSender(client, data);
                            Log(String.Format("SentToAllApartSender: {0}", data));

                            break;
                        case angchange:
                            data = Read(stream);
                            Log(String.Format("Received: {0}", data));

                            world.Ships[spaceShip.Id].Angle = Convert.ToInt32(data);

                            SendToAllApartSender(client, command);
                            Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                            data = spaceShip.Id + separator + data;

                            SendToAllApartSender(client, data);
                            Log(String.Format("SentToAllApartSender: {0}", data));

                            break;
                        case shot:
                            double bulletSpeed = 20.0;
                            double bulletAngularSpeed = 3.0;
                            double bulletDamage = 21.0;

                            Bullet bullet = new Bullet(spaceShip.Pos, spaceShip.Angle, bulletSpeed,
                                bulletAngularSpeed, new Health(), bulletDamage);
                            world.Bullets.Add(bullet.Id, bullet);

                            data = bullet.ToString(separator);

                            Write(stream, data);
                            Log(String.Format("Sent: {0}", data));

                            SendToAllApartSender(client, command);
                            Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                            SendToAllApartSender(client, data);
                            Log(String.Format("SentToAllApartSender: {0}", data));

                            break;
                        case hit:
                            data = Read(stream);
                            Log(String.Format("Received: {0}", data));

                            parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            int bulletId = Convert.ToInt32(parseData[0]);
                            int burnObjectId = Convert.ToInt32(parseData[1]);

                            if (!world.Bullets.ContainsKey(bulletId) || burnObjectId != -1
                                && !world.Ships.ContainsKey(burnObjectId) && !world.Asteroids.ContainsKey(burnObjectId))
                            {
                                continue;
                            }

                            SendToAll(command);
                            Log(String.Format("SentToAll command: {0}", DecryptCommand(command)));

                            data += separator;

                            if (burnObjectId != World.borderId)
                            {
                                if (world.Ships.ContainsKey(burnObjectId))
                                {
                                    world.Ships[burnObjectId].Health.Point -= world.Bullets[bulletId].Damage;

                                    if (world.Ships[burnObjectId].Health.Point <= 0)
                                    {
                                        data += World.objectDestroyed;
                                        world.Ships.Remove(burnObjectId);
                                    }
                                    else
                                    {
                                        data += World.objectSurvived;
                                    }
                                }
                                else if (world.Asteroids.ContainsKey(burnObjectId))
                                {
                                    world.Asteroids[burnObjectId].Health.Point -= world.Bullets[bulletId].Damage;

                                    if (world.Asteroids[burnObjectId].Health.Point <= 0)
                                    {
                                        data += World.objectDestroyed;
                                        world.Asteroids.Remove(burnObjectId);
                                    }
                                    else
                                    {
                                        data += World.objectSurvived;
                                    }
                                }
                            }

                            SendToAll(data);
                            Log(String.Format("SentToAll: {0}", data));

                            world.Bullets.Remove(bulletId);

                            break;
                        case Brain.collision:
                            data = Read(stream);
                            Log(String.Format("Received: {0}", data));

                            int asteroidId = Convert.ToInt32(data);

                            if (!world.Asteroids.ContainsKey(asteroidId))
                            {
                                continue;
                            }

                            SendToAll(command);
                            Log(String.Format("SentToAll command: {0}", DecryptCommand(command)));

                            data = spaceShip.Id.ToString() + separator + data + separator;

                            double decreaseCoef = 1.0 / 3.0;
                            world.Ships[spaceShip.Id].Health.Point -= world.Ships[spaceShip.Id].Health.MaxPoint * decreaseCoef;

                            if (world.Ships[spaceShip.Id].Health.Point <= 0)
                            {
                                data += World.objectDestroyed;
                                world.Ships.Remove(spaceShip.Id);
                            }
                            else
                            {
                                data += World.objectSurvived;
                            }

                            SendToAll(data);
                            Log(String.Format("SentToAll: {0}", data));

                            world.Asteroids.Remove(asteroidId);

                            break;
                        case close:
                            data = Read(stream);
                            Log(String.Format("Received: {0}", data));

                            parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            int count = Convert.ToInt32(parseData[0]);
                            for (int i = 1; count-- > 0; ++i)
                            {
                                int id = Convert.ToInt32(parseData[i]);
                                world.Bullets.Remove(id);
                            }

                            SendToAllApartSender(client, command);
                            Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                            data = spaceShip.Id.ToString() + separator + data;

                            SendToAllApartSender(client, data);
                            Log(String.Format("SentToAllApartSender: {0}", data));

                            world.Ships.Remove(spaceShip.Id);

                            return;
                        default:
                            return;

                        //break;
                    }
                }
            }
            catch (IOException e)
            {
                Log(String.Format("IOException: {0}", e.Message));
            }
            catch (SocketException e)
            {
                Log(String.Format("SocketException: {0}", e.Message));
            }
            catch (Exception e)
            {
                Log(String.Format("Exception: {0}", e.Message));
            }
            finally
            {
                Log("\nThread terminated...");

                nerves[client].Value.Close();
                nerves[client].Key.Close();

                nerves.Remove(client);

                stream.Close();
                client.Close();

                if (world.Ships.ContainsKey(spaceShip.Id))
                {
                    world.Ships.Remove(spaceShip.Id);
                }
                shipSocketDict.Remove(spaceShip);
            }
        }

        private SpaceShip InitWorld(Socket nerve, NetworkStream stream, Byte command, String separator)
        {
            double shipX = 250.0;
            double shipY = 300.0;
            int shipAngle = 0;
            double shipSpeed = 7.0;
            double shipAngularSpeed = 3.0;
            SpaceShip spaceShip = new SpaceShip(new Point(shipX, shipY), shipAngle, shipSpeed,
                shipAngularSpeed, new Health(), new Gun(new Health()));

            SendToAllApartSender(nerve, command);
            Log(String.Format("SendToAllApartSender command: {0}", command));

            String data = spaceShip.ToString(separator, true);

            SendToAllApartSender(nerve, data);
            Log(String.Format("SendToAllApartSender: {0}", data));

            data = spaceShip.ToString(separator);

            data += separator + world.Ships.Count + separator;
            foreach (var ship in world.Ships.Values)
            {
                data += ship.ToString(separator, true) + separator;
            }

            data += world.Asteroids.Count + separator;
            foreach (var asteroid in world.Asteroids.Values)
            {
                data += asteroid.ToString(separator) + separator +
                    asteroid.Type.GetHashCode() + separator;
            }

            data += world.Bullets.Count;
            foreach (var bullet in world.Bullets.Values)
            {
                data += separator + bullet.ToString(separator);
            }

            Write(stream, data);
            Log(String.Format("Send: {0}", data));

            return spaceShip;
        }

        public static String DecryptCommand(Byte command)
        {
            String decryptedCommand = String.Empty;
            switch (command)
            {
                case sendtoall:
                    decryptedCommand = "sendtoall";
                    break;
                case poschange:
                    decryptedCommand = "poschange";
                    break;
                case angchange:
                    decryptedCommand = "angchange";
                    break;
                case shot:
                    decryptedCommand = "shot";
                    break;
                case hit:
                    decryptedCommand = "hit";
                    break;
                case collision:
                    decryptedCommand = "collision";
                    break;
                case close:
                    decryptedCommand = "close";
                    break;
                case newspace:
                    decryptedCommand = "newspace";
                    break;
                default:
                    decryptedCommand = "unknown";
                    break;
            }

            return decryptedCommand;
        }

        string Read(NetworkStream stream)
        {
            String data = String.Empty;
            Byte[] bytes = new Byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data += System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);

                if (!stream.DataAvailable)
                {
                    break;
                }
            }

            return data;
        }

        Byte ReadByte(NetworkStream stream)
        {
            Byte data = new Byte();
            try
            {
                data = Convert.ToByte(stream.ReadByte());
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
                data = unknown;
            }

            return data;
        }

        void Write(NetworkStream stream, String data)
        {
            Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);

            if (stream.CanWrite && !stream.DataAvailable)
            {
                stream.Write(bytes, 0, data.Length);
                stream.Flush();
            }
        }

        void WriteByte(NetworkStream stream, Byte data)
        {
            if (stream.CanWrite && !stream.DataAvailable)
            {
                stream.WriteByte(data);
                stream.Flush();
            }
        }

        /*void SendPolicyFile(NetworkStream stream)
        {
            Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(policy);
            if (stream.CanWrite)
            {
                stream.Write(bytes, 0, policy.Length);
                stream.Flush();
            }

        }*/

        void SendToAllApartSender(Socket sender, String data)
        {
            Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);

            foreach (var nerve in nerves)
            {
                if (nerve.Key != sender)
                {
                    nerve.Value.Value.Write(bytes, 0, data.Length);
                }
            }
        }

        void SendToAllApartSender(Socket sender, Byte data)
        {
            foreach (var nerve in nerves)
            {
                if (nerve.Key != sender)
                {
                    nerve.Value.Value.WriteByte(data);
                }
            }
        }

        void SendToAll(String data)
        {
            Byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);

            foreach (var nerve in nerves.Values)
            {
                nerve.Value.Write(bytes, 0, data.Length);
            }
        }

        void SendToAll(Byte data)
        {
            foreach (var nerve in nerves.Values)
            {
                nerve.Value.WriteByte(data);
            }
        }

        void Log(String data)
        {
            Console.WriteLine("{0}", data);
        }

        void Log(String data, bool consoleOnly)
        {
            if (consoleOnly)
            {
                Log(data);
                return;
            }
            using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(data + '\n');
                fs.Write(info, 0, info.Length);
            }
        }

        void Log(String data, bool usingConsole, bool usingFile)
        {
            if (usingConsole)
            {
                Log(data);
            }

            if (usingFile)
            {
                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(data + '\n');
                    fs.Write(info, 0, info.Length);
                }
            }
        }
    }
}
