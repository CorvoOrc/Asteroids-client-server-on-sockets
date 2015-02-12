/*Steshenko Alexander*/

/*
Client for testing is oriented on check work event function. Event functions:

- newspace (appearence new space ship)
- sendtoall (send message to all nerve)
- poschange (user push on control buttom and move space ship)
- angchange (user push on control buttom and turn space ship)
- shot (user shot, appearence new bullet)
- hit (user hit in somebody object: another space ship, asteroid, border)
- collision (user`s space ship collision with asteroid (in future with another space ship))
- close (user terminated game and close session, need free space ship object and own bullets, also close sockets and streams)
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

namespace Nerve
{
    class Nerve
    {
        NerveSynchro synchro = null;

        World world = null;

        Socket client = null;
        Socket clientUpd = null;

        const Byte sendtoall = 1;
        const Byte poschange = 2;
        const Byte angchange = 3;
        const Byte shot = 4;
        const Byte hit = 5;
        const Byte collision = 6;
        const Byte close = 7;
        const Byte newspace = 8;
        const Byte unknown = 9;

        const int unused = 0; // Monitored on server side (by Brain)
       
        String separator = ">>";
        String logPath = "logfile";

        public Nerve()
        {
            InitNerve();
        }

        public Nerve(String logPath_)
        {
            InitNerve();
            this.logPath = logPath_;
        }

        void InitNerve()
        {
            synchro = new NerveSynchro();

            world = new World();

            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientUpd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(String server, Int32 port, Int32 portUdp)
        {
            NetworkStream stream = null;

            try
            {
                client.Connect(server, port);
                stream = new NetworkStream(client);

                clientUpd.Connect(server, portUdp);

                Thread thread = new Thread(new ParameterizedThreadStart(UpdateService));
                thread.Start((object)clientUpd); //don`t check in try/catch. This is useless

                String message = Read(stream);
                Log(String.Format("Received: {0}", message));

                synchro.WaitAll();
                //lock all
                synchro.ResetAll();
                InitWorld(message, separator);
                synchro.SetAll();
                //unlock all

                string[] parseData;

                while (true)
                {
                    message = Console.ReadLine();// Read command from console
                    if (message == "")
                    {
                        continue;
                    }

                    Byte command = GetCommand(message);

                    switch (command)
                    {
                        case sendtoall:
                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            message = Console.ReadLine();

                            Write(stream, message);
                            Log(String.Format("Sent: {0}", message));

                            break;
                        case poschange:
                            if (world.gameOver)
                            {
                                continue; // in AS3 in function call return;
                            }

                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            synchro.WaitOne(GroupLock.shipsLock);
                            //lock shipsLock
                            synchro.Reset(GroupLock.shipsLock);
                            message = Console.ReadLine();
                            message += separator + Console.ReadLine();
                            synchro.Set(GroupLock.shipsLock);
                            //unlock shipsLock

                            Write(stream, message);
                            Log(String.Format("Sent: {0}", message));

                            parseData = message.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            double x = Convert.ToDouble(parseData[0]);
                            double y = Convert.ToDouble(parseData[1]);

                            synchro.WaitOne(GroupLock.shipsLock);
                            //lock shipLock
                            synchro.Reset(GroupLock.shipsLock);
                            world.Ships[world.MyId].Move(new Point(x, y));
                            synchro.Set(GroupLock.shipsLock);
                            //unlock shipLock

                            break;
                        case angchange:
                            if (world.gameOver)
                            {
                                continue; // in AS3 function return;
                            }

                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            synchro.WaitOne(GroupLock.shipsLock);
                            //lock shipLock
                            synchro.Reset(GroupLock.shipsLock);
                            message = Console.ReadLine();
                            synchro.Set(GroupLock.shipsLock);
                            //unlock shipLock

                            Write(stream, message);
                            Log(String.Format("Sent: {0}", message));

                            int angle = Convert.ToInt32(message);

                            synchro.WaitOne(GroupLock.shipsLock);
                            //lock shipLock
                            synchro.Reset(GroupLock.shipsLock);
                            world.Ships[world.MyId].Turn(angle);
                            synchro.Set(GroupLock.shipsLock);
                            //unlock shipLock

                            break;
                        case shot:
                            if (world.gameOver)
                            {
                                continue; // in AS3 function event return;
                            }

                            synchro.WaitOne(GroupLock.shipsLock);
                            //lock shipsLock
                            synchro.Reset(GroupLock.shipsLock);
                            if (!world.Ships[world.MyId].Gun.Ready())
                            {
                                Log(String.Format("Gun isn`t ready for fire"));
                                continue; // in AS3 function event return;
                            }
                            synchro.Set(GroupLock.shipsLock);
                            //unlock shipsLock

                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            message = Read(stream);
                            Log(String.Format("Received: {0}", message));

                            parseData = message.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            int id = Convert.ToInt32(parseData[0]);
                            double bulletX = Convert.ToDouble(parseData[1]);
                            double bulletY = Convert.ToDouble(parseData[2]);
                            int bulletAngle = Convert.ToInt32(parseData[3]);
                            double bulletSpeed = Convert.ToDouble(parseData[4]);
                            double bulletAngularSpeed = Convert.ToDouble(parseData[5]);

                            Bullet bullet = new Bullet(id, new Point(bulletX, bulletY), bulletAngle,
                                bulletSpeed, bulletAngularSpeed, new Health(), unused);

                            synchro.WaitOne(GroupLock.ownBulletsLock);
                            //lock ownBulletsLock
                            synchro.Reset(GroupLock.ownBulletsLock);
                            world.ShotOwn(id, bullet);
                            synchro.Set(GroupLock.ownBulletsLock);
                            //unlock ownBulletsLock

                            break;
                        case hit:
                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            synchro.WaitOne(GroupLock.ownBulletsLock);
                            //lock ownBulletsLock
                            synchro.Reset(GroupLock.ownBulletsLock);
                            int bulletId = Convert.ToInt32(Console.ReadLine()); // loop ownBullets
                            int burnObjectId = Convert.ToInt32(Console.ReadLine()); // abroad == -1 (borderId)

                            world.UseBullet(bulletId);
                            synchro.Set(GroupLock.ownBulletsLock);
                            //unlock ownBulletsLock

                            message = bulletId.ToString() + separator + burnObjectId;

                            Write(stream, message);
                            Log(String.Format("Sent: {0}", message));

                            break;
                        case collision:
                            if (world.gameOver)
                            {
                                continue; // in AS3 in function call return;
                            }

                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            synchro.WaitOne(GroupLock.asteroidsLock);
                            //lock asteroidsLock
                            synchro.Reset(GroupLock.asteroidsLock);
                            int asteroidId = Convert.ToInt32(Console.ReadLine()); // abroad == -1
                            synchro.Set(GroupLock.asteroidsLock);
                            //unlock asteroidsLock

                            message = asteroidId.ToString();

                            Write(stream, message);
                            Log(String.Format("Sent: {0}", message));

                            break;
                        case close:
                            WriteByte(stream, command);
                            Log(String.Format("Sent command: {0}", DecryptCommand(command)));

                            synchro.WaitOne(GroupLock.shipsLock);
                            //lock shipsLock
                            synchro.Reset(GroupLock.shipsLock);
                            world.ExplodeShip(world.MyId);
                            synchro.Set(GroupLock.shipsLock);
                            //unlock shipsLock

                            synchro.WaitOne(GroupLock.ownBulletsLock);
                            //lock ownBulletsLock
                            synchro.Reset(GroupLock.ownBulletsLock);
                            message = world.OwnBullets.Count.ToString();
                            foreach (var ownBulletId in world.OwnBullets.Keys)
                            {
                                message += separator + ownBulletId;
                            }
                            synchro.Set(GroupLock.ownBulletsLock);
                            //unlock ownBulletsLock

                            Write(stream, message);
                            Log(String.Format("Sent: {0}", message));

                            return;
                        default:
                            return;
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Log(String.Format("ArgumentNullException: {0}", e.Message));
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
                stream.Close();
                client.Close();

                Log("\n Terminated...");
            }
        }

        void UpdateService(Object clientUpdObj)
        {
            NetworkStream streamUpd = null;

            try
            {
                streamUpd = new NetworkStream((Socket)clientUpdObj);

                Log("Upd socket Connected!");

                while (true)
                {
                    String data = null;

                    Byte command = ReadByte(streamUpd);
                    Log(String.Format("Received upd command: {0}", DecryptCommand(command)));

                    switch (command)
                    {
                        case poschange:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int id = Convert.ToInt32(parseData[0]);
                                double x = Convert.ToDouble(parseData[1]);
                                double y = Convert.ToDouble(parseData[2]);

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.Ships[id].Move(new Point(x, y));
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                break;
                            }
                        case angchange:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int id = Convert.ToInt32(parseData[0]);
                                int angle = Convert.ToInt32(parseData[1]);

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.Ships[id].Turn(angle);
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                break;
                            }
                        case shot:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int id = Convert.ToInt32(parseData[0]);
                                double x = Convert.ToDouble(parseData[1]);
                                double y = Convert.ToDouble(parseData[2]);
                                int angle = Convert.ToInt32(parseData[3]);
                                double speed = Convert.ToDouble(parseData[4]);
                                double angularSpeed = Convert.ToDouble(parseData[5]);

                                Bullet bullet = new Bullet(new Point(x, y), angle, speed, angularSpeed, new Health(), unused);

                                synchro.WaitOne(GroupLock.alienBulletsLock);
                                //lock alienBulletsLock
                                synchro.Reset(GroupLock.alienBulletsLock);
                                world.ShotAlien(id, bullet);
                                synchro.Set(GroupLock.alienBulletsLock);
                                //unlock alienBulletsLock

                                break;
                            }
                        case hit:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int bulletId = Convert.ToInt32(parseData[0]);
                                int burnObjectId = Convert.ToInt32(parseData[1]);
                                bool objectDestroyed = Convert.ToBoolean(Convert.ToByte(parseData[2]));

                                if (objectDestroyed)
                                {
                                    //lock shipsLock
                                    //lock asteroidsLock
                                    synchro.WaitOne(GroupLock.shipsLock);
                                    synchro.Reset(GroupLock.shipsLock);
                                    synchro.WaitOne(GroupLock.asteroidsLock);
                                    synchro.Reset(GroupLock.asteroidsLock);
                                    if(world.IsShip(burnObjectId))
                                    {
                                        if(world.IsMyShip(burnObjectId))
                                        {
                                            world.gameOver = true;
                                            Log("GAME OVER");
                                        }

                                        world.ExplodeShip(burnObjectId);
                                    }
                                    else if (world.IsAsteroid(burnObjectId))
                                    {
                                        world.CrushAsteroid(burnObjectId, 0);
                                    }
                                    synchro.Set(GroupLock.shipsLock);
                                    synchro.Set(GroupLock.asteroidsLock);
                                    //unlock shipsLock
                                    //unlock asteroidLock
                                }

                                synchro.WaitOne(GroupLock.alienBulletsLock);
                                //lock alienBulletsLock
                                synchro.Reset(GroupLock.alienBulletsLock);
                                if (world.IsAlienBullet(bulletId))
                                {
                                    world.UseBullet(bulletId);
                                }
                                synchro.Set(GroupLock.alienBulletsLock);
                                //unlock alienBulletsLock

                                break;
                            }
                        case collision:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int shipId = Convert.ToInt32(parseData[0]);
                                int asteroidId = Convert.ToInt32(parseData[1]);
                                bool objectDestroyed = Convert.ToBoolean(Convert.ToByte(parseData[2]));

                                synchro.WaitOne(GroupLock.asteroidsLock);
                                //lock asteroidsLock
                                synchro.Reset(GroupLock.asteroidsLock);
                                world.CrushAsteroid(asteroidId, 0);
                                synchro.Set(GroupLock.asteroidsLock);
                                //unlock asteroidsLock

                                if (objectDestroyed)
                                {
                                    if (world.IsMyShip(shipId))
                                    {
                                        world.gameOver = true;
                                        Log("GAME OVER");
                                    }

                                    synchro.WaitOne(GroupLock.shipsLock);
                                    //lock shipsLock
                                    synchro.Reset(GroupLock.shipsLock);
                                    world.ExplodeShip(shipId);
                                    synchro.Set(GroupLock.shipsLock);
                                    //unlock shipsLock
                                }

                                break;
                            }
                        case close:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int shipId = Convert.ToInt32(parseData[0]);
                                int count = Convert.ToInt32(parseData[1]);

                                if (world.IsMyShip(shipId))
                                {
                                    return;
                                }

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.ExplodeShip(shipId);
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                synchro.WaitOne(GroupLock.alienBulletsLock);
                                //lock alienLock
                                synchro.Reset(GroupLock.alienBulletsLock);
                                for (int i = 2; count-- > 0; ++i)
                                {
                                    int bulletId = Convert.ToInt32(parseData[i]);
                                    world.UseBullet(bulletId);
                                }
                                synchro.Set(GroupLock.alienBulletsLock);
                                //unlock alienLock

                                break;
                            }
                        case newspace:
                            {
                                data = Read(streamUpd);
                                Log(String.Format("Received upd: {0}", data));

                                string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                int id = Convert.ToInt32(parseData[0]);
                                double x = Convert.ToDouble(parseData[1]);
                                double y = Convert.ToDouble(parseData[2]);
                                int angle = Convert.ToInt32(parseData[3]);
                                /*double speed = Convert.ToDouble(parseData[4]);
                                double angularSpeed = Convert.ToDouble(parseData[5]);
                                double point = Convert.ToDouble(parseData[6]);
                                double maxPoint = Convert.ToDouble(parseData[7]);
                                double prepareTime = Convert.ToDouble(parseData[8]);*/

                                SpaceShip ship = new SpaceShip(id, new Point(x, y), angle,
                                    unused, unused, new Health(), new Gun(new Health()));

                                synchro.WaitOne(GroupLock.shipsLock);
                                //lock shipsLock
                                synchro.Reset(GroupLock.shipsLock);
                                world.AddShip(id, ship);
                                synchro.Set(GroupLock.shipsLock);
                                //unlock shipsLock

                                break;
                            }
                        default:
                            {
                                return;
                            }
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
            finally
            {
                streamUpd.Close();
                clientUpd.Close();

                Log("\nThread for update terminated...");
            }

            return;
        }

        void InitWorld(String data, String separator)
        {
            string[] parseData = data.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            world.MyId = Convert.ToInt32(parseData[i++]);
            double myX = Convert.ToDouble(parseData[i++]);
            double myY = Convert.ToDouble(parseData[i++]);
            int myAngle = Convert.ToInt32(parseData[i++]);
            double mySpeed = Convert.ToDouble(parseData[i++]);
            double myAngularSpeed = Convert.ToDouble(parseData[i++]);

            SpaceShip ship = new SpaceShip(world.MyId, new Point(myX, myY), myAngle, mySpeed, myAngularSpeed,
                new Health(), new Gun(new Health(0, 0)));
            world.AddShip(world.MyId, ship);

            int countShips = Convert.ToInt32(parseData[i++]);
            for (; countShips-- > 0; i += 4)
            {
                int id = Convert.ToInt32(parseData[i]);
                double x = Convert.ToDouble(parseData[i + 1]);
                double y = Convert.ToDouble(parseData[i + 2]);
                int angle = Convert.ToInt32(parseData[i + 3]);

                SpaceShip alienShip = new SpaceShip(id, new Point(x, y), angle,
                    unused, unused, new Health(), new Gun(new Health()));
                world.AddShip(id, alienShip);
            }

            int countAsteroids = Convert.ToInt32(parseData[i++]);
            for (; countAsteroids-- > 0; i += 7)
            {
                int id = Convert.ToInt32(parseData[i]);
                double x = Convert.ToDouble(parseData[i + 1]);
                double y = Convert.ToDouble(parseData[i + 2]);
                int angle = Convert.ToInt32(parseData[i + 3]);
                double speed = Convert.ToDouble(parseData[i + 4]);
                double angularSpeed = Convert.ToDouble(parseData[i + 5]);
                //double point = Convert.ToDouble(parseData[i + 6]);
                //double maxPoint = Convert.ToDouble(parseData[i + 7]);
                AsteroidType type = new AsteroidType();
                switch (Convert.ToInt32(parseData[i + 6]))
                {
                    case 0:
                        {
                            type = AsteroidType.big;
                            break;
                        }
                    case 1:
                        {
                            type = AsteroidType.medium;
                            break;
                        }
                    case 2:
                        {
                            type = AsteroidType.small;
                            break;
                        }
                }

                Asteroid asteroid = new Asteroid(id, new Point(x, y), angle, speed, angularSpeed, new Health(), type);
                world.OrganizeArmageddon(id, asteroid);
            }

            int countBullets = Convert.ToInt32(parseData[i++]);
            for (; countBullets-- > 0; i += 6)
            {
                int id = Convert.ToInt32(parseData[i]);
                double x = Convert.ToDouble(parseData[i + 1]);
                double y = Convert.ToDouble(parseData[i + 2]);
                int angle = Convert.ToInt32(parseData[i + 3]);
                double speed = Convert.ToDouble(parseData[i + 4]);
                double angularSpeed = Convert.ToDouble(parseData[i + 5]);

                Bullet bullet = new Bullet(id, new Point(x, y), angle, speed, angularSpeed, new Health(), unused);
                world.ShotAlien(id, bullet);
            }
        }

        public static Byte GetCommand(String commandStr)
        {
            Byte command = new Byte();
            switch (commandStr)
            {
                case "sendtoall":
                    command = sendtoall;
                    break;
                case "poschange":
                    command = poschange;
                    break;
                case "angchange":
                    command = angchange;
                    break;
                case "shot":
                    command = shot;
                    break;
                case "hit":
                    command = hit;
                    break;
                case "collision":
                    command = collision;
                    break;
                case "close":
                    command = close;
                    break;
                case "newspace":
                    command = newspace;
                    break;
                default:
                    command = unknown;
                    break;
            }

            return command;
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
    }
}
