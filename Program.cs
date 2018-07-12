using System;

using ServiceStack.Redis;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using ServiceStack.Common;

namespace ramdomNumber
{
    class Program
    {
        //X and Y axis ranges
        private static int xLowRange = 0;
        private static int xHighRange = 0;
        private static int yLowRange = 0;
        private static int yHighRange = 0;
        //Flags to stop StartX and StartY methods
        private static bool stopFlagX = false;
        private static bool stopFlagY = false;
        //Channel Names
        private const string ChannelXH = "xHigh";
        private const string ChannelXL = "xLow";
        private const string ChannelYH = "yHigh";
        private const string ChannelYL = "yLow";
        private const string ChannelX = "x";
        private const string ChannelY = "y";
        private const string ChannelStartX = "startX";
        private const string ChannelStopX = "stopX";
        private const string ChannelStartY = "startY";
        private const string ChannelStopY = "stopY";
        static void Main(string[] args)
        {


            using (var redisConsumer = new RedisClient("localhost"))
            using (var subscription = redisConsumer.CreateSubscription())
            {
                subscription.OnSubscribe = channel =>
                {
                    Console.WriteLine("Subscribed to '{0}'", channel);
                };
                subscription.OnUnSubscribe = channel =>
                {
                    Console.WriteLine("UnSubscribed from '{0}'", channel);
                };
                //This event is executed every time a message is received on a channel
                subscription.OnMessage = (channel, msg) =>
                {
                    Console.WriteLine("Received '{0}' from channel '{1}'", msg, channel);
                    switch (channel)
                    {
                        case "xHigh":
                            xHighRange = Int32.Parse(msg);
                            Console.WriteLine(xHighRange);
                            break;
                        case "xLow":
                            xLowRange = Int32.Parse(msg);
                            Console.WriteLine(xLowRange);
                            break;
                        case "yLow":
                            yLowRange = Int32.Parse(msg);
                            Console.WriteLine(yLowRange);
                            break;
                        case "yHigh":
                            yHighRange = Int32.Parse(msg);
                            Console.WriteLine(yHighRange);
                            break;
                        case "startX":
                            StartX();
                            break;
                        case "startY":
                            StartY();
                            break;
                        case "stopX":
                            stopFlagX = true;
                            break;
                        case "stopY":
                            stopFlagY = true;
                            break;
                        default:
                            Console.WriteLine("No existe");
                            break;
                    }
                };

                //Console.WriteLine("Started Listening On '{0}'", ChannelName);
                subscription.SubscribeToChannels(ChannelXH,ChannelXL,ChannelYH,ChannelYL,ChannelX,ChannelY,ChannelStartX,ChannelStopX,ChannelStartY,ChannelStopY); //blocking
            }

            Console.WriteLine("EOF");
        }
        public static void StartX(){
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (var redisPublisher = new RedisClient("localhost")){
                    stopFlagX = false;
                    while (!stopFlagX)
                    {
                        Thread.Sleep(1000);
                        redisPublisher.PublishMessage(ChannelX, GenerateX().ToString());
                    }
                }

            });
        }
        public static void StartY(){
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (var redisPublisher = new RedisClient("localhost")){
                    stopFlagY = false;
                    while (!stopFlagY)
                    {
                        Thread.Sleep(1000);
                        redisPublisher.PublishMessage(ChannelY, GenerateY().ToString());
                    }
                }

            });
        }
        //Method to generate a random number for Y Axis
        public static int GenerateY(){
            Random r = new Random();
            int rInt = r.Next(yLowRange,yHighRange);
            return rInt;
        }
        //Method to generate a random number for X Axis
        public static int GenerateX(){
            Random r = new Random();
            int rInt = r.Next(xLowRange,xHighRange);
            return rInt;
        }
    }
}
