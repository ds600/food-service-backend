using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.SettingsRelated
{
    public class Settings
    {
        public Settings() { }

        [JsonIgnore]
        public static string SettingsFileName { get { return "foodsettings.json"; } }
        public string networkInterface { get; set; } = "127.0.0.1";
        public int port { get; set; } = 10073;
        public bool instantlySkipToOrder { get; set; } = false;
        public string winnerIfSkippingToOrder { get; set; } = string.Empty;
        public string senderEmail { get; set; } = string.Empty;
        public string senderPassword { get; set; } = string.Empty;
        public List<string> emailRecipients { get; set; } = new List<string>() { string.Empty, string.Empty };
        public Dictionary<string, int> voteWinners { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> votersAndParticipation { get; set; } = new Dictionary<string, int>();
        public List<Restaurant> restaurants { get; set; } = new List<Restaurant>() { new Restaurant(), new Restaurant() };
        // Currently just to manually paste into the list, not used programmatically
        public List<string> allPossibleEmailRecipients { get; set; } = new List<string>() { string.Empty, string.Empty };


        public class Restaurant
        {
            public Restaurant() { }
            public Restaurant(string name, string mobilePhoneNumber, List<string> foodMenuPageScans)
            {
                this.name = name;
                this.mobilePhoneNumber = mobilePhoneNumber;
                this.foodMenuPageScans = foodMenuPageScans;
            }

            public string name { get; set; } = string.Empty;
            public string mobilePhoneNumber { get; set; } = string.Empty;
            public List<string> foodMenuPageScans { get; set; } = new List<string>();
        }
    }
}
