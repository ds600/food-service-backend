using FoodServiceBackend.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FoodServiceBackend.Networking.Webserver
{
    public static class Post
    {
        public static void POST(object? sender, HttpRequestEventArgs e)
        {
            string rootpath = Environment.CurrentDirectory + "/Networking/Webserver/";
            var req = e.Request;
            var res = e.Response;

            var path = req.RawUrl;
            Webserver.ApiResponse? response = null;

            System.Net.IPEndPoint Client = req.RemoteEndPoint;

            if (path == "/")
                path = "index.html";
            else
            {
                if (path[0] == '/')
                {
                    path = path.Substring(1, path.Length - 1);
                }
            }

            byte[] contents = new byte[0];

            if (path.Contains("sendvote"))
            {
                string jsonString = Encoding.UTF8.GetString(Server.ReadFully(req.InputStream));
                VotePost? votePost = JsonConvert.DeserializeObject<VotePost>(jsonString);
                if (votePost != null && Program.voteManager != null)
                {
                    if (Program.voteManager.TryPlaceVote(votePost.UserId, votePost.VoteResult))
                    {
                        // If everyone voted
                        if (Program.voteManager.allVoters.Where(x => x.VoteOptionChosen == "").Any() == false)
                        {
                            Program.FinishVote();
                        }
                        response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.OK,
                            "Successfully registered Vote", JsonSerializer.Serialize(votePost.VoteResult));
                    }
                    else
                    {
                        response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.ExpectationFailed,
                            "Voter-ID or restaurant not found", JsonSerializer.Serialize(votePost.VoteResult));
                    }

                }
            }
            else if (path.Contains("sendorder"))
            {
                string jsonString = Encoding.UTF8.GetString(Server.ReadFully(req.InputStream));
                Order? orderPost = JsonConvert.DeserializeObject<Order>(jsonString);

                if (orderPost != null && Program.orderManager != null && Program.voteManager != null)
                {
                    string? emailAddress = Program.voteManager.allVoters.Where(x => x.Token == orderPost.Token).Select(x => x.EmailAddress).FirstOrDefault();
                    if (emailAddress != null && Program.orderManager.TryPlaceOrder(emailAddress, orderPost))
                    {
                        // If everyone ordered
                        if (Program.orderManager.OrderList.Where(x => x.Value.OrderNumber == "").Any() == false)
                        {
                            Program.FinishOrder();
                        }
                        response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.OK,
                            "Successfully registered Vote", JsonSerializer.Serialize(emailAddress));
                    }
                    else
                    {
                        response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.ExpectationFailed,
                            "Order-ID not found", JsonSerializer.Serialize(""));
                    }
                }
                else
                {
                    response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.ExpectationFailed,
                        "Order object doesn't have the right format", JsonSerializer.Serialize(""));
                }
            }

            byte[] byteResponse = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes("EMPTY");

            if (response != null)
            {
                byteResponse = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(response);

                contents = byteResponse;
                res.ContentType = "application/json";
                res.ContentEncoding = Encoding.UTF8;
                res.StatusCode = (int)response.HttpStatusCode;
            }

            try
            {
                res.ContentLength64 = contents.LongLength;
                res.Close(contents, true);
            }
            catch(Exception ex)
            {
                Helper.WriteIntoConsoleAndLog(ex.Message);
            }
        }
    }
}
