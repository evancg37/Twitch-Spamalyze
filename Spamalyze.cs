using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using System.Threading;

namespace TwitchAnalysis
{

    class Program
    {
        private const string DEFAULT_CHANNEL_TO_OBSERVE = "loltyler1";
        private static bool DEBUG_VIEW_ALL_MESSAGES = false;
        private const int LOOP_TIME = 30;
        private const int REFRESH_TIME = 200;
        private const int MAX_MESSAGES = 100;
        private static string channelName;
        private static Queue<ChatMessage> archive;

        static bool viewMessages = DEBUG_VIEW_ALL_MESSAGES;

        static void Main(string[] args)
        {

            ConnectionCredentials credentials = new ConnectionCredentials("_username_", "_secret_");
            TwitchClient client;
            archive = new Queue<ChatMessage>(MAX_MESSAGES);

            bool loopRunning = true;

            do//Menu loop
            {
                Console.WriteLine("\n\n\nEnter a channel name to connect to:\n");
                channelName = Console.ReadLine();
                Console.Clear();

                client = new TwitchClient(credentials, channelName);
                client.OnJoinedChannel += clientJoinedChannel;
                client.OnMessageReceived += ReceiveMessage;
                client.OnConnected += clientConnected;

                Console.WriteLine("\n\nConnecting to " + channelName + "...");
                client.Connect();

                int loopCount = 0;
                float emoteAverage = -1;
                int count;

                while (loopRunning)
                {
                    Thread.Sleep(REFRESH_TIME);
                    Console.Clear();
                    count = archive.Count();
                    Console.WriteLine("Messages in archive: {0}/{1}", count, MAX_MESSAGES);

                    var emotesList = GetEmotesList();
                    SortEmotesList(ref emotesList);
                    PrintList(emotesList);
                }

                /**
                while (loopRunning && !Console.KeyAvailable)
                {
                    Console.Clear();
                    Console.WriteLine("Messages in archive: {0}", archive.Count());
                    Console.WriteLine("Time until next update: {0}\n", LOOP_TIME/2 - (loopCount / 2));
                    Thread.Sleep(500);
                    loopCount++;
                    if (loopCount > LOOP_TIME)
                    {
                        viewMessages = false;
                        Thread.Sleep(200);
                        Console.Clear();

                        var emotesList = GetEmotesList();
                        SortEmotesList(ref emotesList);
                        PrintList(emotesList);
                        if (emotesList[0].Item2 > emoteAverage)
                        {
                            Console.WriteLine("\n\nThis was a significant {0} moment.", emotesList[0].Item1);
                        }
                        
                        if (emoteAverage == -1)
                            emoteAverage = emotesList[0].Item2; //Set average to first.

                        else
                            emoteAverage = (emoteAverage + emotesList[0].Item2) / 2f; //Otherwise update average.

                        archive.Clear();
                        Thread.Sleep(4000);
                        loopCount = 0;
                        viewMessages = DEBUG_VIEW_ALL_MESSAGES;
                    }
                   
                }
                **/

                Thread.Sleep(500); Console.Clear();

                loopRunning = false;


                client.OnJoinedChannel -= clientJoinedChannel;
                client.OnMessageReceived -= ReceiveMessage;
                client.OnConnected -= clientConnected;

            } while (loopRunning);

            Console.ReadLine();

        }

        static void SortEmotesList(ref Tuple<string, int>[] referenceList)
        {
            int j = referenceList.Count();
            int a;

            Tuple<string, int>[] list = new Tuple<string, int>[j];

            referenceList.CopyTo(list, 0);
            Tuple<string, int> max;

            Tuple<string, int>[] result = new Tuple<string, int>[j];

            for (int i = 0; i < j; i++) //Starting with the first element in the list,
            {
                max = new Tuple<string, int>("Nan", -1);
                a = -1;

                for (int x = 0; x < j; x++) //for every element, 
                {
                    if (list[x].Item2 > max.Item2)
                    {
                        max = list[x];
                        a = x;
                    }
                }

                result[i] = max;
                list.SetValue(new Tuple<string, int>("Nope", -2), a);
            }

            referenceList = result;
        }

