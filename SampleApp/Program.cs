using System;
using System.Collections.Generic;
using System.Threading;
using RobotControllerAPI;


namespace SampleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            TestForPuppet();
        }

        static void TestForPuppet()
        {
            String ip = "10.10.12.174";
            Int32 port = 22222;

            Console.WriteLine("現在の間接角度を取得する。");
            var axes = RobotOP.ReadAxes(ip, port);
            foreach (String key in axes.Keys)
            {
                Console.WriteLine("Axis[{0}]={1}", key, axes[key]);
            }
            Thread.Sleep(2000);


            Console.WriteLine("1秒間で左手を上げる。");
            var servoMap = new Dictionary<String, Int32>() { { "L_ELBO", 60 } };
            var pose = new Pose { Msec = 1000, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(2000);


            Console.WriteLine("0.5秒間で右手を上げる。");
            servoMap = new Dictionary<String, Int32>() { { "R_ELBO", -60 } };
            pose = new Pose { Msec = 500, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(2000);


            Console.WriteLine("1秒間で頭と両手を下ろし、反時計回りに30度回転にする。");
            servoMap = new Dictionary<String, Int32>()
            {
                { "L_ELBO", -70 },
                { "R_ELBO",  70 },
                { "HEAD_P", -70 },
                { "BODY_Y", -60 }
            };
            pose = new Pose { Msec = 1000, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(2000);


            Console.WriteLine("ポーズをリセットする。");
            servoMap = new Dictionary<String, Int32>()
            {
                { "HEAD_R",   0 },
                { "HEAD_P",   0 },
                { "L_ELBO",   0 },
                { "R_ELBO",   0 },
                { "BODY_Y",   0 }
            };
            pose = new Pose { Msec = 1000, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(2000);


            Console.WriteLine("1秒間で左手をあげる動作を0.3秒後に止める。");
            servoMap = new Dictionary<String, Int32>() { { "L_ELBO", 60 } };
            pose = new Pose { Msec = 1000, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(300);
            RobotOP.StopPose(ip, port);
            Thread.Sleep(2000);

            Console.WriteLine("モーション（ポーズのリスト）を実行する。");
            var nodMotion = new List<Pose>()
            {
                new Pose { Msec = 250, ServoMap = new Dictionary<String, Int32>() { { "R_ELBO", 40 }, { "HEAD_P",   0 }, { "L_ELBO", -40 } } },
                new Pose { Msec = 250, ServoMap = new Dictionary<String, Int32>() { { "R_ELBO", 70 }, { "HEAD_P", -70 }, { "L_ELBO", -70 } } },
                new Pose { Msec = 250, ServoMap = new Dictionary<String, Int32>() { { "R_ELBO", 40 }, { "HEAD_P",   0 }, { "L_ELBO", -40 } } },
            };
            RobotOP.PlayMotion(ip, port, nodMotion);
            Thread.Sleep(2000);


            Console.WriteLine("モーションを0.25秒後に止める。");
            RobotOP.PlayMotion(ip, port, nodMotion);
            Thread.Sleep(250);
            RobotOP.StopMotion(ip, port);
            Thread.Sleep(2000);


            Console.WriteLine("アイドル動作を実行する（スピード1.0, 間隔1000ミリ秒）。");
            RobotOP.PlayIdleMotion(ip, port, 1.0, 1000);
            Thread.Sleep(10000);


            Console.WriteLine("アイドル動作を停止する。");
            RobotOP.StopIdleMotion(ip, port);


            Console.WriteLine("5秒のビートジェスチャ（スピード1.0）を生成し実行する。");
            var endServoMap = new Dictionary<String, Int32>() { { "R_SHOU", 90 }, { "R_ELBO", 0 }, { "L_ELBO", 0 }, { "L_SHOU", -90 } };
            var servoMapList = new List<IDictionary<String, Int32>>()
            {
                new Dictionary<String, Int32>() { {"HEAD_R", -14},  {"BODY_Y", -1 }, {"HEAD_P",  -1},  {"R_ELBO",   0},  {"L_ELBO", -59}},
                new Dictionary<String, Int32>() { {"HEAD_R",  22},  {"BODY_Y", -1 }, {"HEAD_P",  -1},  {"R_ELBO",  54},  {"L_ELBO",  21}},
                new Dictionary<String, Int32>() { {"HEAD_R",  -1},  {"BODY_Y",  0 }, {"HEAD_P",  20},  {"R_ELBO",  54},  {"L_ELBO", -43}},
                new Dictionary<String, Int32>() { {"HEAD_R",  -1},  {"BODY_Y", -1 }, {"HEAD_P",  -7},  {"R_ELBO", -35},  {"L_ELBO",  39}},
                new Dictionary<String, Int32>() { {"HEAD_R", -22},  {"BODY_Y", -1 }, {"HEAD_P", -20},  {"R_ELBO",  10},  {"L_ELBO",   4}},
                new Dictionary<String, Int32>() { {"HEAD_R",   6},  {"BODY_Y",  0 }, {"HEAD_P", -20},  {"R_ELBO", -24},  {"L_ELBO",  43}},
                new Dictionary<String, Int32>() { {"HEAD_R",   6},  {"BODY_Y", -1 }, {"HEAD_P",  -1},  {"R_ELBO",  40},  {"L_ELBO", -25}},
                new Dictionary<String, Int32>() { {"HEAD_R",   6},  {"BODY_Y",  3 }, {"HEAD_P",  -1},  {"R_ELBO",  -2},  {"L_ELBO",   1}}
            };
            var beatMotion = RobotOP.MakeBeatMotion(5000, servoMapList, endServoMap, 1.0);
            RobotOP.PlayMotion(ip, port, beatMotion);
            Thread.Sleep(5000);


            Console.WriteLine("wavファイル（Debugフォルダにある）を再生する。");
            Console.WriteLine("さらに10秒のビートジェスチャを組み合わせる。");
            RobotOP.PlayWav(ip, port, "sample.wav");
            beatMotion = RobotOP.MakeBeatMotion(10000, servoMapList, endServoMap, 1.5);
            RobotOP.PlayMotion(ip, port, beatMotion);
            Thread.Sleep(10000);

        }


        static void TestForSota()
        {
            String ip = "10.10.12.174";
            Int32 port = 22222;

            Console.WriteLine("現在の間接角度を取得する。");
            var axes = RobotOP.ReadAxes(ip, port);
            foreach (String key in axes.Keys)
            {
                Console.WriteLine("Axis[{0}]={1}", key, axes[key]);
            }
            Thread.Sleep(2000);


            Console.WriteLine("1秒間で左手を上げる。");
            var servoMap = new Dictionary<String, Int32>() { { "L_SHOU", 0 } };
            var pose = new Pose { Msec = 1000, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(2000);


            Console.WriteLine("0.5秒間で左手を下ろし、右手を上げ、反時計回りに30度回転する。");
            servoMap = new Dictionary<String, Int32>()
            {
                { "L_SHOU", -90 },
                { "R_SHOU",   0 },
                { "BODY_Y",  30 }
            };
            pose = new Pose { Msec = 500, ServoMap = servoMap };
            RobotOP.PlayPose(ip, port, pose);
            Thread.Sleep(2000);


            Console.WriteLine("モーション（ポーズのリスト）を実行する。");
            var nodMotion = new List<Pose>()
            {
                new Pose { Msec = 250, ServoMap = new Dictionary<String, Int32>() { { "R_SHOU", 105 }, { "HEAD_P", -15 }, { "R_ELBO",  0 }, { "L_ELBO",  -3 }, { "L_SHOU", -102 } } },
                new Pose { Msec = 250, ServoMap = new Dictionary<String, Int32>() { { "R_SHOU",  77 }, { "HEAD_P",  20 }, { "R_ELBO", 17 }, { "L_ELBO", -17 }, { "L_SHOU",  -79 } } },
                new Pose { Msec = 250, ServoMap = new Dictionary<String, Int32>() { { "R_SHOU",  92 }, { "HEAD_P",  -5 }, { "R_ELBO",  5 }, { "L_ELBO",   5 }, { "L_SHOU",  -88 } } },
            };
            RobotOP.PlayMotion(ip, port, nodMotion);
            Thread.Sleep(2000);


            Console.WriteLine("モーションを0.25秒後に止める。");
            RobotOP.PlayMotion(ip, port, nodMotion);
            Thread.Sleep(250);
            RobotOP.StopMotion(ip, port);
            Thread.Sleep(2000);


            Console.WriteLine("アイドル動作を実行する（スピード1.0, 間隔1000ミリ秒）。");
            RobotOP.PlayIdleMotion(ip, port, 1.0, 1000);
            Thread.Sleep(10000);


            Console.WriteLine("アイドル動作を停止する。");
            RobotOP.StopIdleMotion(ip, port);


            Console.WriteLine("5秒のビートジェスチャ（スピード1.0）を生成し実行する。");
            var endServoMap = new Dictionary<String, Int32>() { { "R_SHOU", 90 }, { "R_ELBO", 0 }, { "L_ELBO", 0 }, { "L_SHOU", -90 } };
            var servoMapList = new List<IDictionary<String, Int32>>()
            {
                new Dictionary<String, Int32>() { {"R_SHOU", 59}, {"R_ELBO", 23}, {"L_ELBO", -21}, {"L_SHOU", -63}},
                new Dictionary<String, Int32>() { {"R_SHOU", 32}, {"R_ELBO", 84}, {"L_ELBO", -80}, {"L_SHOU", -16}},
                new Dictionary<String, Int32>() { {"R_SHOU", 15}, {"R_ELBO", 84}, {"L_ELBO", -76}, {"L_SHOU", -40}},
                new Dictionary<String, Int32>() { {"R_SHOU", 57}, {"R_ELBO", 20}, {"L_ELBO", -80}, {"L_SHOU", -46}},
                new Dictionary<String, Int32>() { {"R_SHOU", 29}, {"R_ELBO", 92}, {"L_ELBO", -36}, {"L_SHOU", -74}},
                new Dictionary<String, Int32>() { {"R_SHOU", 75}, {"R_ELBO", 30}, {"L_ELBO", -31}, {"L_SHOU", -79}}
            };
            var beatMotion = RobotOP.MakeBeatMotion(5000, servoMapList, endServoMap, 1.0);
            RobotOP.PlayMotion(ip, port, beatMotion);
            Thread.Sleep(5000);


            Console.WriteLine("wavファイル（Debugフォルダにある）を再生する。");
            Console.WriteLine("さらに10秒のビートジェスチャを組み合わせる。");
            RobotOP.PlayWav(ip, port, "sample.wav");
            beatMotion = RobotOP.MakeBeatMotion(10000, servoMapList, endServoMap, 1.5);
            RobotOP.PlayMotion(ip, port, beatMotion);
            Thread.Sleep(10000);

        }

    }
}
