using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace RobotControllerAPI
{
    static class RobotIO
    {

        public static String Recv(String ip, Int32 port, String cmd)
        {
            var client = new TcpClient(ip, port);
            var stream = client.GetStream();
            __Send(stream, Encoding.ASCII.GetBytes(cmd));
            var data = __Recv(stream);
            stream.Close();
            client.Close();
            return data;
        }

        public static void Send(String ip, Int32 port, String cmd)
        {
            var client = new TcpClient(ip, port);
            var stream = client.GetStream();
            __Send(stream, Encoding.ASCII.GetBytes(cmd));
            stream.Close();
            client.Close();
        }

        public static void Send(String ip, Int32 port, String cmd, Byte[] data)
        {
            var client = new TcpClient(ip, port);
            var stream = client.GetStream();
            __Send(stream, Encoding.ASCII.GetBytes(cmd));
            __Send(stream, data);
            stream.Close();
            client.Close();
        }

        private static String __Recv(NetworkStream stream)
        {
            var size = __RecvSize(stream);
            var data = __RecvData(stream, size);
            return Encoding.ASCII.GetString(data, 0, size);
        }

        private static Int32 __RecvSize(NetworkStream stream)
        {
            var bSize = new Byte[4];
            stream.Read(bSize, 0, 4);
            Array.Reverse(bSize);
            return BitConverter.ToInt32(bSize, 0);
        }

        private static Byte[] __RecvData(NetworkStream stream, Int32 size)
        {
            var chunks = new Byte[size];
            var byteRecved = 0;
            while (byteRecved < size)
            {
                var tempSize = stream.Read(chunks, byteRecved, size - byteRecved);
                if (tempSize == 0)
                {
                    return chunks;
                }
                byteRecved += tempSize;
            }
            return chunks;
        }

        private static void __Send(NetworkStream stream, Byte[] data)
        {
            var size = data.Length;
            var bSize = BitConverter.GetBytes(size);
            Array.Reverse(bSize);
            stream.Write(bSize);
            stream.Write(data);
        }

    }

    public static class RobotOP
    {
        public static IDictionary<String, Int32> ReadAxes(String ip, Int32 port)
        {
            var data = RobotIO.Recv(ip, port, "read_axes");
            var servoMap = JsonSerializer.Deserialize<IDictionary<String, Int32>>(data);
            return servoMap;
        }

        public static void PlayPose(String ip, Int32 port, Pose pose)
        {
            var poseJson = JsonSerializer.Serialize<Pose>(pose);
            var data = Encoding.ASCII.GetBytes(poseJson);
            RobotIO.Send(ip, port, "play_pose", data);
        }

        public static void StopPose(String ip, Int32 port)
        {
            RobotIO.Send(ip, port, "stop_pose");
        }

        public static void PlayMotion(String ip, Int32 port, IList<Pose> motion)
        {
            var motionJson = JsonSerializer.Serialize<IList<Pose>>(motion);
            var data = Encoding.ASCII.GetBytes(motionJson);
            RobotIO.Send(ip, port, "play_motion", data);
        }

        public static void StopMotion(String ip, Int32 port)
        {
            RobotIO.Send(ip, port, "stop_motion");
        }

        public static void PlayIdleMotion(String ip, Int32 port, Double speed, Int32 pause)
        {
            var dict = new
            {
                Speed = speed,
                Pause = pause,
            };
            var paramStr = JsonSerializer.Serialize(dict);
            var data = Encoding.ASCII.GetBytes(paramStr);
            RobotIO.Send(ip, port, "play_idle_motion", data);
        }

        public static void StopIdleMotion(String ip, Int32 port)
        {
            RobotIO.Send(ip, port, "stop_idle_motion");
        }

        public static void PlayWav(String ip, Int32 port, String wavFile)
        {
            using (FileStream fs = File.Open(wavFile, FileMode.Open, FileAccess.Read))
            {
                var info = new FileInfo(wavFile);
                var data = new byte[info.Length];
                fs.Read(data, 0, (int)info.Length);
                RobotIO.Send(ip, port, "play_wav", data);
            }
        }

        public static void StopWav(String ip, Int32 port)
        {
            RobotIO.Send(ip, port, "stop_wav");
        }

        public static IList<Pose> MakeBeatMotion(Int32 duration, IList<IDictionary<String, Int32>> servoMapList, IDictionary<String, Int32> endServoMap, Double speed)
        {
            var msec = (int)(1000 / speed);
            var size = (int)(duration / msec);
            var motion = new List<Pose>();
            IDictionary<String, Int32> prev = new Dictionary<String, Int32>();
            for (int i = 0; i < size; i++)
            {
                var servoMap = __Choose(prev, servoMapList);
                var pose = new Pose
                {
                    Msec = msec,
                    ServoMap = servoMap
                };
                motion.Add(pose);
                prev = servoMap;
            }
            var endPose = new Pose
            {
                Msec = 1000,
                ServoMap = endServoMap
            };
            motion.Add(endPose);
            return motion;
        }

        private static IDictionary<String, Int32> __Choose(IDictionary<String, Int32> prev, IList<IDictionary<String, Int32>> servoMapList)
        {
            while (true)
            {
                var random = new Random();
                var map = servoMapList[random.Next(servoMapList.Count)];
                if (map != prev)
                {
                    return map;
                }
            }
        }
    }

    public class Pose
    {
        public Int32 Msec { get; set; }
        public IDictionary<String, Int32> ServoMap { get; set; }
    }
}
