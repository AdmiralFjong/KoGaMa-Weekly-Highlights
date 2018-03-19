//limit version - special highlights edition
//created by CaptainJens
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Timers;
using System.Text.RegularExpressions;
using System.Text;

namespace Koggy
{
    class Bot
    {
        public HttpClient Client { get; set; } = new HttpClient();
        public bool EnableLogging = false;
        public string Server { get; private set; }
        public ConsoleColor DebugColor = ConsoleColor.DarkCyan;

        public enum ServerType
        {
            WWW,
            Friends,
            BR
        }

        public enum ItemType
        {
            Model,
            Avatar
        }

        public void SetServer(ServerType Server)
        {
            try
            {
                if (Server == ServerType.WWW)
                {
                    this.Client.BaseAddress = new Uri("http://www.kogama.com");
                    this.Server = "WWW";
                }
                else if (Server == ServerType.BR)
                {
                    this.Client.BaseAddress = new Uri("http://kogama.com.br");
                    this.Server = "BR";
                }
                else if (Server == ServerType.Friends)
                {
                    this.Client.BaseAddress = new Uri("http://friends.kogama.com");
                    this.Server = "Friends";
                }
            }
            catch (Exception e)
            {
                Log("ERROR", e.Message);
            }
        }

        private void Log(string LogType, string Message)
        {
            try
            {
                if (LogType.ToLower() == "error")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(LogType + ":");
                    Console.ResetColor();
                    Console.WriteLine(Message + "\n");
                }
                else if (EnableLogging)
                {
                    Console.ForegroundColor = this.DebugColor;
                    Console.WriteLine(LogType + ":");
                    Console.ResetColor();
                    Console.WriteLine(Message + "\n");
                }
            }
            catch (Exception e)
            {
                Log("ERROR", e.Message);
            }
        }

        public class User
        {
            public string Name;
            public string ID;
            public Bot Bot = new Bot();
        }

        public class Item
        {
            public string Description;
            public User Author = new User();
            public string ID;
            public string Name;
            public string CategoryID;
            public string Category;
            public string Price;
            public string Solds;
            public string Likes;
            public bool OwnsModel = false;
            public string Created;
            public Bot Bot = new Bot();
            public Images Images = new Images();
        }

        public class Game
        {
            public List<User> Members = new List<User>();
            public string Description;
            public int PlayCount;
            public int Likes;
            public int PlayingNow;
            public string LastPublished;
            public string Type;
            public string ID;
            public string Name;
            public Images Images = new Images();
        }

        public class Images
        {
            public string Small;
            public string Medium;
            public string Large;
        }

        public async Task<Game> GetGame(string GameID)
        {
            try
            {
                HttpResponseMessage response = await this.Client.GetAsync("/game/" + GameID + "/published/");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                var DataObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(await response.Content.ReadAsStringAsync());
                var Game = new Game()
                {
                    Description = DataObject["data"]["description"],
                    ID = DataObject["data"]["id"],
                    LastPublished = DataObject["data"]["published"],
                    Likes = DataObject["data"]["likes"],
                    Name = DataObject["data"]["name"],
                    PlayCount = DataObject["data"]["played"],
                    PlayingNow = DataObject["data"]["playing_now"],
                    Type = DataObject["data"]["game_type"]
                };
                Game.Images.Small = DataObject["data"]["images"]["small"];
                Game.Images.Medium = DataObject["data"]["images"]["medium"];
                Game.Images.Large = DataObject["data"]["images"]["large"];
                foreach (dynamic Member in DataObject["data"]["owners"])
                {
                    Game.Members.Add(new User()
                    {
                        Bot = this,
                        Name = Member["member_username"],
                        ID = Member["member_user_id"]
                    });
                }
                return Game;
            }
            catch (Exception e)
            {
                Log("ERROR", e.Message);
                return null;
            }
        }

        public async Task<Item> GetItem(ItemType Type, string ItemID)
        {
            try
            {
                Item Item = new Item();
                HttpResponseMessage response = await this.Client.GetAsync("/model/market/" + ((Type == ItemType.Avatar) ? "a-" : "i-") + ItemID);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                var DataObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(await response.Content.ReadAsStringAsync());
                var ItemData = DataObject["data"];

                Item.Author.Name = ItemData["creator"];
                Item.Author.ID = ItemData["author_profile_id"];
                Item.Author.Bot = this;
                Item.Bot = this;
                Item.Category = ItemData["category"];
                Item.CategoryID = ItemData["item_category_id"];
                Item.Created = ItemData["created"];
                Item.Description = ItemData["description"] ?? "";
                Item.ID = ItemData["id"];
                Item.Likes = ItemData["likes_count"];
                Item.Name = ItemData["name"];
                Item.OwnsModel = ItemData["owning"];
                Item.Price = ItemData["price_gold"];
                Item.Solds = ItemData["sold_count"];
                Item.Images.Small = ItemData["images"]["small"];
                Item.Images.Medium = ItemData["images"]["large"];
                Item.Images.Large = ItemData["images"]["image_path_url"];
                return Item;
            }
            catch (Exception e)
            {
                Log("ERROR", e.Message);
                return null;
            }
        }
    }
}
