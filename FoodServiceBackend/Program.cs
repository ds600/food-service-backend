using FoodServiceBackend.Core;
using FoodServiceBackend.Email;
using FoodServiceBackend.SettingsRelated;
using FoodServiceBackend.Networking;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Newtonsoft.Json;

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
                JsonSerializerSettings serializationSettings = new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                string settingsJson = File.ReadAllText(Settings.SettingsFileName);
                settings = JsonConvert.DeserializeObject<Settings>(settingsJson, serializationSettings);
                originalSettings = JsonConvert.DeserializeObject<Settings>(settingsJson, serializationSettings);
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
                    File.WriteAllText(Settings.SettingsFileName, JsonConvert.SerializeObject(settings, Formatting.Indented));
                    Helper.WriteIntoConsoleAndLog(Settings.SettingsFileName + " was created.");
                }
                else
                {
                    return;
                }
            }

            // Add "Don't care" option, that gets ignored in the final calculation
            settings.Restaurants.Add(new Settings.Restaurant()
            {
                name = "Mir egal"
            });

            //Begin voting:
            DateTime placeVoteDueTime = DateTime.Now.Date + TimeSpan.FromHours(settings.VoteEndHour) +
                TimeSpan.FromMinutes(settings.VoteEndMinute);
            voteManager = new VoteManager(placeVoteDueTime);
            if (settings.InstantlySkipToOrder == false)
            {
                voteManager.BeginVoting();
            }
            else
            {
                voteManager.allVoters.Add(new Voter()
                {
                    EmailAddress = "AverageVoterAndy",
                    Token = "voter",
                    VoteOptionChosen = settings.WinnerIfSkippingToOrder
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
            foreach (Settings.Restaurant restaurant in settings.Restaurants)
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
                    if (originalSettings.VotersAndParticipation.ContainsKey(voter.EmailAddress))
                    {
                        originalSettings.VotersAndParticipation[voter.EmailAddress] += 1;
                    }
                    else
                    {
                        originalSettings.VotersAndParticipation.Add(voter.EmailAddress, 1);
                    }
                }
            }

            string winningRestaurantString = votingResult.OrderByDescending(x => x.Value.Count).Where(x=> x.Key != "Mir egal").FirstOrDefault().Key;
            Settings.Restaurant? winningRestaurant = settings.Restaurants?.Where(x => x.name == winningRestaurantString)?.FirstOrDefault();

            Helper.WriteIntoConsoleAndLog("Voting result:");
            Helper.WriteIntoConsoleAndLog(JsonConvert.SerializeObject(votingResult, Formatting.Indented));
            Helper.WriteIntoConsoleAndLog("");
            if (winningRestaurant != null)
            {
                Helper.WriteIntoConsoleAndLog("Restaurant with the most Votes: " + winningRestaurant.name);

                // Count up win amount for winning restaurant. Using original settings, so we don't have a "Don't care" option
                if (originalSettings.VoteWinners.ContainsKey(winningRestaurant.name))
                {
                    originalSettings.VoteWinners[winningRestaurant.name] += 1;
                }
                else
                {
                    originalSettings.VoteWinners.Add(winningRestaurant.name, 1);
                }
                File.WriteAllText(Settings.SettingsFileName, JsonConvert.SerializeObject(originalSettings, Formatting.Indented));
            }
            else
            {
                Helper.WriteIntoConsoleAndLog("Could not determine vote winner, mission abort");
                return;
            }

            // now users can place an order at the restaurant with the most votes:
            DateTime placeOrderDueTime = DateTime.Now.Date + TimeSpan.FromHours(settings.OrderEndHour) +
                TimeSpan.FromMinutes(settings.OrderEndMinute);
            orderManager = new OrderManager(winningRestaurant, placeOrderDueTime);
        }

        private static bool finishedOrderOnceAlready = false;

        public async static void FinishOrder()
        {
            if (orderManager == null || orderManager.Restaurant == null)
            {
                return; 
            }

            // If time is up, finish the order, logging it and writing everything into the console.
            Console.ForegroundColor = ConsoleColor.Green;
            string orderMessage = "Today we will order at " + orderManager.Restaurant.name + ".";
            string callMessage = "Call number " + orderManager.Restaurant.mobilePhoneNumber + ".";
            Helper.WriteIntoConsoleAndLog(orderMessage);
            Helper.WriteIntoConsoleAndLog(callMessage);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Helper.WriteIntoConsoleAndLog("Order list:");
            TextEncoderSettings encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
            Helper.WriteIntoConsoleAndLog(JsonConvert.SerializeObject(orderManager.OrderList, Formatting.Indented));

            string orderManagerMessage = orderMessage + "<br/>" + callMessage + "<br/><br/>";
            foreach(KeyValuePair<string, Order> order in orderManager.OrderList)
            {
                string writtenOrder = order.Key + " Ordered " + order.Value.OrderNumber + " with " + order.Value.AdditionalInformation + " for a price of " + order.Value.Price;
                Helper.WriteToLogFile(writtenOrder);                

                if (string.IsNullOrEmpty(order.Value.OrderNumber) == false)
                {
                    orderManagerMessage += writtenOrder + "<br/>";
                }
            }

            if (finishedOrderOnceAlready == false &&
                settings?.ManagerEmailAddress != null &&
                string.IsNullOrEmpty(settings.ManagerEmailAddress) == false)
            {
                finishedOrderOnceAlready = true;
                await EmailHelper.SendEmailAsync(orderManagerMessage, settings.ManagerEmailAddress,
                "FoodServiceBackend - OrdersFinished");
            }
        }
    }
}