        static void PrintList(Tuple<string, int>[] list)
        {
            Console.WriteLine("Emotes list contents:\n");
            for (int i = 0; i < list.Count(); i++)
            {
                if (! (list[i].Item2 == 0))
                    Console.WriteLine("{0}: {1}", list[i].Item1, list[i].Item2);
            }
        }

        static Tuple<string, int>[] GetEmotesList()
        {
            Tuple<string, int>[] allemotes = new Tuple<string, int>[ChatInformation.EMOTE_COUNT];
            for (int i = 0; i < ChatInformation.EMOTE_COUNT; i++)
            {
                allemotes[i] = new Tuple<string, int>(ChatInformation.emotes[i], 0); //instantiate emote array
            }

            int j = archive.Count();
            ChatMessage[] temp = archive.ToArray();

            for (int i = 0; i < j; i++) //For every message in the archive
            {
                ChatMessage chat = temp[i];
                string emote = chat.emote;

                for (int x = 0; x < ChatInformation.EMOTE_COUNT; x++) //For every emote
                {
                    string current = allemotes[x].Item1;
                    int count = allemotes[x].Item2;

                    if (current == emote)
                    {
                        allemotes[x] = new Tuple<string, int>(current, count + temp[i].emoteQuantity); //Count each emote
                    }
                }
            }

            return allemotes;
        }

        static void clientConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine("Connected to chat.");
        }

        static void clientJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Connected to channel.");
        }

        static void ReceiveMessage(object sender, OnMessageReceivedArgs e)
        {
            DateTime currentTime = DateTime.Now;

            if (viewMessages)
            {
                Console.WriteLine("{0}: {1}", e.ChatMessage.Username, e.ChatMessage.Message);
            }

            ChatMessage msg = new ChatMessage(e.ChatMessage.Message, e.ChatMessage.Username);

            int i = archive.Count();

            if (i >= MAX_MESSAGES)
            {
                archive.Dequeue();
            }

            archive.Enqueue(msg);
        }

    }

    public static class ChatInformation
    {
        public static string[] EMOTES = {"OMEGALUL", "KappaPride", "Kappa", "TriHard", "monkaS", "PogChamp", "LuL", "BibleThump", "cmonBruh", "FeelsBadMan", "HeyGuys", "haHAA",
                                                "ResidentSleeper", "POGGERS", "SMOrc", "4Head", "FeelsGoodMan", "Kreygasm", "KKona", "DansGame", "ANELE", "LUL", "BigBrother",
                                                "SwiftRage", "BlessRNG", "OhMyDog", "EleGiggle", "Jebaited", "BrokeBack", "PJSalt", "CoolStoryBob", "KappaRoss", "PepePls",
                                                "AngelThump", "WutFace", "EZ", "Clap", "D:",  "FeelsSadMan", "<3", "MingLee", "sneakyW", "sneakyEZ", "BabyRage", "SourPls",
                                                "TheIlluminati", "NotLikeThis" }; 

        public static List<string> emotes = new List<string>(EMOTES);

        public static int EMOTE_COUNT = emotes.Count();
    }


    public class ChatMessage
    {
        public string message;
        public string author;
        public string emote;
        public int emoteQuantity;

        public ChatMessage(string message, string author)
        {
            this.message = message;
            this.author = author;

            emote = FindFirstEmote(message);
            emoteQuantity = FindEmoteCount(message, emote);
        }

        public static string FindFirstEmote(string message)
        {
            return FindListItemInString(ChatInformation.emotes, message);
        }

        public static int FindEmoteCount(string message, string emote)
        {
            int j = message.Length;
            int i = 0;
            int count = 0;

            while (i < j){
                int index = message.IndexOf(emote, i); //Find the emote in the string
                if (index > -1)
                {
                    i = index + 1; //increment to avoid the same emote
                    count++; //Count that occurrence
                }
                else
                    break;
            }

            return count; //This is how many times we found the emote
        }

        public bool HasEmote()
        {
            if (this.emote.Equals(""))
            {
                return false;
            }

            return true;
        }

        public static string FindListItemInString(List<string> list, string message)
        {
            int j = list.Count();

            for (int i = 0; i < j; i++)
            {
                int g = message.IndexOf(list[i]);

                if (g > -1) //An emote has been found in the message at g
                {
                    return list[i];
                }
            }

            return "";
        }
    }
}
