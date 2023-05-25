using FoodServiceBackend.Core;
using FoodServiceBackend.Email;
using FoodServiceBackend.Networking.Webserver;
using FoodServiceBackend.SettingsRelated;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FoodServiceBackend.Networking;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace FoodServiceBackend
{
    public class Program
    {
        public static Settings? settings = null;
        public static Settings? originalSettings = null;
        public static VoteManager? voteManager = null;
        public static OrderManager? orderManager = null;
        public static Server? webserver;
        public static bool finishedVoting = false;

        static void Main(string[] args)
        {
            Helper.WriteIntoConsoleAndLog("Food Service!");
            Helper.WriteIntoConsoleAndLog("Are you hungry?");

            try
            {
                string settingsJson = File.ReadAllText(Settings.SettingsFileName);
                settings = JsonSerializer.Deserialize<Settings>(settingsJson);
                originalSettings = JsonSerializer.Deserialize<Settings>(settingsJson);
            }
            catch (Exception ex)
            {
                Helper.WriteIntoConsoleAndLog("Error deserializing " + Settings.SettingsFileName + " --> " + ex.Message);
            }

            if (settings == null)
            {
                Helper.WriteIntoConsoleAndLog("Create a new " + Settings.SettingsFileName + "? (Y/N)");
                string? response = Console.ReadLine();
                if (response != null && String.Equals(response.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
                {
                    settings = new Settings();
                    File.WriteAllText(Settings.SettingsFileName, JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true }));
                    Helper.WriteIntoConsoleAndLog(Settings.SettingsFileName + " was created.");
                }
                else
                {
                    return;
                }
            }

            // Add "Don't care" option, that gets ignored in the final calculation
            settings.restaurants.Add(new Settings.Restaurant()
            {
                name = "Mir egal"
            });
            //Begin voting:
            DateTime placeVoteDueTime = DateTime.Now.Date + TimeSpan.FromHours(10.2);
            voteManager = new VoteManager(placeVoteDueTime);
            if (settings.instantlySkipToOrder == false)
            {
                voteManager.BeginVoting();
            }
            else
            {
                voteManager.allVoters.Add(new Voter()
                {
                    EmailAddress = "AverageVoterAndy",
                    Token = "voter",
                    VoteOptionChosen = settings.winnerIfSkippingToOrder
                });
                voteManager.CreateVotersWithTokens();
                FinishVote();

                // Remove Average voter Andy again, he was only there to fake a voting result
                voteManager.allVoters.RemoveAt(0);
                Program.orderManager!.OrderList.Remove("AverageVoterAndy");
            }


            webserver = new Server();

            Helper.WriteIntoConsoleAndLog("Press CTRL + C to exit");
            while (true)
            {
                Console.ReadLine();
            }
        }

        public static void FinishVote()
        {
            if (finishedVoting == true || voteManager == null || settings == null || originalSettings == null)
            {
                return;
            }
            finishedVoting = true;
            // after voting time is up:
            Dictionary<string, List<string>> votingResult = new Dictionary<string, List<string>>();
            foreach (Settings.Restaurant restaurant in settings.restaurants)
            {
                if (votingResult.ContainsKey(restaurant.name) == false)
                {
                    votingResult.Add(restaurant.name, new List<string>());
                }
            }
            foreach (Voter voter in voteManager.allVoters)
            {
                if (votingResult.ContainsKey(voter.VoteOptionChosen))
                {
                    votingResult[voter.VoteOptionChosen].Add(voter.Token);

                    // Count up how often the users voted. Using original settings, so we don't have a "Don't care" option
                    if (originalSettings.votersAndParticipation.ContainsKey(voter.EmailAddress))
                    {
                        originalSettings.votersAndParticipation[voter.EmailAddress] += 1;
                    }
                    else
                    {
                        originalSettings.votersAndParticipation.Add(voter.EmailAddress, 1);
                    }
                }
            }

            string winningRestaurantString = votingResult.OrderByDescending(x => x.Value.Count).Where(x=> x.Key != "Mir egal").FirstOrDefault().Key;
            Settings.Restaurant? winningRestaurant = settings.restaurants?.Where(x => x.name == winningRestaurantString)?.FirstOrDefault();

            Helper.WriteIntoConsoleAndLog("Voting result:");
            Helper.WriteIntoConsoleAndLog(JsonSerializer.Serialize(
                votingResult, new JsonSerializerOptions() { WriteIndented = true }));
            Helper.WriteIntoConsoleAndLog("");
            if (winningRestaurant != null)
            {
                Helper.WriteIntoConsoleAndLog("Restaurant with the most Votes: " + winningRestaurant.name);

                // Count up win amount for winning restaurant. Using original settings, so we don't have a "Don't care" option
                if (originalSettings.voteWinners.ContainsKey(winningRestaurant.name))
                {
                    originalSettings.voteWinners[winningRestaurant.name] += 1;
                }
                else
                {
                    originalSettings.voteWinners.Add(winningRestaurant.name, 1);
                }
                File.WriteAllText(Settings.SettingsFileName, JsonSerializer.Serialize(originalSettings, new JsonSerializerOptions() { WriteIndented = true }));
            }
            else
            {
                Helper.WriteIntoConsoleAndLog("Could not determine vote winner, mission abort");
                return;
            }

            // now users can place an order at the restaurant with the most votes:
            DateTime placeOrderDueTime = DateTime.Now.Date + TimeSpan.FromHours(11);
            orderManager = new OrderManager(winningRestaurant, placeOrderDueTime);


        }

        public static void FinishOrder()
        {
            if (orderManager == null || orderManager.Restaurant == null)
            {
                return; 
            }

            // If time is up, finish the order, logging it and writing everything into the console.
            Console.ForegroundColor = ConsoleColor.Green;
            Helper.WriteIntoConsoleAndLog("Today we will order at " + orderManager.Restaurant.name + ".");
            Helper.WriteIntoConsoleAndLog("Call number " + orderManager.Restaurant.mobilePhoneNumber + ".");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Helper.WriteIntoConsoleAndLog("Order list:");
            TextEncoderSettings encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
            Helper.WriteIntoConsoleAndLog(JsonSerializer.Serialize(orderManager.OrderList, new JsonSerializerOptions() { WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            foreach(KeyValuePair<string, Order> order in orderManager.OrderList)
            {
                Helper.WriteToLogFile(order.Key + " Ordered " + order.Value.OrderNumber + " for a price of " + order.Value.Price);
            }
        }
    }
}