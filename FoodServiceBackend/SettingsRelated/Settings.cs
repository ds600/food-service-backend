using Newtonsoft.Json;

namespace FoodServiceBackend.SettingsRelated
{
    public class Settings
    {
        public Settings() { }

        [JsonIgnore]
        public static string SettingsFileName { get { return "foodsettings.json"; } }
        public string NetworkInterface { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 10073;
        public int VoteEndHour { get; set; } = 10;
        public int VoteEndMinute { get; set; } = 12;
        public int OrderEndHour { get; set; } = 11;
        public int OrderEndMinute { get; set; } = 0;
        public string ManagerEmailAddress { get; set; } = string.Empty;
        public bool InstantlySkipToOrder { get; set; } = false;
        public string WinnerIfSkippingToOrder { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public List<string> EmailRecipients { get; set; } = new List<string>() { string.Empty, string.Empty };
        public Dictionary<string, int> VoteWinners { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> VotersAndParticipation { get; set; } = new Dictionary<string, int>();
        public List<Restaurant> Restaurants { get; set; } = new List<Restaurant>() { new Restaurant(), new Restaurant() };
        // Currently just to manually paste into the list, not used programmatically
        public List<string> AllPossibleEmailRecipients { get; set; } = new List<string>() { string.Empty, string.Empty };


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
