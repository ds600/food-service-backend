using FoodServiceBackend.SettingsRelated;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodServiceBackend.Email;
using Newtonsoft.Json.Linq;

namespace FoodServiceBackend.Core
{
    public class OrderManager
    {
        public OrderManager(Settings.Restaurant restaurant, DateTime dueTime)
        {
            if (Program.voteManager == null)
            {
                return;
            }

            foreach (Voter voter in Program.voteManager.allVoters)
            {
                if (orderList.ContainsKey(voter.EmailAddress) == false)
                {
                    orderList.Add(voter.EmailAddress, new Order());
                }
                
            }
            this.restaurant = restaurant;
            this.dueTime = dueTime;
            Helper.WriteIntoConsoleAndLog("Orders can be placed for " + restaurant.name + " until " + dueTime.ToString());


            System.Timers.Timer checkForTime = new System.Timers.Timer(interval5Minutes);
            checkForTime.Elapsed += new ElapsedEventHandler(CheckForTimeElapsed);
            checkForTime.Enabled = true;

            BeginOrdering();
        }

        async void BeginOrdering()
        {
            if (Program.settings == null || Program.voteManager == null)
            {
                return;
            }
            foreach (string emailRecipient in Program.settings.emailRecipients)
            {
                string? votingToken = Program.voteManager.allVoters.Where(x => x.EmailAddress == emailRecipient).Select(x => x.Token).FirstOrDefault();
 
                if (votingToken != null)
                {
                    string body = "The vote time is up, you can place your order now! :) <br/>";

                    string link = "http://" + Program.settings.networkInterface + ":" + Program.settings.port + "/order/" + votingToken;
                    body += "<br /><br />Use the following link: <a href=\"" + link + "\">" + link + "</a>";

                    await EmailHelper.SendEmailAsync(body, emailRecipient, "FoodServiceBackend - OrderYourFood");
                }
            }
        }

        const double interval5Minutes = 60 * 5 * 1000; // milliseconds to one hour

        void CheckForTimeElapsed(object? sender, ElapsedEventArgs e)
        {
            if (DateTime.Now > dueTime)
            {
                Program.FinishOrder();
            }
        }

        private DateTime dueTime;
        public DateTime DueTime { get { return dueTime; } }

        private Settings.Restaurant? restaurant;
        public Settings.Restaurant? Restaurant { get { return restaurant; } }


        private Dictionary<string, Order> orderList = new Dictionary<string, Order>();

        public Dictionary<string, Order> OrderList { get { return orderList; } }

        public bool TryPlaceOrder(string emailRecipient, Order order)
        {
            if (orderList.ContainsKey(emailRecipient))
            {
                orderList[emailRecipient] = order;
                Helper.WriteToLogFile(emailRecipient + " Ordered " + order.OrderNumber + " with *" + order.AdditionInformation + "* for" + order.Price);
                return true;
            }

            return false;
        }

    }
}
