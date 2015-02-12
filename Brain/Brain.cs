/*Steshenko Alexander*/

/*
Server(Brain) handled commands, sent from client(Nerve)
For more information:
https://github.com/CorvoOrc/Asteroids-client-server-on-sockets/blob/master/README.md
*/

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
        BrainSynchro synchro = null;

        World world = null;

        Dictionary<Socket, KeyValuePair<Socket, NetworkStream>> nerves = null;
        Dictionary<SpaceShip, Socket> shipSocketBind = null;

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
            //address = address_; // listen
            this.port = port_;
            this.portBroad = port + 10;

            InitBrain();

            this.separator = separatorDefault;
            this.logPath = logPathDefault;
        }

        public Brain(int port_, String separator_)
        {
            //address = address_; // listen
            this.port = port_;
            this.portBroad = port + 10;

            InitBrain();

            this.separator = separator_;
            this.logPath = logPathDefault;
        }

        public Brain(int port_, String separator_, String logPath_)
        {
            //address = address_; // listen
            this.port = port_;
            this.portBroad = port + 10;

            InitBrain();

            this.separator = separator_;
            this.logPath = logPath_;
        }

        private void InitBrain()
        {
            try
            {
                BrainSingleton.TryCheck();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            this.world = new World();

            this.backlog = 50;
            this.sleepTime = 50;

            /*policy =
@"<?xml version=""1.0""?>
<cross-domain-policy>
    <site-control permitted-cross-domain-policies=""master-only\""/>
    <allow-access-from domain=""" + address + @""" to-ports=""10000-15000, 843""/>
</cross-domain-policy>";*/

            this.nerves = new Dictionary<Socket, KeyValuePair<Socket, NetworkStream>>();
            this.shipSocketBind = new Dictionary<SpaceShip, Socket>();

            this.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            this.server.Bind(new IPEndPoint(IPAddress.Any, port));
            this.server.Listen(backlog);

            this.broadcast.Bind(new IPEndPoint(IPAddress.Any, portBroad));
            this.broadcast.Listen(backlog);

            synchro = new BrainSynchro();
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

                    synchro.WaitOne(GroupLock.nervesLock);
                    //lock nervesLock
                    synchro.Reset(GroupLock.nervesLock);
                    nerves.Add(client, new KeyValuePair<Socket, NetworkStream>(sender, streamSender));
                    synchro.Set(GroupLock.nervesLock);
                    //unlock nervesLock

                    Thread thread = new Thread(new ParameterizedThreadStart(Processing));
                    object clientObj = client as object;

                    thread.Start(clientObj); //don`t check in try/catch. This is useless
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
                synchro.WaitAll();                
                //lock all
                synchro.ResetAll();
                spaceShip = InitWorld(client, stream, newspace, separator);
                shipSocketBind.Add(spaceShip, client);
                world.AddShip(spaceShip.Id, spaceShip);
                synchro.SetAll();
                //unlock all
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\nInit world is failed");
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
                            {
                                data = Read(stream);
                                Log(String.Format("Received: {0}", data));

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAllApartSender(client, data);
                                Log(String.Format("Received and SendToAllApartSender: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //unlock senttoallLock

                                break;
                            }
                        case poschange:
                            {
                                data = Read(stream);
                                Log(String.Format("Received: {0}", data));

                                parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                double x = Convert.ToDouble(parseData[0]);
                                double y = Convert.ToDouble(parseData[1]);

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.Ships[spaceShip.Id].Move(new Point(x, y));
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                data = spaceShip.Id + separator + data;

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAllApartSender(client, command);
                                Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                                SendToAllApartSender(client, data);
                                Log(String.Format("SentToAllApartSender: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //unlock sendtoallLock

                                break;
                            }
                        case angchange:
                            {
                                data = Read(stream);
                                Log(String.Format("Received: {0}", data));

                                int angle = Convert.ToInt32(data);

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.Ships[spaceShip.Id].Turn(angle);
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                data = spaceShip.Id + separator + data;

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAllApartSender(client, command);
                                Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                                SendToAllApartSender(client, data);
                                Log(String.Format("SentToAllApartSender: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //lock sendtoallLock

                                break;
                            }
                        case shot:
                            {
                                double bulletSpeed = 20.0;
                                double bulletAngularSpeed = 3.0;
                                double bulletDamage = 21.0;

                                Bullet bullet = new Bullet(spaceShip.Pos, spaceShip.Angle, bulletSpeed,
                                    bulletAngularSpeed, new Health(), bulletDamage);

                                synchro.WaitOne(GroupLock.bulletsLock);
                                //lock bulletsLock
                                synchro.Reset(GroupLock.bulletsLock);
                                world.Shot(bullet.Id, spaceShip.Id, bullet);
                                synchro.Set(GroupLock.bulletsLock);
                                //unlock bulletsLock

                                data = bullet.ToString(separator);

                                Write(stream, data);
                                Log(String.Format("Sent: {0}", data));

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAllApartSender(client, command);
                                Log(String.Format("SentToAllApartSender command: {0}", DecryptCommand(command)));

                                SendToAllApartSender(client, data);
                                Log(String.Format("SentToAllApartSender: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //unlock sendtoallLock

                                break;
                            }
                        case hit:
                            {
                                data = Read(stream);
                                Log(String.Format("Received: {0}", data));

                                parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int bulletId = Convert.ToInt32(parseData[0]);
                                int burnObjectId = Convert.ToInt32(parseData[1]);

                                synchro.WaitOne(GroupLock.bulletsLock);
                                //lock bulletsLock
                                synchro.Reset(GroupLock.bulletsLock);
                                if (!world.IsActuallyExist(bulletId, burnObjectId) || !world.IsBulletOwner(bulletId, spaceShip.Id))
                                {
                                    synchro.Set(GroupLock.bulletsLock);
                                    continue;
                                }
                                synchro.Set(GroupLock.bulletsLock);
                                //unlock bulletsLock

                                data += separator;

                                if (!world.IsBorder(burnObjectId))
                                {
                                    double damage = world.Bullets[bulletId].Damage;

                                    synchro.WaitOne(GroupLock.shipsLock);
                                    synchro.Reset(GroupLock.shipsLock);
                                    synchro.WaitOne(GroupLock.asteroidsLock);
                                    synchro.Reset(GroupLock.asteroidsLock);
                                    //lock shipsLock
                                    //lock asteroidLock
                                    if (world.IsShip(burnObjectId))
                                    {
                                        world.HarmShip(burnObjectId, damage);

                                        if (world.Ships[burnObjectId].Health.isDead())
                                        {
                                            data += World.objectDestroyed;
                                            world.ExplodeShip(burnObjectId);
                                        }
                                        else
                                        {
                                            data += World.objectSurvived;
                                        }
                                    }
                                    else if (world.IsAsteroid(burnObjectId))
                                    {
                                        world.HarmShip(burnObjectId, damage);

                                        if (world.Asteroids[burnObjectId].Health.isDead())
                                        {
                                            data += World.objectDestroyed;
                                            world.CrushAsteroid(burnObjectId, 0);
                                        }
                                        else
                                        {
                                            data += World.objectSurvived;
                                        }
                                    }
                                    synchro.Set(GroupLock.shipsLock);
                                    synchro.Set(GroupLock.asteroidsLock);
                                    //unlock shipsLock
                                    //unlock asteroidLock
                                }

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAll(command);
                                Log(String.Format("SentToAll command: {0}", DecryptCommand(command)));

                                SendToAll(data);
                                Log(String.Format("SentToAll: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //unlock sendtoallLock

                                synchro.WaitOne(GroupLock.bulletsLock);
                                //lock bulletsLock
                                synchro.Reset(GroupLock.bulletsLock);
                                world.UseBullet(bulletId);
                                synchro.Set(GroupLock.bulletsLock);
                                //unlock bulletsLock

                                break;
                            }
                        case collision:
                            {
                                data = Read(stream);
                                Log(String.Format("Received: {0}", data));

                                int asteroidId = Convert.ToInt32(data);

                                synchro.WaitOne(GroupLock.asteroidsLock);
                                //lock asteroidLock
                                synchro.Reset(GroupLock.asteroidsLock);
                                if (!world.IsAsteroid(asteroidId))
                                {
                                    synchro.Set(GroupLock.asteroidsLock);
                                    continue;
                                }
                                synchro.Set(GroupLock.asteroidsLock);
                                //unlock asteroidLock

                                data = spaceShip.Id.ToString() + separator + data + separator;

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                double decreaseCoef = 1.0 / 3.0;
                                double damage = world.Ships[spaceShip.Id].Health.MaxPoint * decreaseCoef;
                                world.HarmShip(spaceShip.Id, damage);

                                if (world.Ships[spaceShip.Id].Health.isDead())
                                {
                                    data += World.objectDestroyed;
                                    world.ExplodeShip(spaceShip.Id);
                                }
                                else
                                {
                                    data += World.objectSurvived;
                                }
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAll(command);
                                Log(String.Format("SentToAll command: {0}", DecryptCommand(command)));

                                SendToAll(data);
                                Log(String.Format("SentToAll: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //unlock sendtoallLock

                                synchro.WaitOne(GroupLock.asteroidsLock);
                                //lock asteroidLock
                                synchro.Reset(GroupLock.asteroidsLock);
                                world.CrushAsteroid(asteroidId, 0);
                                synchro.Set(GroupLock.asteroidsLock);
                                //unlock asteroidLock

                                break;
                            }
                        case close:
                            {
                                data = Read(stream);
                                Log(String.Format("Received: {0}", data));

                                parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int count = Convert.ToInt32(parseData[0]);
                                synchro.WaitOne(GroupLock.bulletsLock);
                                //lock bulletsLock
                                synchro.Reset(GroupLock.bulletsLock);
                                for (int i = 1; count-- > 0; ++i)
                                {
                                    int id = Convert.ToInt32(parseData[i]);
                                    world.UseBullet(id);
                                }
                                synchro.Set(GroupLock.bulletsLock);
                                //unlock bulletsLock

                                data = spaceShip.Id.ToString() + separator + data;

                                synchro.WaitOne(GroupLock.sendtoallLock);
                                //lock sendtoallLock
                                synchro.Reset(GroupLock.sendtoallLock);
                                SendToAll(command);
                                Log(String.Format("SentToAll command: {0}", DecryptCommand(command)));

                                SendToAll(data);
                                Log(String.Format("SentToAll: {0}", data));
                                synchro.Set(GroupLock.sendtoallLock);
                                //unlock sendtoallLock

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.ExplodeShip(spaceShip.Id);
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                return;
                            }
                        default:
                            return;
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
                nerves[client].Value.Close();
                nerves[client].Key.Close();

                synchro.WaitOne(GroupLock.nervesLock);
                //lock nervesLock
                synchro.Reset(GroupLock.nervesLock);
                nerves.Remove(client);
                synchro.Set(GroupLock.nervesLock);
                //unlock nervesLock

                synchro.WaitOne(GroupLock.shipsLock);
                //lock shipsLock
                synchro.Reset(GroupLock.shipsLock);
                if (world.IsShip(spaceShip.Id))
                {
                    world.ExplodeShip(spaceShip.Id);
                }
                synchro.Set(GroupLock.shipsLock);
                //unlock shipsLock

                synchro.WaitOne(GroupLock.shipSocketBindLock);
                //lock shipsSocketBindLock
                synchro.Reset(GroupLock.shipSocketBindLock);
                shipSocketBind.Remove(spaceShip);
                synchro.Set(GroupLock.shipSocketBindLock);
                //unlock shipsSocketBindLock

                stream.Close();
                client.Close();

                Log("\nThread terminated...");
            }
        }

        SpaceShip InitWorld(Socket nerve, NetworkStream stream, Byte command, String separator)
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
