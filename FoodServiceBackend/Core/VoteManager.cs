using FoodServiceBackend.Email;
using FoodServiceBackend.SettingsRelated;
using System.Runtime.InteropServices;
using System.Timers;

namespace FoodServiceBackend.Core
{
    public class VoteManager
    {
        /// <summary>
        /// emailRecipient, GUID Token
        /// </summary>
        public List<Voter> allVoters { get; set; } = new List<Voter>();
        System.Timers.Timer checkForTime = new System.Timers.Timer(interval1Minute);

        public VoteManager(DateTime dueTime)
        {
            this.dueTime = dueTime;
            Helper.WriteIntoConsoleAndLog("Orders can be placed until " + dueTime.ToString());
        }

        private DateTime dueTime;
        public DateTime DueTime { get { return dueTime; } }

        public async void BeginVoting()
        {
            if (Program.settings == null)
            {
                return;
            }
            allVoters = new List<Voter>();

            foreach (string emailRecipient in Program.settings.EmailRecipients)
            {
                string votingToken = Guid.NewGuid().ToString();
                allVoters.Add(new Voter() {
                    EmailAddress = emailRecipient,
                    Token = votingToken,
                    VoteOptionChosen = string.Empty
                });
                string body = "It's " + System.DateTime.Now.DayOfWeek.ToString() + " again! <br />" +
                    "You may vote to order food at the following restaurants:<br />";
                foreach (Settings.Restaurant restaurant in Program.settings.Restaurants)
                {
                    body += restaurant.name + "<br />";
                }

                string link = "http://" + Program.settings.NetworkInterface + ":" + Program.settings.Port + "/vote/" + votingToken;
                body += "<br/>Use the following link: <a href=\"" + link + "\">" + link + "</a>";
                body += "<br/><br/>";
                body += "You can see all menus here: <br/>";
                string linkMenus = "http://" + Program.settings.NetworkInterface + ":" + Program.settings.Port + "/menus";
                body += "<a href =\"" + linkMenus + "\">" + linkMenus + "</a>";
                await EmailHelper.SendEmailAsync(body, emailRecipient, "FoodServiceBackend - VoteForFood");
            }

            checkForTime.Elapsed += new ElapsedEventHandler(CheckForTimeElapsed);
            checkForTime.Enabled = true;
        }

        /// <summary>
        /// Creates Voters and their tokens, only use this if you want to skip the actual voting process
        /// </summary>
        public void CreateVotersWithTokens()
        {
            if (Program.settings == null)
            {
                return;
            }
            foreach (string emailRecipient in Program.settings.EmailRecipients)
            {
                string votingToken = Guid.NewGuid().ToString();
                allVoters.Add(new Voter()
                {
                    EmailAddress = emailRecipient,
                    Token = votingToken,
                    VoteOptionChosen = string.Empty
                });
            }
        }

        const double interval1Minute = 60 * 1 * 1000; 

        void CheckForTimeElapsed(object? sender, ElapsedEventArgs e)
        {
            if (DateTime.Now > dueTime)
            {
                Program.FinishVote();
                if (Program.finishedVoting == true)
                {
                    checkForTime.Stop();
                }
            }
        }

        public bool TryPlaceVote(string token, string restaurant)
        {
            if ((DateTime.Now -TimeSpan.FromDays(1)) < dueTime && Program.settings != null)
            {
                lock (allVoters)
                {
                    if (allVoters.Where(x => x.Token == token).Any())
                    {
                        if (Program.settings.Restaurants.Where(x => x.name == restaurant).Any())
                        {
                            allVoters.Where(x => x.Token == token).FirstOrDefault()!.VoteOptionChosen = restaurant;
                            Helper.WriteToLogFile(allVoters.Where(x => x.Token == token).FirstOrDefault()!.EmailAddress + " Voted on " + restaurant);
                        }
                        return true;
                    }
                }
            }

            return false;
        }


    }
}
